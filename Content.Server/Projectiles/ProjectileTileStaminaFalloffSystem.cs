using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Map;

namespace Content.Server.Projectiles;

public sealed class ProjectileTileStaminaFalloffSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Run before SharedStaminaSystem so that Damage is already reduced when TakeStaminaDamage is called.
        SubscribeLocalEvent<ProjectileTileStaminaFalloffComponent, ProjectileHitEvent>(OnProjectileHit,
            before: new[] { typeof(SharedStaminaSystem) });
    }

    private void OnProjectileHit(Entity<ProjectileTileStaminaFalloffComponent> ent, ref ProjectileHitEvent args)
    {
        if (ent.Comp.StartCoordinates == null || ent.Comp.DamageReductionPerTile <= 0f)
            return;

        var xform = Transform(ent);
        var currentMapCoords = _transform.ToMapCoordinates(xform.Coordinates);
        var startMapCoords = ent.Comp.StartCoordinates.Value;

        if (startMapCoords.MapId != currentMapCoords.MapId)
            return;

        var distance = (currentMapCoords.Position - startMapCoords.Position).Length();
        var traveledTiles = (int) MathF.Floor(distance);

        if (traveledTiles <= 0)
            return;

        if (!TryComp<StaminaDamageOnCollideComponent>(ent, out var stamina))
            return;

        var reduction = ent.Comp.DamageReductionPerTile * traveledTiles;
        stamina.Damage = MathF.Max(0f, stamina.Damage - reduction);
    }
}
