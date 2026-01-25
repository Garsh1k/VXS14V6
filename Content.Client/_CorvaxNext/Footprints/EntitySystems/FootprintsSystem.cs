using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared._CorvaxNext.Footprints;
using Content.Shared._CorvaxNext.Footprints.Components;

namespace Content.Client._CorvaxNext.Footprints.EntitySystems;

public sealed class FootprintsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<TransformComponent> _transformQuery;
    private EntityQuery<MobThresholdsComponent> _mobThresholdQuery;
    private EntityQuery<AppearanceComponent> _appearanceQuery;

    public override void Initialize()
    {
        _transformQuery = GetEntityQuery<TransformComponent>();
        _mobThresholdQuery = GetEntityQuery<MobThresholdsComponent>();
        _appearanceQuery = GetEntityQuery<AppearanceComponent>();

        SubscribeLocalEvent<FootprintVisualizerComponent, ComponentStartup>(OnStartupComponent);
        SubscribeLocalEvent<FootprintVisualizerComponent, MoveEvent>(OnMove);
    }

    private void OnStartupComponent(EntityUid uid, FootprintVisualizerComponent component, ComponentStartup args)
    {
        component.StepSize = Math.Max(0f, component.StepSize + _random.NextFloat(-0.05f, 0.05f));
    }

    private void OnMove(EntityUid uid, FootprintVisualizerComponent component, ref MoveEvent args)
    {
        if (!_transformQuery.TryComp(uid, out var transform))
            return;

        // Check if entity is being dragged (has a puller)
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var dragging = entityManager.HasComponent<PullableComponent>(uid) &&
                      entityManager.TryGetComponent<PullableComponent>(uid, out var pullable) &&
                      pullable.Puller != null;

        // Also check mob state as backup
        if (!dragging && _mobThresholdQuery.TryComp(uid, out var mobThreshHolds))
        {
            dragging = mobThreshHolds.CurrentThresholdState is MobState.Critical or MobState.Dead;
        }

        if (!_map.TryFindGridAt(_transform.GetMapCoordinates((uid, transform)), out var gridUid, out _))
            return;

        var distance = (transform.LocalPosition - component.StepPos).Length();
        // Dramatically increase step sizes to drastically reduce footprint frequency
        var stepSize = dragging ? component.DragSize * 4.0f : component.StepSize * 3.0f;

        if (distance <= stepSize)
            return;

        // Check if we're stepping on a puddle - only create footprints when on puddles
        if (!IsSteppingOnPuddle(uid, transform, out var puddleColor))
        {
            component.StepPos = transform.LocalPosition; // Still update position to prevent spam
            return;
        }

        component.RightStep = !component.RightStep;

        var entity = Spawn(component.StepProtoId, CalcCoords(gridUid, component, transform, dragging));
        var footPrintComponent = EnsureComp<FootprintComponent>(entity);

        // Ensure the entity has the required components for visualization
        EnsureComp<SpriteComponent>(entity);
        EnsureComp<AppearanceComponent>(entity);

        footPrintComponent.FootprintsVisualizer = uid;
        Dirty(entity, footPrintComponent);

        if (_appearanceQuery.TryComp(entity, out var appearance))
        {
            _appearance.SetData(entity, FootprintVisualState.State, PickState(uid, dragging), appearance);
            // Set footprint color to match puddle color with appropriate alpha
            var footprintColor = puddleColor.WithAlpha(dragging ? 0.5f : 0.7f);
            _appearance.SetData(entity, FootprintVisualState.Color, footprintColor, appearance);
        }

        if (!_transformQuery.TryComp(entity, out var stepTransform))
            return;

        stepTransform.LocalRotation = dragging
            ? (transform.LocalPosition - component.StepPos).ToAngle() + Angle.FromDegrees(-90f)
            : transform.LocalRotation + Angle.FromDegrees(180f);

        component.StepPos = transform.LocalPosition;

        if (!TryComp<SolutionContainerManagerComponent>(entity, out var solutionContainer))
            return;

        if (!_solution.TryGetSolution((entity, solutionContainer), footPrintComponent.SolutionName, out var solutionEntity, out var solution))
            return;

        if (string.IsNullOrWhiteSpace(component.ReagentToTransfer) || solution.Volume >= 1)
            return;

        _solution.TryAddReagent(solutionEntity.Value, component.ReagentToTransfer, 1, out _);
    }

    private bool IsSteppingOnPuddle(EntityUid uid, TransformComponent transform, out Color puddleColor)
    {
        puddleColor = Color.Black; // Default color

        // Check if there's a puddle at the current position
        var puddles = new HashSet<Entity<PuddleComponent>>();
        // Increased detection radius significantly
        _lookup.GetEntitiesInRange(transform.Coordinates, 0.8f, puddles, LookupFlags.Dynamic | LookupFlags.Uncontained);

        foreach (var (puddleUid, puddleComp) in puddles)
        {
            // Check if puddle has substantial solution
            if (_solution.TryGetSolution(puddleUid, puddleComp.SolutionName, out _, out var solution) &&
                solution.Volume > 1.0f) // Require more liquid to reduce spam
            {
                // Get the puddle's color from appearance
                if (TryComp<AppearanceComponent>(puddleUid, out var appearance) &&
                    _appearance.TryGetData<Color>(puddleUid, PuddleVisuals.SolutionColor, out var color, appearance))
                {
                    puddleColor = color;
                }

                return true;
            }
        }

        return false;
    }

    private EntityCoordinates CalcCoords(EntityUid uid, FootprintVisualizerComponent component, TransformComponent transform, bool state)
    {
        if (state)
            return new EntityCoordinates(uid, transform.LocalPosition);

        var offset = component.RightStep
            ? new Angle(Angle.FromDegrees(180f) + transform.LocalRotation).RotateVec(component.OffsetPrint)
            : new Angle(transform.LocalRotation).RotateVec(component.OffsetPrint);

        return new EntityCoordinates(uid, transform.LocalPosition + offset);
    }

    private FootprintVisuals PickState(EntityUid uid, bool dragging)
    {
        var state = FootprintVisuals.BareFootprint;

        if (dragging)
            state = FootprintVisuals.Dragging;
        else if (_inventory.TryGetSlotEntity(uid, "shoes", out _))
            state = FootprintVisuals.ShoesPrint;

        return state;
    }
}
