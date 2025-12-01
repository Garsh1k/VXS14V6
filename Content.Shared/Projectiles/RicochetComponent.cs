using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Content.Shared.Inventory;

namespace Content.Shared.Projectiles;

/// <summary>
/// Entities with this component can ricochet projectiles based on the angle of impact.
/// The projectile will only be deflected if the angle of incidence is within the specified range.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RicochetComponent : Component
{
    /// <summary>
    /// Minimum angle (in degrees) from perpendicular at which a projectile can ricochet.
    /// Projectiles hitting at angles more shallow than this will not ricochet.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinRicochetAngle = 1f;

    /// <summary>
    /// Maximum angle (in degrees) from perpendicular at which a projectile can ricochet.
    /// Projectiles hitting at angles steeper than this will not ricochet.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxRicochetAngle = 20f;

    /// <summary>
    /// Probability for a projectile to be ricocheted when the angle is appropriate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RicochetProb = 0.75f;

    /// <summary>
    /// Spread angle (in degrees) for the ricocheted projectile.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Spread = 15f;

    /// <summary>
    /// The sound to play when ricocheting.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundOnRicochet = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg");

    /// <summary>
    /// Select in which inventory slots it will ricochet.
    /// By default, it will ricochet in any inventory position, except pockets.
    /// </summary>
    [DataField]
    public SlotFlags SlotFlags = SlotFlags.WITHOUT_POCKET;

    /// <summary>
    /// Is it allowed to ricochet while being in hands.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RicochetingInHands = true;

    /// <summary>
    /// Can only ricochet when placed correctly.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool InRightPlace;
}
