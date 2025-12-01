using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Armor;

[Serializable, NetSerializable]
public sealed partial class PlateInsertDoAfterEvent : SimpleDoAfterEvent
{
}
