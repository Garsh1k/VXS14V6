using Content.Shared.GG.CapturePoint;
using Content.Client.CharacterInfo;
using Robust.Client.UserInterface.Controls;
using Content.Client.CharacterInfo;
using Content.Client.Message;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.GG.CapturePoint;

public sealed class CaptureTicketsSystem : EntitySystem
{
    [Dependency] private readonly CharacterInfoSystem _characterInfo = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CaptureTicketsComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<CharacterInfoSystem.GetCharacterInfoControlsEvent>(OnCharacterInfo);
    }

    private void OnState(EntityUid uid, CaptureTicketsComponent comp, ref AfterAutoHandleStateEvent args)
    {
        _characterInfo.RequestCharacterInfo();
    }

    private void OnCharacterInfo(ref CharacterInfoSystem.GetCharacterInfoControlsEvent ev)
    {
        foreach (var comp in EntityQuery<CaptureTicketsComponent>())
        {
            var box = new BoxContainer
            {
                Margin = new Thickness(5),
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };

            var title = new RichTextLabel
            {
                HorizontalAlignment = Control.HAlignment.Center
            };
            title.SetMarkup(Loc.GetString("point-scoreboard-header"));

            var line1 = new RichTextLabel();
            line1.SetMarkup($"Syndy: {comp.SyndyTickets}");

            var line2 = new RichTextLabel();
            line2.SetMarkup($"Solfed: {comp.SolfedTickets}");

            box.AddChild(title);
            box.AddChild(line1);
            box.AddChild(line2);

            ev.Controls.Add(box);
        }
    }
}
