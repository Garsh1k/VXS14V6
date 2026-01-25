// using Content.Shared.FixedPoint;
// using Content.Shared.Roles;
// using Content.Shared.Storage;
// using Robust.Shared.Network;
// using Robust.Shared.Prototypes;
// using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
// using Content.Server.GG.GameTicking.Rules.Components;
// using System.Linq;
// using Content.Server.GameTicking.Rules.Components;
// using Content.Server.KillTracking;
// using Content.Server.Mind;
// using Content.Server.Points;
// using Content.Server.RoundEnd;
// using Content.Server.Station.Systems;
// using Content.Shared.GameTicking.Components;
// using Content.Shared.Points;
// using Robust.Server.GameObjects;
// using Robust.Server.Player;
// using Robust.Shared.Utility;
// using Content.Shared.Roles;
// using Content.Shared.Station;
// using Robust.Shared.Prototypes;
// using Content.Shared.Clothing;
// using Content.Shared.Preferences;
// using Content.Shared.Preferences.Loadouts;
// using Robust.Shared.Player;
// using Content.Server.GameTicking.Rules;
// using Content.Server.GameTicking;
// using Content.Shared.Roles;
// using Content.Shared.Roles.Jobs;
// using Content.Shared.GG.CapturePoint;
// using Content.Server.Spawners.EntitySystems;
// using Robust.Shared.Map;
// using Content.Server.IntroSystem;
// using Content.Shared.IntroSystem;

// namespace Content.Server.GG.GameTicking.Rules.Systems;

// public sealed class CapturePointGameRuleSystem : GameRuleSystem<CapturePointGameRuleComponent>
// {
//     [Dependency] private readonly IPlayerManager _player = default!;
//     [Dependency] private readonly MindSystem _mind = default!;
//     [Dependency] private readonly PointSystem _point = default!;

//     [Dependency] private readonly RespawnRuleSystem _respawn = default!;
//     [Dependency] private readonly RoundEndSystem _roundEnd = default!;
//     [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
//     [Dependency] private readonly TransformSystem _transform = default!;
//     [Dependency] private readonly ActorSystem _actors = default!;

//     [Dependency] private readonly ContainerSpawnPointSystem _containerSpawnPointSystem = default!;

//     [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

//     public override void Initialize()
//     {
//         base.Initialize();
//         SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
//         SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
//     }

//     public override void Update(float frameTime)
//     {
//         base.Update(frameTime);

//         foreach (var (ruleComponent, _) in EntityQuery<CapturePointGameRuleComponent, GameRuleComponent>())
//         {
//             UpdateTeamPoints(ruleComponent, frameTime);
//             CheckForVictory(ruleComponent);
//         }
//     }
//     private void OnBeforeSpawn(PlayerBeforeSpawnEvent ev)
//     {

//         var query = EntityQueryEnumerator<CapturePointGameRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
//         while (query.MoveNext(out var uid, out var cp, out var tracker, out var rule))
//         {
//             if (!GameTicker.IsGameRuleActive(uid, rule))
//                 continue;


//             var protoMan = IoCManager.Resolve<IPrototypeManager>();

//             if(ev.JobId != null)
//             {

//             var jobPrototype = _prototypeManager.Index<JobPrototype>(ev.JobId);
//             var job = new JobComponent {Prototype = ev.JobId};
//             var jobLoadout = LoadoutSystem.GetJobPrototype(ev.JobId);

//             var newMind = _mind.CreateMind(ev.Player.UserId, ev.Profile.Name);
//             _mind.SetUserId(newMind, ev.Player.UserId);
//             Logger.Error($"ev.Player.Name {ev.Player.Name}");
//             Logger.Error($"ev.JobId {ev.JobId}");
//             var highPriorityJob = ev.Profile.JobPriorities.FirstOrDefault(p => p.Value == JobPriority.High).Key;
//             Logger.Error($"highPriorityJob {highPriorityJob}");
//             EntityCoordinates spawnCoordinates = EntityCoordinates.Invalid;
//             switch (highPriorityJob.Id)
//             {
//                 case "USEC":
//                     foreach (var spawn in EntityQuery<TransformComponent>())
//                     {
//                         if (EntityManager.GetComponentOrNull<MetaDataComponent>(spawn.Owner)?.EntityPrototype?.ID == "SpawnPointUSECLateJoin")
//                         {
//                             spawnCoordinates = Transform(spawn.Owner).Coordinates;
//                             break;
//                         }
//                     }
//                     break;
//                 case "Passenger":
//                     foreach (var spawn in EntityQuery<TransformComponent>())
//                     {
//                         if (EntityManager.GetComponentOrNull<MetaDataComponent>(spawn.Owner)?.EntityPrototype?.ID == "SpawnPointBEARLateJoin")
//                         {
//                             spawnCoordinates = Transform(spawn.Owner).Coordinates;
//                             break;
//                         }
//                     }
//                     break;
//             }
//             var mobMaybe = _stationSpawning.SpawnPlayerMob(spawnCoordinates, job,ev.Profile, ev.Station );
//             DebugTools.AssertNotNull(mobMaybe);
//             var mob = mobMaybe;

//             _mind.TransferTo(newMind, mob);

//             switch (ev.JobId)
//             {
//                 case "USEC":
//                     EnsureComp<GGUsecTeamComponent>(mob);
//                     break;
//                 case "Passenger":
//                     EnsureComp<GGBearTeamComponent>(mob);
//                     break;
//             }

//             _respawn.AddToTracker(ev.Player.UserId, (uid, tracker));

//             ev.Handled = true;
//             break;
//             }
//         }
//     }
//     private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
//     {

//         switch (ev.JobId)
//         {
//             case "USEC":
//                 EnsureComp<GGUsecTeamComponent>(ev.Mob);
//                 break;
//             case "Passenger":
//                 EnsureComp<GGBearTeamComponent>(ev.Mob);
//                 break;
//         }

//         var query = EntityQueryEnumerator<CapturePointGameRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
//         while (query.MoveNext(out var uid, out _, out var tracker, out var rule))
//         {
//             if (!GameTicker.IsGameRuleActive(uid, rule))
//                 continue;
//             _respawn.AddToTracker((ev.Mob, null), (uid, tracker));
//         }
//     }

//     private void UpdateTeamPoints(CapturePointGameRuleComponent component, float frameTime)
//     {
//         // Получить все точки с компонентом захвата.
//         foreach (var (capturePoint, _) in EntityQuery<GGCapturePointComponent, TransformComponent>())
//         {
//             // Начислять очки команде, которая владеет точкой.
//             if (capturePoint.Leader == "B")
//             {
//                 component.BearTeamPoints += frameTime;
//             }
//             else if (capturePoint.Leader == "U")
//             {
//                 component.UsecTeamPoints += frameTime;
//             }
//         }
//     }

//     private void CheckForVictory(CapturePointGameRuleComponent component)
//     {
//         if (component.Victor != null)
//             return;

//         if (component.BearTeamPoints >= component.PointsCap)
//         {
//             component.Victor = "Bear";
//             _roundEnd.EndRound(TimeSpan.FromSeconds(10f));
//         }
//         else if (component.UsecTeamPoints >= component.PointsCap)
//         {
//             component.Victor = "Usec";
//             _roundEnd.EndRound(TimeSpan.FromSeconds(10f));
//         }
//     }

//     protected override void AppendRoundEndText(EntityUid uid, CapturePointGameRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
//     {
//         if(component.Victor != null)
//         {
//             var victoryMessage = Loc.GetString($"gg-capturepoint-victory-{component.Victor.ToLower()}");
//             args.AddLine(Loc.GetString(victoryMessage));
//         }

//     }
// //
// }
