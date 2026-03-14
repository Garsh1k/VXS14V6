using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server._VXS.ActiveRadioHeading.Components;

[RegisterComponent]
public sealed partial class VXSTargetLockSignallerComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public ProtoId<SourcePortPrototype> Port = "VXSOnTargetLock";
}
