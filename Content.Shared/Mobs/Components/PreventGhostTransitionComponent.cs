using Robust.Shared.GameStates;

namespace Content.Shared.Mobs.Components;

/// <summary>
/// Added to mobs in critical state to prevent them from transitioning to ghost state
/// until their brain fully dies.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreventGhostTransitionComponent : Component
{
}
