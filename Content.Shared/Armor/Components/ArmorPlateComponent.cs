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
    /// The armor modifiers provided by this plate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, float> Modifiers = new();
}
