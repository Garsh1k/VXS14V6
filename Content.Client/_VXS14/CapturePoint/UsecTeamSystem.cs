
// using Content.Shared.StatusIcon.Components;
// using Robust.Shared.Prototypes;
// using Content.Shared.GG.CapturePoint;

// namespace Content.Client.GG.CapturePoint;

// public sealed class GGSolfedTeamSystem : SharedGGSolfedTeamSystem
// {
//     [Dependency] private readonly IPrototypeManager _prototype = default!;

//     public override void Initialize()
//     {
//         base.Initialize();

//         SubscribeLocalEvent<GGSolfedTeamComponent, GetStatusIconsEvent>(GetSolfedIcon);
//     }

//     private void GetSolfedIcon(Entity<GGSolfedTeamComponent> ent, ref GetStatusIconsEvent args)
//     {
//         if (HasComp<GGSyndyTeamComponent>(ent))
//             return;

//         if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
//             args.StatusIcons.Add(iconPrototype);
//     }

// }
