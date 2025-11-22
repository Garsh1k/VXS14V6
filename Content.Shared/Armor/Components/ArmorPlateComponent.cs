using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Armor.Components;

/// <summary>
/// Component for armor plates that can be inserted into modular armor.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArmorPlateComponent : Component
{
    /// <summary>
    /// Coefficient modifiers provided by this plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> Coefficients = new();

    /// <summary>
    /// Flat reduction modifiers provided by this plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> FlatReductions = new();

    /// <summary>
    /// Hard resistances that fully consume a set amount of damage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> HardResistances = new();

    /// <summary>
    /// Hard-spendable resistances that consume damage but lower by that amount.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> HardSpendableResistances = new();

    /// <summary>
    /// Hard-spendable-percent resistances that consume damage but lower by a percentage of the consumed damage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> HardSpendablePercentResistances = new();

    #region Visual States
    /// <summary>
    /// The sprite state used when the armor plate is in perfect condition.
    /// </summary>
    [DataField]
    public string NormalSpriteState = "normal";

    /// <summary>
    /// The sprite state used when the armor plate is damaged but still functional.
    /// </summary>
    [DataField]
    public string DamagedSpriteState = "damaged";

    /// <summary>
    /// The sprite state used when the armor plate is broken and provides no protection.
    /// </summary>
    [DataField]
    public string BrokenSpriteState = "broken";

    /// <summary>
    /// Threshold percentage at which the plate changes to damaged state.
    /// When spendable resistances drop below this percentage of their maximum, the plate becomes damaged.
    /// </summary>
    [DataField]
    public float DamagedThreshold = 0.5f; // 50% of maximum resistance

    /// <summary>
    /// Threshold percentage at which the plate changes to broken state.
    /// When spendable resistances drop below this percentage of their maximum, the plate becomes broken.
    /// </summary>
    [DataField]
    public float BrokenThreshold = 0.1f; // 10% of maximum resistance
    #endregion
}

[Serializable, NetSerializable]
public enum ArmorPlateVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum ArmorPlateState : byte
{
    Normal,
    Damaged,
    Broken
}
