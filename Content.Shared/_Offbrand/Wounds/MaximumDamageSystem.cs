using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Offbrand.Wounds;

public sealed class MaximumDamageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MaximumDamageComponent, BeforeDamageCommitEvent>(OnBeforeDamageCommit, before: [typeof(WoundableSystem)]);
    }

    private void OnBeforeDamageCommit(Entity<MaximumDamageComponent> ent, ref BeforeDamageCommitEvent args)
    {
        // Damage downscaling has been removed as requested
        // The original scaling logic that reduced damage based on current health has been eliminated
        // All damage will now be applied at full value regardless of current damage state
        return;
    }
}
