// using Content.Shared.GG.CapturePoint;
// using Content.Server.GG.GameTicking.Rules.Components;
// using Content.Shared.GameTicking.Components;

// namespace Content.Server.GG.CapturePoint;

// public sealed class CaptureTicketsSystem : EntitySystem
// {
//     public override void Initialize()
//     {
//         SubscribeLocalEvent<CapturePointGameRuleComponent, PointChangedEvent>(UpdateTickets);
//     }



//     public void UpdateTickets(EntityUid uid, CapturePointGameRuleComponent comp, ref PointChangedEvent args)
//     {
//         if (!TryComp<CaptureTicketsComponent>(uid, out var shared))
//             return;

//         shared.SyndyTickets = args.Point1;
//         Logger.Info($"[CapturePointRule] тикеты обновлены у синдиката → {shared.SyndyTickets}");
//         shared.SolfedTickets = args.Point2;
//         Logger.Info($"[CapturePointRule] тикеты обновлены у солфеда → {shared.SolfedTickets}");

//         Dirty(uid, shared);
//     }
// }



// [ByRefEvent]
// public readonly record struct PointChangedEvent(int Point1, int Point2);
