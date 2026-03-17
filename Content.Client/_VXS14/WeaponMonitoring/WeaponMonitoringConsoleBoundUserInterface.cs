using Content.Shared._VXS14.WeaponMonitoring;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._VXS14.WeaponMonitoring;

public sealed class WeaponMonitoringConsoleBoundUserInterface : BoundUserInterface
{
    private WeaponMonitoringConsoleWindow? _window;

    public WeaponMonitoringConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new WeaponMonitoringConsoleWindow(Owner);
        _window.OnRefreshRequested += () => SendMessage(new RequestWeaponMonitoringRefreshMessage());
        _window.OnControlActionRequested += (action, entity) => SendMessage(new WeaponMonitoringControlActionMessage(entity, action));
        _window.OnClose += Close;

        var uiSys = EntMan.System<UserInterfaceSystem>();
        if (uiSys.TryGetPosition(Owner, UiKey, out var pos))
            _window.Open(pos);
        else
            _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not WeaponMonitoringConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Close();
    }
}
