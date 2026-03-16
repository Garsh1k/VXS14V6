namespace Content.Server._VXS.ActiveRadioHeading.Components;

[RegisterComponent]
public sealed partial class VXSRetargetThrusterComponent: Component
{
    [DataField("chanceToRetarget")]
    public float ChanceToRetarget = 0.6f;
}
