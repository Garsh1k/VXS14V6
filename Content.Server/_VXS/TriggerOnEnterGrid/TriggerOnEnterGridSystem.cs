using Content.Server.Explosion.EntitySystems;
using Content.Shared.Trigger;

namespace Content.Server._VXS.TriggerOnEnterGrid;

/// <summary>
/// Thanks, ekxkiri and justaunitydev!!
/// </summary>
public sealed class TriggerOnEnterGridSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TriggerOnEnterGridComponent, TransformComponent>();
        while (query.MoveNext(out var entity, out var component, out var xform))
        {
            switch (component.ReadyToTrigger)
            {
                case true when xform.GridUid.HasValue:
                    Trigger(entity);
                    break;
                case false when !xform.GridUid.HasValue:
                    component.ReadyToTrigger = true;
                    break;
            }
        }
    }

    public bool Trigger(EntityUid trigger, EntityUid? user = null)
    {
        var triggerEvent = new TriggerEvent(user);
        EntityManager.EventBus.RaiseLocalEvent(trigger, ref triggerEvent, true);
        return triggerEvent.Handled;
    }
}
