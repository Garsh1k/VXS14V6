using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class OxygenDeprivationOnDamageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BrainDamageSystem _brainDamage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OxygenDeprivationOnDamageComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<OxygenDeprivationOnDamageComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnMapInit(Entity<OxygenDeprivationOnDamageComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastUpdate = _timing.CurTime;
    }

    private void OnDamageChanged(Entity<OxygenDeprivationOnDamageComponent> ent, ref DamageChangedEvent args)
    {
        // Check if the entity has a brain damage component to affect
        if (!HasComp<BrainDamageComponent>(ent))
            return;

        // Check if total damage exceeds threshold
        if (args.Damageable.TotalDamage >= ent.Comp.DamageThreshold)
        {
            ApplyOxygenDeprivation(ent);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OxygenDeprivationOnDamageComponent>();
        while (query.MoveNext(out var uid, out var oxygenDeprivation))
        {
            // Check if the entity has a brain damage component to affect
            if (!HasComp<BrainDamageComponent>(uid))
                continue;

            // Check if it's time to update
            if (oxygenDeprivation.LastUpdate is not { } last ||
                last + TimeSpan.FromSeconds(oxygenDeprivation.UpdateInterval) >= _timing.CurTime)
                continue;

            oxygenDeprivation.LastUpdate = _timing.CurTime;
            Dirty(uid, oxygenDeprivation);

            // Get the damageable component to check damage threshold
            if (TryComp<DamageableComponent>(uid, out var damageable) &&
                damageable.TotalDamage >= oxygenDeprivation.DamageThreshold)
            {
                ApplyOxygenDeprivation((uid, oxygenDeprivation));
            }
        }
    }

    private void ApplyOxygenDeprivation(Entity<OxygenDeprivationOnDamageComponent> ent)
    {
        // Calculate oxygen loss based on update interval
        var oxygenLoss = ent.Comp.OxygenLossPerSecond * ent.Comp.UpdateInterval;

        // Apply oxygen loss to brain
        _brainDamage.TryChangeBrainOxygenation(ent.Owner, -oxygenLoss);
    }
}
