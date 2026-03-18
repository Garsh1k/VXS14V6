using Robust.Shared.Map;

namespace Content.Shared._VXS.Manpads.Components;

[RegisterComponent]
public sealed partial class VXSManpadsProjectileTransferComponent : Component
{
    [DataField]
    public float TransferSpeed = 450f;

    [DataField]
    public float TransferDistance = 1200f;

    [DataField]
    public float MaxTransferDelay = 0.35f;

    [ViewVariables]
    public bool PendingTransfer;

    [ViewVariables]
    public TimeSpan TransferAt;

    [ViewVariables]
    public MapCoordinates Destination;

    [ViewVariables]
    public EntityUid? Target;
}
