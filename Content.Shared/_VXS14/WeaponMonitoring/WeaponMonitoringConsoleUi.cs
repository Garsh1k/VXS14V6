using Content.Shared._VXS14.WeaponMonitoring.Components;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._VXS14.WeaponMonitoring;

[Serializable, NetSerializable]
public enum WeaponMonitoringConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class WeaponMonitoringConsoleState : BoundUserInterfaceState
{
    public List<WeaponMonitoringConsoleEntry> Entries = new();
    public NetEntity? ConsoleGrid;
    public bool PlanetaryMap;
}

[Serializable, NetSerializable]
public sealed class RequestWeaponMonitoringRefreshMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class WeaponMonitoringConsoleEntry
{
    public NetEntity Entity;
    public NetCoordinates Coordinates;
    public string Name = string.Empty;
    public WeaponMonitoringCategory Category;
    public float? Fov;
    public string SeekerType = string.Empty;
    public string WarheadType = string.Empty;
    public float? FlightTime;
    public float? Deviation;
    public float? ProjectileSpeed;
    public string LockedTarget = string.Empty;
    public string Notes = string.Empty;
}

[Serializable, NetSerializable]
public sealed class WeaponMonitoringControlActionMessage : BoundUserInterfaceMessage
{
    public new NetEntity Entity;
    public WeaponMonitoringControlAction Action;

    public WeaponMonitoringControlActionMessage(NetEntity entity, WeaponMonitoringControlAction action)
    {
        Entity = entity;
        Action = action;
    }
}

[Serializable, NetSerializable]
public enum WeaponMonitoringControlAction : byte
{
    SetBombTarget,
    ControlGun,
    LaunchRocket,
}
