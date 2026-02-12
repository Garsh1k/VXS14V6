using Robust.Shared.Map;
using Robust.Shared.Serialization;
using System;
using System.Numerics;

namespace Content.Shared.ArtilleryDetection;

/// <summary>
/// Represents a detected artillery fire event.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArtilleryFireEvent
{
    /// <summary>
    /// Unique ID for this event.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the fire was detected.
    /// </summary>
    public TimeSpan DetectionTime { get; set; }

    /// <summary>
    /// Approximate coordinates of the shot (with inaccuracy applied).
    /// </summary>
    public Vector2 DetectedCoordinates { get; set; }

    /// <summary>
    /// Type of weapon that fired.
    /// </summary>
    public string WeaponType { get; set; } = "Unknown";

    public ArtilleryFireEvent() { }

    public ArtilleryFireEvent(Vector2 coordinates, string weaponType, TimeSpan detectionTime)
    {
        Id = Guid.NewGuid();
        DetectedCoordinates = coordinates;
        WeaponType = weaponType;
        DetectionTime = detectionTime;
    }
}
