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
    /// Additional armor modifiers provided by the inserted plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> PlateModifiers = new();
}
