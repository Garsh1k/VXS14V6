using Content.Server._VXS14.AerialBomb;
using Content.Shared._ADT.SS40k.Turrets;
using Content.Shared._ADT.SS40k.Turrets.Components;
using Content.Shared._VXS14.AerialBomb;
using Content.Shared._VXS14.WeaponMonitoring;
using Content.Shared._VXS14.WeaponMonitoring.Components;
using Content.Shared.Mind;
using Content.Shared.Trigger.Systems;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Server._VXS14.WeaponMonitoring;

public sealed class WeaponMonitoringConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly AerialBombSystem _aerialBombSystem = default!;
    [Dependency] private readonly TriggerSystem _triggerSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    private const float UpdateInterval = 1.0f;
    private float _updateAccumulator;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<WeaponMonitoringConsoleComponent>(WeaponMonitoringConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnConsoleOpened);
            subs.Event<RequestWeaponMonitoringRefreshMessage>(OnRefreshRequested);
            subs.Event<WeaponMonitoringControlActionMessage>(OnControlAction);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateAccumulator += frameTime;
        if (_updateAccumulator < UpdateInterval)
            return;

        _updateAccumulator = 0f;

        var query = EntityQueryEnumerator<WeaponMonitoringConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (!_ui.IsUiOpen(uid, WeaponMonitoringConsoleUiKey.Key))
                continue;

            UpdateConsoleUi(uid, xform);
        }
    }

    private void OnConsoleOpened(Entity<WeaponMonitoringConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateConsoleUi(ent.Owner);
    }

    private void OnRefreshRequested(Entity<WeaponMonitoringConsoleComponent> ent, ref RequestWeaponMonitoringRefreshMessage args)
    {
        UpdateConsoleUi(ent.Owner);
    }

    private void OnControlAction(Entity<WeaponMonitoringConsoleComponent> ent, ref WeaponMonitoringControlActionMessage args)
    {
        var actor = args.Actor;
        if (!actor.Valid)
            return;

        var target = GetEntity(args.Entity);
        if (!target.Valid || !EntityManager.EntityExists(target))
            return;

        if (!TryComp<TransformComponent>(ent.Owner, out var consoleXform) ||
            !TryComp<TransformComponent>(target, out var targetXform) ||
            consoleXform.GridUid != targetXform.GridUid)
        {
            return;
        }

        switch (args.Action)
        {
            case WeaponMonitoringControlAction.SetBombTarget:
                if (!HasComp<SharedAerialBombComponent>(target))
                    return;

                _aerialBombSystem.TryOpenUi(target, actor);
                break;

            case WeaponMonitoringControlAction.ControlGun:
                if (!TryComp<TurretControllableComponent>(target, out var turretComp))
                    return;

                if (turretComp.User is { } && turretComp.User != actor)
                    return;

                RaiseLocalEvent(target, new GettingControlledEvent(actor, ent.Owner));
                _mindSystem.ControlMob(actor, target);
                break;

            case WeaponMonitoringControlAction.LaunchRocket:
                _triggerSystem.Trigger(target, actor);
                break;
        }
    }

    private void UpdateConsoleUi(EntityUid consoleUid, TransformComponent? consoleXform = null)
    {
        if (!Resolve(consoleUid, ref consoleXform, false))
            return;

        var entries = new List<WeaponMonitoringConsoleEntry>();
        var consoleGrid = consoleXform.GridUid;

        if (consoleGrid != null)
        {
            var query = EntityQueryEnumerator<WeaponMonitoringProfileComponent, TransformComponent, MetaDataComponent>();
            while (query.MoveNext(out var uid, out var profile, out var xform, out var meta))
            {
                if (xform.GridUid != consoleGrid)
                    continue;

                entries.Add(new WeaponMonitoringConsoleEntry
                {
                    Entity = GetNetEntity(uid),
                    Name = meta.EntityName,
                    Category = profile.Category,
                    Fov = profile.Fov,
                    SeekerType = profile.SeekerType,
                    WarheadType = profile.WarheadType,
                    FlightTime = profile.FlightTime,
                    Deviation = profile.Deviation,
                    ProjectileSpeed = profile.ProjectileSpeed,
                    Notes = profile.Notes,
                });
            }
        }

        entries.Sort(static (a, b) =>
        {
            var categoryCompare = a.Category.CompareTo(b.Category);
            if (categoryCompare != 0)
                return categoryCompare;

            return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });

        _ui.SetUiState(consoleUid, WeaponMonitoringConsoleUiKey.Key, new WeaponMonitoringConsoleState
        {
            Entries = entries
        });
    }
}
