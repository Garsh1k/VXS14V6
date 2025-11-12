using Content.Server.EUI;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Administration;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Content.Server._VXS14.Mortar;
using Content.Shared._VXS14.Mortar;
using Robust.Shared.Map;
using Robust.Shared.GameObjects;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.Audio;
using System.Numerics;
using Robust.Shared.Player;
using Robust.Shared.Audio;

namespace Content.Server._VXS14.Mortar;

/// <summary>
///     Mortar Eui
/// </summary>
///


[UsedImplicitly]
public sealed class MortarEui : BaseEui
{
    private int Count = 0;
    private readonly EntityUid Mortar;

    public MortarEui(EntityUid uid)
    {
        Mortar = uid;
    }

    public override void Opened()
    {
        base.Opened();

        // Send mortar configuration to the client
        var entMan = IoCManager.Resolve<IEntityManager>();
        var mortarComp = entMan.GetComponent<SharedMortarComponent>(Mortar);
        SendMessage(new MortarSpawnExplosionEuiMsg.MortarConfig(
            mortarComp.MinOffsetX,
            mortarComp.MaxOffsetX,
            mortarComp.MinOffsetY,
            mortarComp.MaxOffsetY,
            mortarComp.MinSafeDistance));
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not MortarSpawnExplosionEuiMsg.MortarCords request)
        {
            Close();
            return;
        }

        // Get the mortar's position
        var entMan = IoCManager.Resolve<IEntityManager>();
        var transformSystem = entMan.System<SharedTransformSystem>();
        var mortarPosition = transformSystem.GetMapCoordinates(Mortar);

        // Calculate the target position based on offsets
        var targetPosition = new MapCoordinates(
            new Vector2(
                mortarPosition.X + request.OffsetX,
                mortarPosition.Y + request.OffsetY),
            mortarPosition.MapId);

        // Prevent shooting at too close a range (use mortar's minimum safe distance)
        var distanceFromMortar = (targetPosition.Position - mortarPosition.Position).Length();
        var mortarComp = entMan.GetComponent<SharedMortarComponent>(Mortar);
        var minDistance = mortarComp.MinSafeDistance;
        if (distanceFromMortar < minDistance)
        {
            // Adjust target to minimum distance in the same direction
            var direction = targetPosition.Position - mortarPosition.Position;
            if (direction.Length() > 0)
            {
                direction = Vector2.Normalize(direction);
                var adjustedPosition = mortarPosition.Position + direction * minDistance;
                targetPosition = new MapCoordinates(adjustedPosition, mortarPosition.MapId);
            }
            else
            {
                // If direction is zero, set a default offset to the right
                targetPosition = new MapCoordinates(
                    new Vector2(mortarPosition.X + minDistance, mortarPosition.Y),
                    mortarPosition.MapId);
            }
        }

        // Dumb code
        var sysMan = IoCManager.Resolve<IEntitySystemManager>();
        var itemSlots = sysMan.GetEntitySystem<ItemSlotsSystem>();

        var rocket = itemSlots.GetItemOrNull(Mortar, "mortar_chamber");
        if (rocket == null)
        {
            Close();
            return;
        }

        entMan.TryGetComponent<SharedMortarShellComponent>(rocket, out var comp);

        // Play fire sound at mortar position
        if (comp?.FireSound != null)
        {
            var audioSystem = sysMan.GetEntitySystem<SharedAudioSystem>();
            var mortarCoords = entMan.GetComponent<TransformComponent>(Mortar).Coordinates;
            audioSystem.PlayPvs(new SoundPathSpecifier(comp.FireSound), mortarCoords);
        }

        // Calculate distance for delay
        var distance = (targetPosition.Position - mortarPosition.Position).Length();
        var delay = (int)(distance * (comp?.DelayPerTile ?? 0.1f) * 1000); // Convert to milliseconds

        // Schedule the explosion with delay
        var timerManager = IoCManager.Resolve<ITimerManager>();
        timerManager.AddTimer(new Timer(delay, false, () =>
        {
            // Play pre-explosion sound at target position
            if (comp?.PreExplosionSound != null)
            {
                var audioSystem = sysMan.GetEntitySystem<SharedAudioSystem>();
                var mapSystem = sysMan.GetEntitySystem<SharedMapSystem>();
                var mapEntity = mapSystem.GetMapOrInvalid(targetPosition.MapId);
                var targetCoords = transformSystem.ToCoordinates(mapEntity, targetPosition);
                audioSystem.PlayPvs(new SoundPathSpecifier(comp.PreExplosionSound), targetCoords);
            }

            // Add a small delay before the actual explosion
            timerManager.AddTimer(new Timer(500, false, () =>
            {
                entMan.DeleteEntity(rocket);

                // Apply distance-based accuracy scaling
                if(comp != null)
                {
                    // Get mortar component for accuracy parameters
                    var mortarComp = entMan.GetComponent<SharedMortarComponent>(Mortar);

                    // Calculate distance for accuracy scaling
                    var distance = (targetPosition.Position - mortarPosition.Position).Length();

                    // Calculate accuracy modifier (decreases with distance)
                    var accuracyModifier = Math.Max(0.1f, mortarComp.BaseAccuracy - (distance * mortarComp.AccuracyDegradation));

                    // Apply accuracy modifier to explosion parameters
                    var adjustedTotalIntensity = comp.TotalIntensity * accuracyModifier;
                    var adjustedSlope = comp.Slope * accuracyModifier;
                    var adjustedMaxTileIntensity = comp.MaxTileIntensity * accuracyModifier;

                    sysMan.GetEntitySystem<ExplosionSystem>().QueueExplosion(targetPosition, comp.Type, adjustedTotalIntensity, adjustedSlope, adjustedMaxTileIntensity, null);
                }
            }));
        }));

        Close();
    }
}
