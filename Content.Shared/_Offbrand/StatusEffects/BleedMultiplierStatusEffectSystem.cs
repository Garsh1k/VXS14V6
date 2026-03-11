using Content.Shared._Offbrand.Wounds;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffect; // for StatusEffectAddedEvent / StatusEffectEndedEvent
using Content.Shared.Body.Systems; // for SharedBloodstreamSystem
using Content.Shared.Body.Components; // for BloodstreamComponent

namespace Content.Shared._Offbrand.StatusEffects;

public sealed class BleedMultiplierStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();

        // used for ongoing bleed events (wounds, etc.)
        SubscribeLocalEvent<BleedMultiplierStatusEffectComponent, StatusEffectRelayedEvent<ModifyBleedLevelEvent>>(OnGetBleedMultiplier);

        // react to the effect being added/removed in order to apply an instant adjustment
        SubscribeLocalEvent<BleedMultiplierStatusEffectComponent, StatusEffectAddedEvent>(OnEffectAdded);
        SubscribeLocalEvent<BleedMultiplierStatusEffectComponent, StatusEffectEndedEvent>(OnEffectEnded);
    }

    private void OnGetBleedMultiplier(Entity<BleedMultiplierStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ModifyBleedLevelEvent> args)
    {
        args.Args = args.Args with { BleedLevel = args.Args.BleedLevel * ent.Comp.Multiplier };
    }

    private void OnEffectAdded(EntityUid uid, BleedMultiplierStatusEffectComponent comp, StatusEffectAddedEvent args)
    {
        // when the status effect is applied we immediately scale the current bleed amount
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        var original = bloodstream.BleedAmount;
        var target = original * comp.Multiplier;

        // clamp ourselves so we can remember the exact amount we applied;
        // calling TryModifyBleedAmount may also clamp, and we want to be able
        // to restore back to whatever the value was prior to the effect.
        var clamped = Math.Clamp(target, 0f, bloodstream.MaxBleedAmount);
        var appliedChange = clamped - original;

        // store for restoration later
        comp.StoredBleedChange = appliedChange;
        if (appliedChange != 0f)
        {
            _bloodstream.TryModifyBleedAmount((uid, bloodstream), appliedChange);
        }
    }

    private void OnEffectEnded(EntityUid uid, BleedMultiplierStatusEffectComponent comp, StatusEffectEndedEvent args)
    {
        // when the effect ends, reverse the previous instant change
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        var change = comp.StoredBleedChange;
        if (change != 0f)
        {
            // negate what we added earlier
            _bloodstream.TryModifyBleedAmount((uid, bloodstream), -change);
            comp.StoredBleedChange = 0f;
        }
    }
}
