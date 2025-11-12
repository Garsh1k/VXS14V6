using Content.Client.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared._VXS14.Mortar;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Client._VXS14.Mortar;

[UsedImplicitly]
public sealed class MortarEui : BaseEui
{
    [Dependency] private readonly EntityManager _entManager = default!;

    private readonly MortarWindow _window;
    private MapCoordinates _mapCords;

    public MortarEui()
    {
        IoCManager.InjectDependencies(this);
        _window = new MortarWindow();
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

    public void SendClosedMessage()
    {
        SendMessage(new CloseEuiMessage());
    }

    public void SendCords(Vector2 offset)
    {
        SendMessage(new MortarSpawnExplosionEuiMsg.MortarCords(offset.X, offset.Y));
    }
}
