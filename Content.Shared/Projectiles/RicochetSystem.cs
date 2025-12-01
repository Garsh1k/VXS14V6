using System.Numerics;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Physics;
using Content.Shared.Inventory.Events;
using Content.Shared.Hands;

namespace Content.Shared.Projectiles;

/// <summary>
/// This handles ricocheting projectiles based on angle of impact.
/// </summary>
public abstract partial class SharedRicochetSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RicochetComponent, ProjectileReflectAttemptEvent>(OnRicochetAttempt);
        SubscribeLocalEvent<RicochetComponent, GotEquippedEvent>(OnRicochetEquipped);
        SubscribeLocalEvent<RicochetComponent, GotUnequippedEvent>(OnRicochetUnequipped);
        SubscribeLocalEvent<RicochetComponent, GotEquippedHandEvent>(OnRicochetHandEquipped);
        SubscribeLocalEvent<RicochetComponent, GotUnequippedHandEvent>(OnRicochetHandUnequipped);
    }

    private void OnRicochetAttempt(Entity<RicochetComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.InRightPlace)
            return; // only ricochet when equipped correctly

        if (TryRicochetProjectile(ent, ent.Owner, args.ProjUid))
            args.Cancelled = true;
    }

    private void OnRicochetEquipped(Entity<RicochetComponent> ent, ref GotEquippedEvent args)
    {
        ent.Comp.InRightPlace = (ent.Comp.SlotFlags & args.SlotFlags) == args.SlotFlags;
        Dirty(ent);
    }

    private void OnRicochetUnequipped(Entity<RicochetComponent> ent, ref GotUnequippedEvent args)
    {
        ent.Comp.InRightPlace = false;
        Dirty(ent);
    }

    private void OnRicochetHandEquipped(Entity<RicochetComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.InRightPlace = ent.Comp.RicochetingInHands;
        Dirty(ent);
    }

    private void OnRicochetHandUnequipped(Entity<RicochetComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.InRightPlace = false;
        Dirty(ent);
    }

    private bool TryRicochetProjectile(Entity<RicochetComponent> ricocheter, EntityUid user, Entity<ProjectileComponent?> projectile)
    {
        if (!_random.Prob(ricocheter.Comp.RicochetProb) ||
            !TryComp<PhysicsComponent>(projectile, out var physics))
        {
            return false;
        }

        // Calculate the angle of incidence
        var projectileVelocity = _physics.GetMapLinearVelocity(projectile, component: physics);
        var surfaceNormal = GetSurfaceNormal(projectile, user); // Simplified normal calculation

        // Calculate angle between projectile velocity and surface normal
        var angle = CalculateAngleBetweenVectors(projectileVelocity, surfaceNormal);

        // Convert to degrees for comparison
        var angleDegrees = Math.Abs(angle * 180 / Math.PI);

        // Check if angle is within ricochet range
        // We want angles that are neither too shallow nor too steep
        if (angleDegrees < ricocheter.Comp.MinRicochetAngle || angleDegrees > ricocheter.Comp.MaxRicochetAngle)
        {
            return false; // Angle not suitable for ricochet
        }

        // Perform the ricochet
        var rotation = _random.NextAngle(-Angle.FromDegrees(ricocheter.Comp.Spread) / 2, Angle.FromDegrees(ricocheter.Comp.Spread) / 2);
        var existingVelocity = _physics.GetMapLinearVelocity(projectile, component: physics);
        var relativeVelocity = existingVelocity - _physics.GetMapLinearVelocity(user);

        // Calculate reflection vector based on surface normal
        var reflectedVelocity = ReflectVector(existingVelocity, surfaceNormal);

        // Apply spread/randomization
        var newVelocity = rotation.RotateVec(reflectedVelocity);

        // Have the velocity in world terms above so need to convert it back to local.
        var difference = newVelocity - existingVelocity;

        _physics.SetLinearVelocity(projectile, physics.LinearVelocity + difference, body: physics);

        var locRot = Transform(projectile).LocalRotation;
        var newRot = rotation.RotateVec(locRot.ToVec());
        _transform.SetLocalRotation(projectile, newRot.ToAngle());

        PlayAudioAndPopup(ricocheter.Comp, user);

        // Check if the projectile component is valid before accessing its properties
        if (Resolve(projectile, ref projectile.Comp, false))
        {
            _adminLogger.Add(LogType.BulletHit, LogImpact.Medium, $"{ToPrettyString(user)} ricocheted {ToPrettyString(projectile)} from {ToPrettyString(projectile.Comp.Weapon)} shot by {projectile.Comp.Shooter}");

            projectile.Comp.Shooter = user;
            projectile.Comp.Weapon = user;
            Dirty(projectile, projectile.Comp);
        }
        else
        {
            _adminLogger.Add(LogType.BulletHit, LogImpact.Medium, $"{ToPrettyString(user)} ricocheted {ToPrettyString(projectile)}");
        }

        return true;
    }

    /// <summary>
    /// Calculates a simplified surface normal based on the relative positions of the projectile and the entity.
    /// </summary>
    private Vector2 GetSurfaceNormal(Entity<ProjectileComponent?> projectile, EntityUid target)
    {
        if (!TryComp<TransformComponent>(projectile, out var projXform) ||
            !TryComp<TransformComponent>(target, out var targetXform))
        {
            return Vector2.UnitX; // Default fallback
        }

        // Simple approximation: vector from target to projectile
        var normal = projXform.WorldPosition - targetXform.WorldPosition;

        // Normalize the vector
        if (normal.Length() > 0)
            normal = Vector2.Normalize(normal);
        else
            normal = Vector2.UnitX;

        return normal;
    }

    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    private float CalculateAngleBetweenVectors(Vector2 vec1, Vector2 vec2)
    {
        var dot = Vector2.Dot(vec1, vec2);
        var mag1 = vec1.Length();
        var mag2 = vec2.Length();

        if (mag1 == 0 || mag2 == 0)
            return 0;

        var cosAngle = dot / (mag1 * mag2);
        // Clamp to avoid numerical errors
        cosAngle = Math.Clamp(cosAngle, -1.0f, 1.0f);

        return (float)Math.Acos(cosAngle);
    }

    /// <summary>
    /// Reflects a vector across a normal.
    /// </summary>
    private Vector2 ReflectVector(Vector2 incident, Vector2 normal)
    {
        var dotProduct = Vector2.Dot(incident, normal);
        return incident - 2 * dotProduct * normal;
    }

    private void PlayAudioAndPopup(RicochetComponent ricochet, EntityUid user)
    {
        // Can probably be changed for prediction
        if (_netManager.IsServer)
        {
            _popup.PopupEntity("Ricochet!", user);
            _audio.PlayPvs(ricochet.SoundOnRicochet, user);
        }
    }
}
