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

    [DataField("impactEntity"), AutoNetworkedField]
    public string? ImpactEntity;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Dropped;
}
