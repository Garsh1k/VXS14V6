using Content.Server.Popups;
using Content.Shared.Projectiles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Server.Projectiles;

/// <summary>
/// Server-side implementation of the ricochet system.
/// </summary>
public sealed class RicochetSystem : SharedRicochetSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    // Server-specific overrides or additions can go here if needed
}
