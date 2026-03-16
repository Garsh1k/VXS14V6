using Content.Server.EUI;
using Content.Shared._VXS14.AerialBomb;
using Content.Shared.Eui;
using Content.Shared.Parallax.Biomes;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._VXS14.AerialBomb;

[UsedImplicitly]
public sealed class AerialBombEui : BaseEui
{
    private readonly EntityUid _bomb;

    public AerialBombEui(EntityUid bomb)
    {
        _bomb = bomb;
    }

    public override void Opened()
    {
        base.Opened();
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var mapSystem = entMan.System<SharedMapSystem>();

        var maps = new List<AerialBombEuiMsg.MapEntry>();

        foreach (var mapId in mapSystem.GetAllMapIds())
        {
            if (!mapSystem.MapExists(mapId))
                continue;

            var mapUid = mapSystem.GetMapOrInvalid(mapId);
            if (!entMan.TryGetComponent<BiomeComponent>(mapUid, out _))
                continue;

            var mapName = entMan.TryGetComponent<MetaDataComponent>(mapUid, out var metadata)
                ? metadata.EntityName
                : $"Map {mapId}";

            maps.Add(new AerialBombEuiMsg.MapEntry(entMan.GetNetEntity(mapUid), mapName));
        }

        maps.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return new AerialBombEuiState(maps);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not AerialBombEuiMsg.DropRequest request)
        {
            Close();
            return;
        }

        var entMan = IoCManager.Resolve<IEntityManager>();
        var mapSystem = entMan.System<SharedMapSystem>();
        var bombSystem = entMan.System<AerialBombSystem>();

        if (!entMan.TryGetComponent<SharedAerialBombComponent>(_bomb, out var bombComp))
        {
            Close();
            return;
        }

        if (bombComp.Dropped)
        {
            Close();
            return;
        }

        var targetMapUid = entMan.GetEntity(request.MapEntity);
        if (!entMan.EntityExists(targetMapUid) || !entMan.TryGetComponent<MapComponent>(targetMapUid, out var mapComp))
        {
            Close();
            return;
        }

        var targetMapId = mapComp.MapId;

        bombSystem.TryDropBomb(_bomb, targetMapId, bombComp);

        Close();
    }
}
