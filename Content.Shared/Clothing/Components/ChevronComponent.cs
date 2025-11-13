using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// This component represents a chevron that can be attached to clothing items.
/// </summary>
[RegisterComponent]
public sealed partial class ChevronComponent : Component
{
    /// <summary>
    /// The chevron information to display when the wearer is examined.
    /// </summary>
    [DataField(required: true)]
    public string ChevronInfo = string.Empty;

    /// <summary>
    /// Optional: Custom examination text that replaces the default.
    /// </summary>
    [DataField]
    public string? CustomExamineText;
}

/// <summary>
/// This component allows clothing items to have chevron slots for attaching chevrons.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ChevronSlotComponent : Component
{
    /// <summary>
    /// The maximum number of chevrons that can be attached to this clothing item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxChevrons = 1;

    /// <summary>
    /// The list of currently attached chevron prototype IDs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> AttachedChevronPrototypes = new();
}
