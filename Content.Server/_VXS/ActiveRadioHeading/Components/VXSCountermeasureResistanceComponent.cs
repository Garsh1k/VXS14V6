namespace Content.Server._VXS.ActiveRadioHeading.Components;

/// <summary>
/// Defines how a missile responds to countermeasures.
/// </summary>
public enum CountermeasureResistanceType : byte
{
    /// <summary>No countermeasure resistance — retargeting works normally.</summary>
    None = 0,

    /// <summary>
    /// Type 2: When a countermeasure is detected within FOV, the seeker FOV is narrowed
    /// to <see cref="VXSCountermeasureResistanceComponent.NarrowedFOV"/> instead of retargeting.
    /// </summary>
    FovNarrowing = 1,

    /// <summary>
    /// Type 3: When a countermeasure is detected within FOV, the seeker is temporarily
    /// disabled and the missile flies inertially. After <see cref="VXSCountermeasureResistanceComponent.InertialFlightDuration"/>
    /// the seeker re-activates.
    /// </summary>
    InertialFlight = 2,
}

[RegisterComponent]
public sealed partial class VXSCountermeasureResistanceComponent : Component
{
    /// <summary>
    /// Which countermeasure resistance mode this missile uses.
    /// </summary>
    [DataField(required: true)]
    public CountermeasureResistanceType Type = CountermeasureResistanceType.None;

    /// <summary>
    /// FOV in degrees the seeker narrows to upon detecting a countermeasure (Type 2 only).
    /// </summary>
    [DataField]
    public float NarrowedFOV = 10f;

    /// <summary>
    /// How long the missile flies inertially after detecting a countermeasure (Type 3 only).
    /// </summary>
    [DataField]
    public TimeSpan InertialFlightDuration = TimeSpan.FromSeconds(3);

    // --- Runtime state (not serialised) ---

    /// <summary>Whether the missile is currently in inertial-flight mode (Type 3).</summary>
    public bool InInertialFlight;

    /// <summary>Game-time at which the inertial-flight phase ends (Type 3).</summary>
    public TimeSpan InertialFlightEndTime;
}
