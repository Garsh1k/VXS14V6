using Robust.Shared.GameStates;

namespace Content.Shared.GG.CapturePoint;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public partial class CaptureTicketsComponent : Component
{
    [AutoNetworkedField]
    public int SyndyTickets;

    [AutoNetworkedField]
    public int SolfedTickets;
}
