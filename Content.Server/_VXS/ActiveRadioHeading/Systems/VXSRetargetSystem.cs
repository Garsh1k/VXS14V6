using System.Numerics;
using Content.Server._VXS.ActiveRadioHeading.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._VXS.ActiveRadioHeading.Systems;


public sealed class VXSRetargetSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly VXSActiveRadioHeadingSystem _activeRadioHeading = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VXSRetargetComponent, ComponentStartup>(OnSpawn);
    }

    private void OnSpawn(Entity<VXSRetargetComponent> ent, ref ComponentStartup args)
    {
        var retargeterXform = Transform(ent);
        var retargeterPos = _transform.GetMapCoordinates(retargeterXform).Position;

        var missiles = new HashSet<Entity<VXSActiveRadioHeadingComponent, TransformComponent>>();
        _lookup.GetEntitiesOnMap(retargeterXform.MapID, missiles);

        foreach (var missile in missiles)
        {
            var missilePos = _transform.GetMapCoordinates(missile.Comp2).Position;


            var distance = Vector2.Distance(missilePos, retargeterPos);
            if (distance > missile.Comp1.SeekRange)
                continue;


            var missileAngle = _transform.GetWorldRotation(missile.Comp2);
            var angleToRetargeter = (retargeterPos - missilePos).ToWorldAngle();
            var angleDifference = Angle.ShortestDistance(missileAngle, angleToRetargeter);

            if (Math.Abs(angleDifference.Theta) > missile.Comp1.FOV * Math.PI / 180f / 2f)
                continue;

            if (!_random.Prob(ent.Comp.ChanceToRetarget))
                continue;


            _activeRadioHeading.SetNewTarget((missile, missile.Comp1), ent);
        }
    }
}
