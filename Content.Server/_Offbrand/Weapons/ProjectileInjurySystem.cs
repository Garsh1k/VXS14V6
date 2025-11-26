using Content.Shared._Offbrand.Weapons;
using Content.Shared.Projectiles;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._Offbrand.Weapons;

public sealed class ProjectileInjurySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly WoundableSystem _woundable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileInjuryComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(Entity<ProjectileInjuryComponent> ent, ref ProjectileHitEvent args)
    {
        // Check if the target is woundable
        if (!HasComp<WoundableComponent>(args.Target))
            return;

        // Get the projectile component to determine damage
        if (!TryComp<ProjectileComponent>(ent, out var projectile))
            return;

        // Calculate total damage
        var totalDamage = projectile.Damage.GetTotal();

        // Check if damage meets minimum requirement
        if (totalDamage < ent.Comp.MinimumDamage)
            return;

        // Calculate injury chance based on damage and caliber multiplier
        var injuryChance = ent.Comp.BaseInjuryChance * ent.Comp.CaliberMultiplier * (totalDamage.Float() / 10.0f);

        // Cap the chance at 95% to prevent guaranteed injuries
        injuryChance = Math.Min(injuryChance, 0.95f);

        // Roll for injury
        if (!_random.Prob(injuryChance))
            return;

        // Determine wound type based on highest damage type
        var woundType = ent.Comp.WoundType;
        if (woundType == null)
        {
            woundType = DetermineWoundType(projectile.Damage);
        }

        // Inflict the wound
        if (woundType != null)
        {
            var target = new Entity<WoundableComponent>(args.Target, Comp<WoundableComponent>(args.Target));
            _woundable.TryWound(target, woundType.Value, damage: new(projectile.Damage), refreshDamage: true);
        }
    }

    private EntProtoId? DetermineWoundType(DamageSpecifier damage)
    {
        if (damage.DamageDict.Count == 0)
            return null;

        // Get the damage type with the highest value
        var maxDamage = damage.DamageDict.MaxBy(kvp => kvp.Value);

        // Map damage types to appropriate wound types
        var woundId = maxDamage.Key switch
        {
            "Piercing" => "WoundPunctureModerate",
            "Slash" => "WoundCutModerate",
            "Blunt" => "WoundBruise",
            "Heat" => "WoundHeatModerate",
            "Cold" => "WoundColdModerate",
            "Caustic" => "WoundCausticModerate",
            "Shock" => "WoundShockModerate",
            "Radiation" => "WoundCausticModerate", // Radiation damage causes caustic-like wounds
            _ => "WoundBruise" // Default to bruise for unknown types
        };

        return new EntProtoId(woundId);
    }
}
