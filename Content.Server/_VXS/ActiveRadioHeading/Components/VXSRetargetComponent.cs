namespace Content.Server._VXS.ActiveRadioHeading.Components;

[RegisterComponent]
public sealed partial class VXSRetargetComponent: Component
{
    [DataField("chanceToRetarget")]
    public float ChanceToRetarget = 0.6f;
}
