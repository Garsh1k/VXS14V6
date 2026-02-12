using Content.Server.ArtilleryDetection.Systems;
using Content.Shared.ArtilleryDetection;
using Content.Shared.ArtilleryDetection.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork.Components;
using Robust.Server.GameObjects;
using Content.Shared.UserInterface;

namespace Content.Server.ArtilleryDetection.Systems;

/// <summary>
/// Server-side console system for displaying artillery fire detection logs.
/// </summary>
public sealed class ArtilleryDetectionConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ArtilleryDetectionSystem _detection = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("artdet.console");
        Subs.BuiEvents<ArtilleryDetectionConsoleComponent>(ArtilleryDetectionConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnConsoleOpened);
            subs.Event<DeleteArtilleryFireEventMessage>(OnDeleteEvent);
            subs.Event<RequestArtilleryFireEventsMessage>(OnRequestEvents);
        });
        // Log open attempt events to diagnose cancellations (power/lock/etc)
        SubscribeLocalEvent<ArtilleryDetectionConsoleComponent, ActivatableUIOpenAttemptEvent>(OnActivatableAttempt);
        // Also listen for activatable UI open events in case the ActivatableUI flow prevents the BUI from opening
        SubscribeLocalEvent<ArtilleryDetectionConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableOpen);
        SubscribeLocalEvent<ArtilleryDetectionConsoleComponent, AfterActivatableUIOpenEvent>(OnAfterActivatableOpen);
    }

    private void OnConsoleOpened(Entity<ArtilleryDetectionConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        _sawmill.Info($"Console opened: {ent.Owner}");
        UpdateConsoleUi(ent.Owner);
    }

    private void OnBeforeActivatableOpen(EntityUid uid, ArtilleryDetectionConsoleComponent component, BeforeActivatableUIOpenEvent args)
    {
        _sawmill.Info($"BeforeActivatableUIOpenEvent for {uid}, user {args.User}");
    }

    private void OnActivatableAttempt(EntityUid uid, ArtilleryDetectionConsoleComponent component, ref ActivatableUIOpenAttemptEvent args)
    {
        _sawmill.Info($"ActivatableUIOpenAttempt for {uid}, user {args.User}, cancelled: {args.Cancelled}");
    }

    private void OnAfterActivatableOpen(EntityUid uid, ArtilleryDetectionConsoleComponent component, AfterActivatableUIOpenEvent args)
    {
        _sawmill.Info($"AfterActivatableUIOpenEvent for {uid}, user {args.User}");
        // Ensure UI state is sent when activatable successfully opens
        UpdateConsoleUi(uid);
    }

    private void OnDeleteEvent(Entity<ArtilleryDetectionConsoleComponent> ent, ref DeleteArtilleryFireEventMessage msg)
    {
        _detection.DeleteFireEvent(msg.EventId);
        UpdateConsoleUi(ent.Owner);
    }

    private void OnRequestEvents(Entity<ArtilleryDetectionConsoleComponent> ent, ref RequestArtilleryFireEventsMessage msg)
    {
        UpdateConsoleUi(ent.Owner);
    }

    /// <summary>
    /// Updates the console UI with current fire events.
    /// </summary>
    private void UpdateConsoleUi(EntityUid consoleUid)
    {
        if (!TryComp<ArtilleryDetectionConsoleComponent>(consoleUid, out var _))
            return;

        var state = new ArtilleryDetectionConsoleState();

        // If console is networked, only show events from detectors on the same device network
        if (EntityManager.TryGetComponent<DeviceNetworkComponent>(consoleUid, out var consoleNet))
        {
            foreach (var detectorUid in _detection.DetectorEvents.Keys)
            {
                if (!EntityManager.TryGetComponent<DeviceNetworkComponent>(detectorUid, out var detNet))
                    continue;

                if (detNet.DeviceNetId != consoleNet.DeviceNetId)
                    continue;

                if (!_deviceNetwork.IsDeviceConnected(detectorUid, detNet))
                    continue;

                if (!_deviceNetwork.IsDeviceConnected(consoleUid, consoleNet))
                    continue;

                state.Events.AddRange(_detection.GetFireEvents(detectorUid));
            }
        }
        else
        {
            // Console not networked: show everything (legacy behavior)
            foreach (var detectorUid in _detection.DetectorEvents.Keys)
            {
                state.Events.AddRange(_detection.GetFireEvents(detectorUid));
            }
        }

        // Sort by detection time (newest first)
        state.Events.Sort((a, b) => b.DetectionTime.CompareTo(a.DetectionTime));

        _ui.SetUiState(consoleUid, ArtilleryDetectionConsoleUiKey.Key, state);
    }
}
