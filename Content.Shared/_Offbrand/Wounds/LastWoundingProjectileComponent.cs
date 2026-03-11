using Robust.Shared.GameStates;

namespace Content.Shared._Offbrand.Wounds;

/// <summary>
/// Temporary component used to store the last projectile that hit an entity.
/// This is used to apply projectile-specific trauma coefficients to wounds.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LastWoundingProjectileComponent : Component
{
    /// <summary>
    /// Reference to the projectile that last hit this entity.
    /// </summary>
    [DataField]
    public EntityUid? ProjectileEntity;

    /// <summary>
    /// The trauma coefficient from the projectile.
    /// </summary>
    [DataField]
    public double TraumaCoefficient = 1.0;
}
