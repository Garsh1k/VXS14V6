using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.Trigger.Components.Conditions;

/// <summary>
/// Prevents trigger activation until the entity is at least the configured distance away from its initial spawn point.
/// Distance is measured in tiles.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MinimumDistanceTriggerConditionComponent : BaseTriggerConditionComponent
{
    /// <summary>
    /// Minimum distance from the initial spawn point in tiles before trigger is allowed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinimumDistanceTiles = 4f;

    /// <summary>
    /// Initial map-space spawn position captured on map init.
    /// </summary>
    [ViewVariables]
    public Vector2? StartPosition;

    /// <summary>
    /// Map of the captured spawn position.
    /// </summary>
    [ViewVariables]
    public MapId? StartMap;
}
