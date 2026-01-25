using System.Numerics;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared._CorvaxNext.Footprints.Components;

namespace Content.Server.Fluids.EntitySystems;

/// <inheritdoc/>
public sealed class AbsorbentSystem : SharedAbsorbentSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public const float FootprintAbsorptionRange = 0.25f; // Corvax-Next-Footprints

    public override void Initialize()
    {
        base.Initialize();
        // No event subscriptions - let the shared system handle all events
        // We'll provide footprint functionality as helper methods that can be called externally
    }



    // Corvax-Next-Footprints-Start
    /// <summary>
    /// Custom footprint interaction logic that runs before the shared system's standard absorbent logic.
    /// Called from our server-specific event handlers to handle footprint cleaning.
    /// </summary>
    private bool TryFootprintInteract(EntityUid user, EntityUid used, EntityUid target, AbsorbentComponent absorber, UseDelayComponent? useDelay, Entity<SolutionComponent> absorberSoln)
    {
        if (!HasComp<FootprintComponent>(target)) // Perform a check if it was a footprint that was clicked on
            return false;

        var soundPlayed = false;

        var footPrints = new HashSet<Entity<FootprintComponent>>();
        _lookup.GetEntitiesInRange(Transform(target).Coordinates, FootprintAbsorptionRange, footPrints, LookupFlags.Dynamic | LookupFlags.Uncontained);

        foreach (var (footstepUid, comp) in footPrints)
        {
            if (!_solutionContainerSystem.TryGetSolution(footstepUid, comp.SolutionName, out var solutionEntity, out var targetStepSolution) || targetStepSolution.Volume <= 0)
                continue;

            if (_puddleSystem.CanFullyEvaporate(targetStepSolution))
                continue; // no spam

            var absorberSolution = absorberSoln.Comp.Solution;
            var available = absorberSolution.GetTotalPrototypeQuantity(_puddleSystem.GetEvaporatingReagents(absorberSolution));

            // No material
            if (available == FixedPoint2.Zero)
            {
                _popups.PopupEntity(Loc.GetString("mopping-system-no-water", ("used", used)), user, user);
                return true;
            }

            var transferMax = absorber.PickupAmount;
            var transferAmount = FixedPoint2.Min(transferMax, available);

            var puddleSplit = targetStepSolution.SplitSolutionWithout(transferAmount, _puddleSystem.GetEvaporatingReagents(targetStepSolution));
            var absorberSplit = absorberSolution.SplitSolutionWithOnly(puddleSplit.Volume, _puddleSystem.GetEvaporatingReagents(absorberSolution));

            var transform = Transform(target);
            var gridUid = transform.GridUid;
            if (TryComp<MapGridComponent>(gridUid, out var mapGrid))
            {
                var tileRef = _mapSystem.GetTileRef(gridUid.Value, mapGrid, transform.Coordinates);
                _puddleSystem.DoTileReactions(tileRef, absorberSplit);
            }

            _solutionContainerSystem.AddSolution(solutionEntity.Value, absorberSplit);
            _solutionContainerSystem.AddSolution(absorberSoln, puddleSplit);

            if (!soundPlayed)
            {
                soundPlayed = true; // to prevent sound spam
                _audio.PlayPvs(absorber.PickupSound, target);
            }

            if (useDelay is not null)
                _useDelay.TryResetDelay((used, useDelay));
        }

        var userXform = Transform(user);
        var targetPos = _transform.GetWorldPosition(target);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(userXform));
        localPos = userXform.LocalRotation.RotateVec(localPos);

        _melee.DoLunge(user, used, Angle.Zero, localPos, null, false);

        return true;
    }

    /// <summary>
    /// Public method that can be called by other systems to handle footprint cleaning.
    /// This allows external systems to integrate footprint cleaning functionality
    /// without creating event subscription conflicts.
    /// </summary>
    public bool TryCleanFootprints(EntityUid user, EntityUid target, EntityUid used, AbsorbentComponent component)
    {
        if (!SolutionContainer.TryGetSolution(used, component.SolutionName, out var absorberSoln))
            return false;

        if (TryComp<UseDelayComponent>(used, out var useDelay)
            && _useDelay.IsDelayed((used, useDelay)))
            return false;

        return TryFootprintInteract(user, used, target, component, useDelay, absorberSoln.Value);
    }
    // Corvax-Next-Footprints-End
}
