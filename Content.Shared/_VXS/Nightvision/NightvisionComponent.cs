using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._VXS.Nightvision;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightvisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Amplification factor for light sources when night vision is active.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("lightAmplification"), AutoNetworkedField]
    public float LightAmplification { get; set; } = 1.0f;
}

[RegisterComponent]
[NetworkedComponent]
public sealed partial class NightvisionClothingComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Amplification factor for light sources when night vision goggles are equipped.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("lightAmplification")]
    public float LightAmplification { get; set; } = 10.0f; // 10x amplification for VX-NVG
}
