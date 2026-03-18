using Robust.Shared.GameStates;

namespace Content.Shared._VXS.Manpads.Components;

[RegisterComponent]
public sealed partial class VXSManpadsAutoDefenseComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float FireInterval = 3f;

    [DataField]
    public float ScanInterval = 0.4f;

    [DataField]
    public float ScanRadius = 300f;

    [ViewVariables]
    public TimeSpan NextScan;

    [ViewVariables]
    public TimeSpan NextFire;
}
