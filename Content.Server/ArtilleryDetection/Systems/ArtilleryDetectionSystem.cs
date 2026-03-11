using Content.Server.Station.Systems;
using Content.Shared.ArtilleryDetection;
using Content.Shared.ArtilleryDetection.Components;
using Content.Shared.ArtilleryDetection.Systems;
using Robust.Server.GameObjects;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Robust.Shared.Map;
using System;
using System.Numerics;

namespace Content.Server.ArtilleryDetection.Systems;

/// <summary>
/// Server-side system for detecting and logging artillery fire.
/// </summary>
public sealed class ArtilleryDetectionSystem : SharedArtilleryDetectionSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    private ISawmill _sawmill = default!;

    /// <summary>
    /// Dictionary of pending fire detections with their scheduled times.
    /// Format: (detectorId, fireEvent, scheduledTime)
    /// </summary>
    private List<(EntityUid DetectorId, ArtilleryFireEvent Event, float ScheduledTime)> _pendingDetections = new();

    /// <summary>
    /// Counter for generating local sequential IDs for events.
    /// </summary>
    private int _localEventIdCounter = 0;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("artdet.detection");
        _sawmill.Info("=== ARTILLERY DETECTION SYSTEM INITIALIZED ===");
        _sawmill.Info($"System instance: {this}");
        _sawmill.Info($"Dependencies resolved: GameTiming={_gameTiming != null}, UI={_ui != null}, DeviceNetwork={_deviceNetwork != null}");
        // Removed GunComponent subscription as artillery uses separate firing system
        // SubscribeLocalEvent<GunComponent, AmmoShotEvent>(OnGunShot);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Process pending detections that have reached their scheduled time
        var currentTime = (float)_gameTiming.CurTime.TotalSeconds;

        if (_pendingDetections.Count > 0)
        {
            _sawmill.Debug($"=== UPDATE CYCLE START ===");
            _sawmill.Debug($"Обработка {_pendingDetections.Count} ожидающих обнаружений. Текущее время: {currentTime:F2}");
        }

        for (int i = _pendingDetections.Count - 1; i >= 0; i--)
        {
            var (detectorId, fireEvent, scheduledTime) = _pendingDetections[i];

            if (currentTime < scheduledTime)
            {
                _sawmill.Debug($"Событие еще не готово: текущее время {currentTime:F2} < запланированное {scheduledTime:F2}");
                continue;
            }

            _sawmill.Info($"=== ОБРАБОТКА СОБЫТИЯ ===");
            _sawmill.Info($"Время пришло для события: текущее время {currentTime:F2} >= запланированное {scheduledTime:F2}");

            // Only register if detector still exists
            if (!Exists(detectorId))
            {
                _sawmill.Warning($"Детектор {detectorId} больше не существует, событие отброшено");
                _pendingDetections.RemoveAt(i);
                continue;
            }

            RegisterFireEvent(detectorId, fireEvent);
            _sawmill.Info($"✓ Событие ЗАРЕГИСТРИРОВАНО для детектора {detectorId}");
            _sawmill.Info($"  Оружие: {fireEvent.WeaponType}");
            _sawmill.Info($"  Координаты: {fireEvent.DetectedCoordinates}");
            _sawmill.Info($"  Время: {fireEvent.DetectionTime}");

            // Update any connected consoles
            _sawmill.Info("Обновление консолей...");
            var query = EntityQueryEnumerator<ArtilleryDetectionConsoleComponent>();
            int consoleCount = 0;
            while (query.MoveNext(out var consoleUid, out _))
            {
                consoleCount++;
                _sawmill.Info($"Обновление консоли #{consoleCount}: {consoleUid}");
                // Check if this console is connected to the detector via device network
                // For now, update all consoles (they'll filter accordingly)
                UpdateConsoleUi(consoleUid);
            }
            _sawmill.Info($"Всего обновлено консолей: {consoleCount}");

            _pendingDetections.RemoveAt(i);
            _sawmill.Info("=== СОБЫТИЕ ОБРАБОТАНО ===");
        }

        if (_pendingDetections.Count > 0)
        {
            _sawmill.Debug($"=== UPDATE CYCLE END ===");
            _sawmill.Debug($"Осталось событий в очереди: {_pendingDetections.Count}");
        }
    }

    // Removed OnGunShot method as artillery uses separate firing system
    // private void OnGunShot(Entity<GunComponent> gun, ref AmmoShotEvent args)
    // {
    //     // Old implementation for regular guns - removed to prevent conflicts with artillery system
    // }

    /// <summary>
    /// Test method to verify system is working
    /// </summary>
    public void TestMethod()
    {
        _sawmill.Info("=== TEST METHOD CALLED SUCCESSFULLY ===");
    }

    /// <summary>
    /// Called when artillery (such as a mortar) fires to detect it.
    /// </summary>
    public void OnArtilleryFired(MapCoordinates firePosition, string weaponType, TimeSpan detectionTime, string artilleryType = "Unknown", string projectileType = "Unknown")
    {
        var mapId = firePosition.MapId;
        _sawmill.Info($"=== АРТИЛЛЕРИЙСКИЙ ВЫСТРЕЛ ОБНАРУЖЕН ===");
        _sawmill.Info($"Оружие: {weaponType}");
        _sawmill.Info($"Позиция выстрела: {firePosition.Position} на карте {mapId}");
        _sawmill.Info($"Время обнаружения: {detectionTime}");
        _sawmill.Info($"Тип артиллерии: {artilleryType}, Тип снаряда: {projectileType}");

        var detectorQuery = EntityQueryEnumerator<ArtilleryDetectorComponent>();
        int detectorCount = 0;
        int foundCount = 0;
        int registeredCount = 0;

        _sawmill.Info("Поиск активных артиллерийских детекторов...");

        while (detectorQuery.MoveNext(out var detectorUid, out var detector))
        {
            detectorCount++;
            _sawmill.Debug($"Найден детектор #{detectorCount}: {detectorUid}");

            var detectorCoords = _transformSystem.GetMapCoordinates(detectorUid);
            _sawmill.Debug($"Координаты детектора: {detectorCoords.Position} на карте {detectorCoords.MapId}");

            // Check if detector is on the same map
            if (detectorCoords.MapId != mapId)
            {
                _sawmill.Warning($"Детектор {detectorUid} не на той же карте! Детектор: {detectorCoords.MapId}, Выстрел: {mapId}");
                continue;
            }

            // Check distance
            var distance = Vector2.Distance(firePosition.Position, detectorCoords.Position);
            _sawmill.Info($"Дистанция между выстрелом и детектором {detectorUid}: {distance:F2} тайлов (радиус детектора: {detector.DetectionRadius})");

            if (distance > detector.DetectionRadius)
            {
                _sawmill.Warning($"Детектор {detectorUid} слишком далеко! Расстояние: {distance:F2}, Радиус: {detector.DetectionRadius}");
                continue;
            }

            foundCount++;
            _sawmill.Info($"✓ Детектор {detectorUid} в зоне действия!");
            _sawmill.Info($"ShowArtilleryType: {detector.ShowArtilleryType}, ShowProjectileType: {detector.ShowProjectileType}");

            // Calculate inaccurate coordinates using robust random
            var offsetX = (float)(_random.NextGaussian() - 0.5f) * detector.AccuracyX * 2f;
            var offsetY = (float)(_random.NextGaussian() - 0.5f) * detector.AccuracyY * 2f;

            var detectedCoords = new Vector2(
                firePosition.Position.X + offsetX,
                firePosition.Position.Y + offsetY
            );

            _sawmill.Info($"Точные координаты выстрела: {firePosition.Position}");
            _sawmill.Info($"Обнаруженные координаты (с погрешностью): {detectedCoords}");
            _sawmill.Info($"Погрешность: X={offsetX:F2}, Y={offsetY:F2}");

            // Create fire event with optional artillery and projectile type information
            var filteredArtilleryType = detector.ShowArtilleryType ? artilleryType : "Unknown";
            var filteredProjectileType = detector.ShowProjectileType ? projectileType : "Unknown";

            _localEventIdCounter++;
            var fireEvent = new ArtilleryFireEvent(
                coordinates: detectedCoords,
                weaponType: weaponType,
                detectionTime: detectionTime,
                artilleryType: filteredArtilleryType,
                projectileType: filteredProjectileType,
                localId: _localEventIdCounter
            );

            // Queue for delayed processing
            var scheduledTime = (float)_gameTiming.CurTime.TotalSeconds + detector.DetectionDelay;
            _pendingDetections.Add((detectorUid, fireEvent, scheduledTime));
            registeredCount++;

            _sawmill.Info($"Событие поставлено в очередь для детектора {detectorUid}");
            _sawmill.Info($"Задержка обнаружения: {detector.DetectionDelay} секунд");
            _sawmill.Info($"Запланированное время обработки: {scheduledTime:F2} секунд");
        }

        _sawmill.Info($"=== РЕЗУЛЬТАТЫ СКАНИРОВАНИЯ ===");
        _sawmill.Info($"Всего детекторов в мире: {detectorCount}");
        _sawmill.Info($"Детекторов на той же карте: {foundCount}");
        _sawmill.Info($"Событий поставлено в очередь: {registeredCount}");
        _sawmill.Info($"Событий в очереди ожидания: {_pendingDetections.Count}");

        if (registeredCount == 0)
        {
            _sawmill.Error("!!! НИ ОДНО СОБЫТИЕ НЕ ЗАРЕГИСТРИРОВАНО !!!");
            _sawmill.Error("Возможные причины:");
            _sawmill.Error("1. Нет активных артиллерийских детекторов в мире");
            _sawmill.Error("2. Все детекторы находятся на другой карте");
            _sawmill.Error("3. Все детекторы слишком далеко от точки выстрела");
        }
    }

    /// <summary>
    /// Updates the UI for an artillery detection console.
    /// </summary>
    private void UpdateConsoleUi(EntityUid consoleUid)
    {
        if (!TryComp<ArtilleryDetectionConsoleComponent>(consoleUid, out _))
            return;

        var state = new ArtilleryDetectionConsoleState();

        // If the console is networked, only include detectors on the same network
        if (EntityManager.TryGetComponent<DeviceNetworkComponent>(consoleUid, out var consoleNet))
        {
            foreach (var detectorUid in DetectorEvents.Keys)
            {
                if (!EntityManager.TryGetComponent<DeviceNetworkComponent>(detectorUid, out var detNet))
                    continue;

                if (detNet.DeviceNetId != consoleNet.DeviceNetId)
                    continue;

                if (!_deviceNetwork.IsDeviceConnected(detectorUid, detNet))
                    continue;

                if (!_deviceNetwork.IsDeviceConnected(consoleUid, consoleNet))
                    continue;

                state.Events.AddRange(GetFireEvents(detectorUid));
            }
        }
        else
        {
            // Console not networked: show everything
            foreach (var detectorUid in DetectorEvents.Keys)
            {
                state.Events.AddRange(GetFireEvents(detectorUid));
            }
        }

        // Sort by detection time (newest first)
        state.Events.Sort((a, b) => b.DetectionTime.CompareTo(a.DetectionTime));

        _ui.SetUiState(consoleUid, ArtilleryDetectionConsoleUiKey.Key, state);
    }

    /// <summary>
    /// Called when a console requests to delete a fire event.
    /// </summary>
    public void DeleteFireEvent(Guid eventId)
    {
        foreach (var (_, events) in DetectorEvents)
        {
            events.RemoveAll(e => e.Id == eventId);
        }
    }
}
