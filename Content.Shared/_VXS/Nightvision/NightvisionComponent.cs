using Content.Shared.Eye.Blinding.Components;
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

    /// <summary>
    /// Tracks whether the light is currently too intense and may cause eye damage.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("lightTooIntense"), AutoNetworkedField]
    public bool LightTooIntense { get; set; } = false;

    /// <summary>
    /// Timer for tracking how long the light has been too intense.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("intenseLightTimer"), AutoNetworkedField]
    public float IntenseLightTimer { get; set; } = 0f;
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

    /// <summary>
    /// Threshold at which light becomes too intense and may cause eye damage.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("intenseLightThreshold")]
    public float IntenseLightThreshold { get; set; } = 5.0f;
}
