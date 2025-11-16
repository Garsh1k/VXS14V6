using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Armor.Components;

/// <summary>
/// Component for armor that can have plates inserted to enhance protection.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModularArmorComponent : Component
{
    /// <summary>
    /// The item slot for inserting armor plates.
    /// </summary>
    [DataField]
    public ItemSlot PlateSlot = new();

    /// <summary>
    /// Coefficient modifiers provided by the inserted plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> PlateCoefficients = new();

    /// <summary>
    /// Flat reduction modifiers provided by the inserted plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> PlateFlatReductions = new();

    /// <summary>
    /// Hard resistances provided by the inserted plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> PlateHardResistances = new();

    /// <summary>
    /// Hard-spendable resistances provided by the inserted plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> PlateHardSpendableResistances = new();

    /// <summary>
    /// Hard-spendable-percent resistances provided by the inserted plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> PlateHardSpendablePercentResistances = new();
}