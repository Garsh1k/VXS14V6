using Content.Server.EUI;
using Content.Shared._VXS14.AerialBomb;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.Server._VXS14.AerialBomb;

public sealed class AerialBombSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SharedAerialBombComponent, GetVerbsEvent<ExamineVerb>>(OnGetVerb);
    }

    private void OnGetVerb(EntityUid uid, SharedAerialBombComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        var verb = new ExamineVerb
        {
            Text = Loc.GetString("aerial-bomb-verb-open"),
            Act = () => OpenUi(uid, args.User)
        };

        args.Verbs.Add(verb);
    }

    private void OpenUi(EntityUid bomb, EntityUid user)
    {
        if (!_player.TryGetSessionByEntity(user, out var session))
            return;

        var eui = IoCManager.Resolve<EuiManager>();
        eui.OpenEui(new AerialBombEui(bomb), session);
    }
}
