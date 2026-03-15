using Content.Server.EUI;
using Content.Shared._VXS14.AerialBomb;
using Content.Shared.Eui;
using Content.Shared.Parallax.Biomes;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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
        var random = IoCManager.Resolve<IRobustRandom>();
        var mapSystem = entMan.System<SharedMapSystem>();
        var transformSystem = entMan.System<SharedTransformSystem>();
        var audio = entMan.System<SharedAudioSystem>();

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

        if (!entMan.TryGetComponent<BiomeComponent>(targetMapUid, out _))
        {
            Close();
            return;
        }

        if (!entMan.TryGetComponent<TransformComponent>(_bomb, out var xform) || xform.GridUid is null)
        {
            Close();
            return;
        }

        var sourcePosition = transformSystem.GetMapCoordinates(_bomb);
        var sourceMapUid = mapSystem.GetMapOrInvalid(sourcePosition.MapId);

        if (entMan.TryGetComponent<BiomeComponent>(sourceMapUid, out _))
        {
            Close();
            return;
        }

        if (string.IsNullOrWhiteSpace(bombComp.ImpactEntity))
        {
            Close();
            return;
        }

        bombComp.Dropped = true;

        var impactOffset = random.NextVector2(bombComp.DispersionRadius);
        var impactPosition = new MapCoordinates(sourcePosition.Position + impactOffset, targetMapId);
        var flightTime = Math.Max(0.1f, bombComp.FlightTime);
        var impactEntity = bombComp.ImpactEntity;
        var arrivalSound = bombComp.ArrivalSound;

        entMan.DeleteEntity(_bomb);

        Timer.Spawn(TimeSpan.FromSeconds(flightTime), () =>
        {
            if (!mapSystem.MapExists(targetMapId))
                return;

            var spawned = entMan.SpawnEntity(impactEntity, impactPosition);

            if (!string.IsNullOrWhiteSpace(arrivalSound))
                audio.PlayPvs(new SoundPathSpecifier(arrivalSound), spawned);
        });

        Close();
    }
}
