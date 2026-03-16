using Content.Shared.Shuttles.Components;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.UI.MapObjects;

[Serializable, NetSerializable]
public sealed class RadarMarkerData
{
    public NetCoordinates Coordinates;
    public Angle Angle;
    public bool Enabled;
    public RadarShape Shape;
    public Color Color;
    public bool ShowName;
    public string? Name;
}
