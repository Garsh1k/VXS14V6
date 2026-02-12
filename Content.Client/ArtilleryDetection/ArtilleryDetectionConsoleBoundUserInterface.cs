using Content.Shared.ArtilleryDetection;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.ArtilleryDetection;

public sealed class ArtilleryDetectionConsoleBoundUserInterface : BoundUserInterface
{
    private ArtilleryDetectionConsoleWindow? _window;
    private readonly ISawmill _sawmill = Logger.GetSawmill("artdet.client");

    public ArtilleryDetectionConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        _sawmill.Info($"[Artillery Console BUI] Open() called for entity {Owner}");

        base.Open();
        _sawmill.Info($"[Artillery Console BUI] base.Open() completed");

        try
        {
            _sawmill.Info("[Artillery Console BUI] Creating window...");
            _window = new ArtilleryDetectionConsoleWindow(Owner);
            _sawmill.Info("[Artillery Console BUI] Window created");

            _sawmill.Info("[Artillery Console BUI] Setting up event handlers...");
            _window.OnDeleteEvent += eventId => SendMessage(new DeleteArtilleryFireEventMessage(eventId));
            _window.OnRefreshRequested += () => SendMessage(new RequestArtilleryFireEventsMessage());
            _window.OnClose += Close;
            _sawmill.Info("[Artillery Console BUI] Event handlers attached");

            var uiSys = EntMan.System<UserInterfaceSystem>();
            if (uiSys.TryGetPosition(Owner, UiKey, out var pos))
            {
                _sawmill.Info($"[Artillery Console BUI] Opening at saved position: {pos}");
                _window.Open(pos);
            }
            else
            {
                _sawmill.Info("[Artillery Console BUI] Opening centered");
                _window.OpenCentered();
            }

            _sawmill.Info("[Artillery Console BUI] Window opened successfully");
        }
        catch (Exception ex)
        {
            _sawmill.Error($"[Artillery Console BUI] Exception in Open(): {ex}");
            throw;
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        _sawmill.Info($"Bound UI UpdateState called for {Owner}, state type: {state?.GetType().Name}");

        if (state is not ArtilleryDetectionConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Close();
    }
}
