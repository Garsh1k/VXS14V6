using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.GameStates;

namespace Content.Shared.Mobs.Systems;

/// <summary>
/// System that prevents mobs in critical state from transitioning to ghost state
/// while still allowing them to move (WASD controls).
/// </summary>
public sealed class PreventGhostTransitionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventGhostTransitionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PreventGhostTransitionComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<PreventGhostTransitionComponent> ent, ref ComponentStartup args)
    {
        // When this component is added, ensure the mob can still move
        if (TryComp<MobStateComponent>(ent, out var mobState))
        {
            // Allow movement for critical mobs with this component
            if (mobState.CurrentState == MobState.Critical)
            {
                // We don't actually need to do anything here since we're modifying
                // the CheckAct method in MobStateSystem to allow movement for mobs
                // with PreventGhostTransitionComponent
            }
        }
    }

    private void OnShutdown(Entity<PreventGhostTransitionComponent> ent, ref ComponentShutdown args)
    {
        // Component removed, nothing special needed
    }
}
