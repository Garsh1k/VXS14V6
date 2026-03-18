using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._VXS.Manpads.Components;

[RegisterComponent]
public sealed partial class VXSIffTransponderComponent : Component
{
    [DataField("iffType", required: true)]
    public VXSManpadsIffType IffType = VXSManpadsIffType.SF;
}

public enum VXSManpadsIffType : byte
{
    SF,
    Syndicate,
}
