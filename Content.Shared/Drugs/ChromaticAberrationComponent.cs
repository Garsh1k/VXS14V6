using Robust.Shared.GameStates;

namespace Content.Shared.Drugs;

/// <summary>
///  Adds a chromatic aberration shader to the client that scales with the effect duration.
///  Use only in conjunction with <see cref="StatusEffectComponent"/>, on the status effect entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChromaticAberrationStatusEffectComponent : Component;
