using Content.Shared.Damage;

namespace Content.Shared.Projectiles;

/// <summary>
/// Raised before a projectile applies damage to a target.
/// Used to pass projectile information (like TraumaCoefficient) to damage systems.
/// </summary>
[ByRefEvent]
public record struct ProjectileDamageEvent(
    EntityUid Projectile,
    ProjectileComponent ProjectileComponent,
    EntityUid Target,
    DamageSpecifier Damage,
    EntityUid? Shooter);
