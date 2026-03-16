using Robust.Shared.GameObjects;

namespace Content.Shared._VXS14.AerialBomb;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class SharedAerialBombComponent : Component
{
    [DataField("flightTime"), AutoNetworkedField]
    public float FlightTime = 10f;

    [DataField("dispersionRadius"), AutoNetworkedField]
    public float DispersionRadius = 4f;

    [DataField("arrivalSound"), AutoNetworkedField]
    public string? ArrivalSound = "/Audio/Weapons/Guns/Artillery/mortarflyby.ogg";

    [DataField("preImpactDelay"), AutoNetworkedField]
    public float PreImpactDelay = 0.5f;

    [DataField("impactEntity"), AutoNetworkedField]
    public string? ImpactEntity;

    [DataField("signalTargetMapName"), AutoNetworkedField]
    public string? SignalTargetMapName;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Dropped;
}
