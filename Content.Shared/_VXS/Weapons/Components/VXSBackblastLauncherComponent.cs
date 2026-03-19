using Robust.Shared.Prototypes;

namespace Content.Shared._VXS.Weapons.Components;

[RegisterComponent]
public sealed partial class VXSBackblastLauncherComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ProjectileProto = default!;

    [DataField]
    public float Speed = 28f;
}
