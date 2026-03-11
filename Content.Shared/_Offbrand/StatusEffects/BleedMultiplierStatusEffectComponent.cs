namespace Content.Shared._Offbrand.StatusEffects;

[RegisterComponent]
[Access(typeof(BleedMultiplierStatusEffectSystem))]
public sealed partial class BleedMultiplierStatusEffectComponent : Component
{
    /// <summary>
    /// Factor applied to incoming bleed level changes.
    /// </summary>
    [DataField(required: true)]
    public float Multiplier;

    /// <summary>
    /// Amount that was immediately applied/removed when the status effect was added.
    /// Stored so we can revert it when the effect ends.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float StoredBleedChange;
}
