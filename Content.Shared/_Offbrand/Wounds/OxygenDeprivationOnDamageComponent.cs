using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Offbrand.Wounds;

/// <summary>
/// Component that causes rapid oxygen loss when body damage reaches a certain threshold
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(OxygenDeprivationOnDamageSystem))]
public sealed partial class OxygenDeprivationOnDamageComponent : Component
{
    /// <summary>
    /// The damage threshold at which oxygen deprivation begins
    /// </summary>
    [DataField(required: true)]
    public FixedPoint2 DamageThreshold = 100;

    /// <summary>
    /// How much oxygen is lost per second when above the threshold
    /// </summary>
    [DataField(required: true)]
    public FixedPoint2 OxygenLossPerSecond = 10;

    /// <summary>
    /// How frequently to apply oxygen loss (in seconds)
    /// </summary>
    [DataField]
    public float UpdateInterval = 1f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan? LastUpdate;
}
