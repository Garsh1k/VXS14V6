using Content.Server._VXS.ActiveRadioHeading.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.EntitySystems;

namespace Content.Server._VXS.ActiveRadioHeading.Systems;

public sealed class VXSTargetLockSignallerSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _link = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<VXSTargetLockSignallerComponent, VXSTargetLockedEvent>(OnTargetLocked);
    }

    private void OnTargetLocked(Entity<VXSTargetLockSignallerComponent> ent, ref VXSTargetLockedEvent args)
    {
        if (!ent.Comp.Enabled)
            return;
        if (!this.IsPowered(ent, EntityManager))
            return;
        _link.InvokePort(ent, ent.Comp.Port);
    }
}
