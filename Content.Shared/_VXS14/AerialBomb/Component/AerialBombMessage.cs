using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._VXS14.AerialBomb;

[Serializable, NetSerializable]
public sealed class AerialBombEuiState : EuiStateBase
{
    public readonly List<AerialBombEuiMsg.MapEntry> Maps;

    public AerialBombEuiState(List<AerialBombEuiMsg.MapEntry> maps)
    {
        Maps = maps;
    }
}

public static class AerialBombEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class DropRequest : EuiMessageBase
    {
        public readonly NetEntity MapEntity;

        public DropRequest(NetEntity mapEntity)
        {
            MapEntity = mapEntity;
        }
    }

    [Serializable, NetSerializable]
    public sealed class MapEntry
    {
        public readonly NetEntity MapEntity;
        public readonly string Name;

        public MapEntry(NetEntity mapEntity, string name)
        {
            MapEntity = mapEntity;
            Name = name;
        }
    }
}
