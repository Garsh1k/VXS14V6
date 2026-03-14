using Content.Server._VXS.ActiveRadioHeading.Components;
using Content.Server.Power.EntitySystems;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Server._VXS.ActiveRadioHeading.Systems;

public sealed class VXSTargetLockSirenSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VXSTargetLockSirenComponent, VXSTargetLockedEvent>(OnTargetLocked);
    }

    private void OnTargetLocked(EntityUid uid, VXSTargetLockSirenComponent comp, VXSTargetLockedEvent args)
    {

        if (!comp.Enabled || !this.IsPowered(uid, EntityManager))
            return;

        _audio.PlayEntity(comp.SirenSound, Filter.Pvs(uid), uid, true);
    }
}
