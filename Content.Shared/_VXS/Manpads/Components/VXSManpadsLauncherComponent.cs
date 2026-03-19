using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._VXS.Manpads.Components;

[RegisterComponent]
public sealed partial class VXSManpadsLauncherComponent : Component
{
    [DataField]
    public float ScanRadius = 220f;

    [DataField]
    public float ScanInterval = 0.5f;

    [DataField]
    public SoundSpecifier LockSound = new SoundPathSpecifier("/Audio/Machines/beep.ogg");

    [DataField]
    public float LockSoundCooldown = 1.2f;

    [DataField]
    public HashSet<VXSManpadsIffType> FriendlyTransponders = new() { VXSManpadsIffType.SF };

    [DataField]
    public string? PreferredSpaceMapName;

    [ViewVariables]
    public EntityUid? Holder;

    [ViewVariables]
    public EntityUid? CurrentTarget;

    [ViewVariables]
    public TimeSpan NextScan;

    [ViewVariables]
    public TimeSpan NextLockSound;
}
