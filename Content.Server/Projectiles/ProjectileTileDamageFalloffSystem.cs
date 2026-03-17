using Content.Shared.FixedPoint;
using Content.Shared.Projectiles;
using Robust.Shared.Map;

namespace Content.Server.Projectiles;

public sealed class ProjectileTileDamageFalloffSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileTileDamageFalloffComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(Entity<ProjectileTileDamageFalloffComponent> ent, ref ProjectileHitEvent args)
    {
        if (ent.Comp.StartCoordinates == null || ent.Comp.DamageReductions.Count == 0)
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

        foreach (var (damageType, reductionPerTile) in ent.Comp.DamageReductions)
        {
            if (reductionPerTile <= FixedPoint2.Zero)
                continue;

            if (!args.Damage.DamageDict.TryGetValue(damageType, out var damage) || damage <= FixedPoint2.Zero)
                continue;

            var reducedDamage = damage - reductionPerTile * traveledTiles;
            if (reducedDamage <= FixedPoint2.Zero)
                args.Damage.DamageDict.Remove(damageType);
            else
                args.Damage.DamageDict[damageType] = reducedDamage;
        }
    }
}
