using Content.Shared.FixedPoint;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Player;

namespace Content.Server.GG.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class CapturePointGameRuleComponent : Component
{
    [DataField("pointsCap"), ViewVariables(VVAccess.ReadWrite)]
    public static int PointsCap = 500;

    public int SyndyTeamPoints = PointsCap;
    public int SoledTeamPoints = PointsCap;

    [DataField("playerFirstJob")]
    public Dictionary<ICommonSession, string> PlayerFirstJob = new();

    [DataField("victor")]
    public string? Victor;
}

