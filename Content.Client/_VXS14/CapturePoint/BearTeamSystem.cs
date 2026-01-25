
// using Content.Shared.StatusIcon.Components;
// using Robust.Shared.Prototypes;
// using Content.Shared.GG.CapturePoint;

// namespace Content.Client.GG.CapturePoint;

// public sealed class GGSyndyTeamSystem : SharedGGSyndyTeamSystem
// {
//     [Dependency] private readonly IPrototypeManager _prototype = default!;

//     public override void Initialize()
//     {
//         base.Initialize();

//         SubscribeLocalEvent<GGSyndyTeamComponent, GetStatusIconsEvent>(GetSyndyIcon);
//     }
//     private void GetSyndyIcon(Entity<GGSyndyTeamComponent> ent, ref GetStatusIconsEvent args)
//     {
//         if (HasComp<GGSolfedTeamComponent>(ent))
//             return;

//         if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
//         {
//             if(iconPrototype == null)
//             {
//                 Logger.WarningS("status-icon", $"Missing or invalid StatusIconPrototype: {ent.Comp.StatusIcon}");
//             }
//             else
//                 args.StatusIcons.Add(iconPrototype);


//         }
//     }


// }
