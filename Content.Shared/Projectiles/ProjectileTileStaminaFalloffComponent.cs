using Robust.Shared.Map;

namespace Content.Shared.Projectiles;

/// <summary>
/// Reduces stamina damage applied on collision by a fixed amount for each tile the projectile has traveled.
/// Requires <see cref="Content.Shared.Damage.Components.StaminaDamageOnCollideComponent"/> on the same entity.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileTileStaminaFalloffComponent : Component
{
    /// <summary>
    /// Flat stamina damage reduction applied per tile traveled.
    /// </summary>
    [DataField(required: true)]
    public float DamageReductionPerTile = 0f;

    /// <summary>
    /// Starting map coordinates saved when the projectile shooter is assigned.
    /// Runtime-only.
    /// </summary>
    [ViewVariables]
    public MapCoordinates? StartCoordinates;
}
