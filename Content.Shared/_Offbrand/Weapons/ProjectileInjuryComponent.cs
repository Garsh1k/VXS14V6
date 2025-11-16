using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.Weapons;

/// &lt;summary&gt;
/// Component that allows projectiles to inflict injuries on hit based on caliber.
/// &lt;/summary&gt;
[RegisterComponent]
public sealed partial class ProjectileInjuryComponent : Component
{
    /// &lt;summary&gt;
    /// Base chance to inflict an injury. This will be modified by caliber.
    /// &lt;/summary&gt;
    [DataField]
    public float BaseInjuryChance = 0.3f;

    /// &lt;summary&gt;
    /// Caliber multiplier for injury chance. Larger calibers have higher multipliers.
    /// &lt;/summary&gt;
    [DataField]
    public float CaliberMultiplier = 1.0f;

    /// &lt;summary&gt;
    /// Minimum damage required to have a chance of inflicting injury.
    /// &lt;/summary&gt;
    [DataField]
    public float MinimumDamage = 5.0f;

    /// &lt;summary&gt;
    /// Type of wound to inflict. If null, will be determined by damage type.
    /// &lt;/summary&gt;
    [DataField]
    public EntProtoId? WoundType;
}
