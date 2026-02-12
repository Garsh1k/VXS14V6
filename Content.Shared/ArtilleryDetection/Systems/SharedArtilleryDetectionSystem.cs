using Content.Shared.ArtilleryDetection.Components;

namespace Content.Shared.ArtilleryDetection.Systems;

/// <summary>
/// Shared base system for artillery detection.
/// </summary>
public abstract class SharedArtilleryDetectionSystem : EntitySystem
{
    /// <summary>
    /// Dictionary storing fire events per detector entity.
    /// </summary>
    public Dictionary<EntityUid, List<ArtilleryFireEvent>> DetectorEvents = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArtilleryDetectorComponent, ComponentShutdown>(OnDetectorShutdown);
    }

    private void OnDetectorShutdown(Entity<ArtilleryDetectorComponent> ent, ref ComponentShutdown args)
    {
        DetectorEvents.Remove(ent.Owner);
    }

    /// <summary>
    /// Registers a fire detection event.
    /// </summary>
    public void RegisterFireEvent(EntityUid detectorId, ArtilleryFireEvent fireEvent)
    {
        Logger.DebugS("artdet.shared", $"RegisterFireEvent called for detector {detectorId}");
        Logger.DebugS("artdet.shared", $"Event details - Weapon: {fireEvent.WeaponType}, Coords: {fireEvent.DetectedCoordinates}, Time: {fireEvent.DetectionTime}");

        if (!DetectorEvents.ContainsKey(detectorId))
        {
            Logger.DebugS("artdet.shared", $"Creating new event list for detector {detectorId}");
            DetectorEvents[detectorId] = new List<ArtilleryFireEvent>();
        }

        DetectorEvents[detectorId].Add(fireEvent);
        Logger.InfoS("artdet.shared", $"Event added to detector {detectorId}. Total events for this detector: {DetectorEvents[detectorId].Count}");
        Logger.InfoS("artdet.shared", $"Total detectors with events: {DetectorEvents.Count}");
    }

    /// <summary>
    /// Gets all fire events from a detector.
    /// </summary>
    public List<ArtilleryFireEvent> GetFireEvents(EntityUid detectorId)
    {
        Logger.DebugS("artdet.shared", $"GetFireEvents called for detector {detectorId}");
        Logger.DebugS("artdet.shared", $"Total detectors in system: {DetectorEvents.Count}");

        if (DetectorEvents.TryGetValue(detectorId, out var events))
        {
            Logger.DebugS("artdet.shared", $"Found {events.Count} events for detector {detectorId}");
            return new List<ArtilleryFireEvent>(events);
        }

        Logger.WarningS("artdet.shared", $"No events found for detector {detectorId}");
        return new List<ArtilleryFireEvent>();
    }

    /// <summary>
    /// Removes a fire event from the log.
    /// </summary>
    public void RemoveFireEvent(EntityUid detectorId, Guid eventId)
    {
        if (DetectorEvents.TryGetValue(detectorId, out var events))
        {
            events.RemoveAll(e => e.Id == eventId);
        }
    }

    /// <summary>
    /// Clears all events from a detector.
    /// </summary>
    public void ClearEvents(EntityUid detectorId)
    {
        if (DetectorEvents.ContainsKey(detectorId))
        {
            DetectorEvents[detectorId].Clear();
        }
    }
}
