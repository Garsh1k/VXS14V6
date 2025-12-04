using Robust.Shared.GameStates;

namespace Content.Shared.GG.CapturePoint;

[RegisterComponent,NetworkedComponent]

public partial class GGCapturePointComponent : Component
{

    [ViewVariables(VVAccess.ReadWrite)][DataField] public string PointName = "G";
    [DataField] public string Leader = "None";

    private float PointProgression = 100f;

    [ViewVariables(VVAccess.ReadWrite)][DataField] public float CurrentPointProgression = 0f;

    public Color PointColor = Color.White;
    public HashSet<EntityUid> CollidingEntities = new();

    public EntityUid PointUid;

    public string? Team;

    public bool Captured = false;

}
