using System.Numerics;

namespace Content.Server._VXS.ActiveRadioHeading.Components;

[RegisterComponent]
public sealed partial class VXSActiveThrusterRadioHeadingComponent : Component
{
    [DataField]
    public float SeekRange = 300f;

    [DataField]
    public Angle WeaponArc = Angle.FromDegrees(360);

    [DataField]
    public Angle? RotationSpeed = 100f;

    [DataField]
    public GuidanceType GuidanceAlgorithm = GuidanceType.PredictiveGuidance;

    [DataField]
    public EntityUid? TargetEntity;

    [DataField]
    public float Acceleration = 50f;

    [DataField]
    public float TopSpeed = 50f;

    [DataField]
    public float InitialSpeed = 10f;

    [DataField]
    public float Speed;

    [DataField]
    public float FOV = 90f;

    [DataField]
    public TimeSpan RetargetWindow = TimeSpan.FromSeconds(10);

    public float OldDistance;

    public Vector2 OldPosition;
}
