using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.GameTicking;
using Robust.Shared.Timing;

namespace Content.Shared._VXS.Nightvision;

public sealed class NightvisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly BlindableSystem _blindableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NightvisionComponent, EntityPausedEvent>(OnEntityPaused);
        SubscribeLocalEvent<NightvisionComponent, EntityUnpausedEvent>(OnEntityUnpaused);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTiming.Paused)
            return;

        var query = EntityQueryEnumerator<NightvisionComponent, BlindableComponent>();
        while (query.MoveNext(out var uid, out var nightVision, out var blindable))
        {
            // Check if light is too intense (amplification > threshold)
            if (nightVision.LightAmplification > 5.0f) // Using a fixed threshold for now
            {
                nightVision.LightTooIntense = true;
                nightVision.IntenseLightTimer += frameTime;

                // Apply eye damage based on how long the light has been too intense
                // and how intense it is
                if (nightVision.IntenseLightTimer > 1.0f) // Damage every second of exposure
                {
                    var damageAmount = (int)(nightVision.LightAmplification / 5.0f);
                    _blindableSystem.AdjustEyeDamage((uid, blindable), damageAmount);
                    nightVision.IntenseLightTimer = 0f; // Reset timer
                }
            }
            else
            {
                nightVision.LightTooIntense = false;
                nightVision.IntenseLightTimer = 0f;
            }

            Dirty(uid, nightVision);
        }
    }

    private void OnEntityPaused(EntityUid uid, NightvisionComponent component, ref EntityPausedEvent args)
    {
        // When entity is paused, we don't need to do anything special
        // The timer will naturally stop incrementing while paused
    }

    private void OnEntityUnpaused(EntityUid uid, NightvisionComponent component, ref EntityUnpausedEvent args)
    {
        // Reset timer when entity unpauses
        component.IntenseLightTimer = 0f;
        Dirty(uid, component);
    }
}
