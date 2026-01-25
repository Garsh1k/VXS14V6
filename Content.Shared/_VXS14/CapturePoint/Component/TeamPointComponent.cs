using Content.Shared.Antag;
using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.GG.CapturePoint;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GGSyndyTeamComponent : Component
{
    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "SyndiFaction";

    [DataField, AutoNetworkedField]
    public int Current;

    [DataField, AutoNetworkedField]
    public int Max;

    public override bool SessionSpecific => true;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GGSolfedTeamComponent : Component
{
    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "SolfedFaction";

    [AutoNetworkedField]
    public int Current;

    [AutoNetworkedField]
    public int Max;

    public override bool SessionSpecific => true;
}

