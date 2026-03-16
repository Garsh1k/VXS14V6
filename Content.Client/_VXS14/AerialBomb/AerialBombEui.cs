using Content.Client.Eui;
using Content.Shared._VXS14.AerialBomb;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client._VXS14.AerialBomb;

[UsedImplicitly]
public sealed class AerialBombEui : BaseEui
{
    private readonly AerialBombWindow _window;

    public AerialBombEui()
    {
        _window = new AerialBombWindow();
        _window.SetEui(this);
        _window.OnClose += SendClosedMessage;
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.OnClose -= SendClosedMessage;
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);

        if (state is AerialBombEuiState config)
            _window.SetMaps(config.Maps);
    }

    public void SendDropRequest(NetEntity mapEntity)
    {
        SendMessage(new AerialBombEuiMsg.DropRequest(mapEntity));
    }

    private void SendClosedMessage()
    {
        SendMessage(new CloseEuiMessage());
    }
}
