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
}
