using Content.Shared.FixedPoint;
using Robust.Shared.Map;

namespace Content.Shared.Projectiles;

/// <summary>
/// Reduces one or more damage types by a fixed amount for each tile a projectile has traveled.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileTileDamageFalloffComponent : Component
{
    /// <summary>
    /// Maps damage type IDs (e.g. Piercing, Blunt) to the flat reduction applied per traveled tile.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> DamageReductions = new();

    /// <summary>
    /// Starting map coordinates saved when projectile shooter is assigned.
    /// Runtime-only.
    /// </summary>
    [ViewVariables]
    public MapCoordinates? StartCoordinates;
}
