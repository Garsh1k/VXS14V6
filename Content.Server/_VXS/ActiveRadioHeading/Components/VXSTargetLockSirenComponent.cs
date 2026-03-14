using Robust.Shared.Audio;

namespace Content.Server._VXS.ActiveRadioHeading.Components;

[RegisterComponent]
public sealed partial class VXSTargetLockSirenComponent : Component
{
    [DataField("sirenSound")]
    public SoundSpecifier? SirenSound = new SoundPathSpecifier("/Audio/Ambience/Objects/alarm.ogg");

    [DataField]
    public bool Enabled = true;
}
