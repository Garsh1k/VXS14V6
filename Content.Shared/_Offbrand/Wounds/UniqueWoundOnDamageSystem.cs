using Content.Shared.Damage;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class UniqueWoundOnDamageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly WoundableSystem _woundable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UniqueWoundOnDamageComponent, DamageChangedEvent>(OnDamageChanged, after: [typeof(WoundableSystem)]);
    }

    private void OnDamageChanged(Entity<UniqueWoundOnDamageComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is not { } delta || !args.DamageIncreased)
            return;

        var seed = SharedRandomExtensions.HashCodeCombine(new() { (int)_timing.CurTick.Value, GetNetEntity(ent).Id });
        var rand = new System.Random(seed);

        var damageable = Comp<DamageableComponent>(ent);
        var woundable = Comp<WoundableComponent>(ent);

        // Check if this entity was hit by a projectile and has a trauma coefficient
        var traumaCoefficient = 1.0;
        if (TryComp<LastWoundingProjectileComponent>(ent, out var lastProj))
        {
            traumaCoefficient = lastProj.TraumaCoefficient;
        }

        foreach (var wound in ent.Comp.Wounds)
        {
            var incomingAmount = ThresholdHelpers.Count(wound.DamageTypes, delta);
            var totalAmount = ThresholdHelpers.Count(wound.DamageTypes, damageable.Damage);

            if (incomingAmount < wound.MinimumDamage || totalAmount < wound.MinimumTotalDamage)
                continue;

            var baseProbability = wound.DamageProbabilityCoefficient * incomingAmount.Double() + wound.DamageProbabilityConstant;
            var probability = baseProbability * traumaCoefficient;
            if (!rand.Prob(probability))
                continue;

            _woundable.TryWound((ent.Owner, woundable), wound.WoundPrototype, wound.WoundDamages, unique: true, refreshDamage: true);
        }

        // Clear the last projectile component after processing
        RemComp<LastWoundingProjectileComponent>(ent);
    }
}
