using Content.Shared.Clothing.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles interaction events for attaching and detaching chevrons.
/// </summary>
public sealed class ChevronInteractionSystem : EntitySystem
{
    [Dependency] private readonly ChevronSystem _chevronSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChevronComponent, UseInHandEvent>(OnChevronUseInHand);
        SubscribeLocalEvent<ChevronSlotComponent, InteractUsingEvent>(OnChevronSlotInteractUsing);
        SubscribeLocalEvent<ChevronSlotComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnChevronUseInHand(EntityUid uid, ChevronComponent component, UseInHandEvent args)
    {
        // When a chevron is used in hand, we could show information about it
        // For now, we'll just prevent the default use behavior
        args.Handled = true;
    }

    private void OnChevronSlotInteractUsing(EntityUid uid, ChevronSlotComponent component, InteractUsingEvent args)
    {
        // Check if the item being used is a chevron
        if (!HasComp<ChevronComponent>(args.Used))
            return;

        // Prevent handling the same interaction multiple times
        if (args.Handled)
            return;

        // Try to attach the chevron
        if (_chevronSystem.TryAttachChevron(uid, args.Used))
        {
            // Successfully attached - consume the chevron item
            QueueDel(args.Used);
            _popupSystem.PopupEntity(Loc.GetString("chevron-attached-success"), uid, args.User);
            args.Handled = true;
        }
        else
        {
            // Only show failure message if the user actually tried to attach
            _popupSystem.PopupEntity(Loc.GetString("chevron-attached-failure"), uid, args.User);
            args.Handled = true;
        }
    }

    private void OnGetVerbs(EntityUid uid, ChevronSlotComponent component, GetVerbsEvent<Verb> args)
    {
        // Only allow the wearer or someone stripping the wearer to remove chevrons
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Add a verb for each attached chevron
        for (int i = 0; i < component.AttachedChevronPrototypes.Count; i++)
        {
            var prototypeId = component.AttachedChevronPrototypes[i];
            var chevronInfo = GetChevronInfo(prototypeId);
            if (string.IsNullOrEmpty(chevronInfo))
                continue;

            // Create a closure with the current index
            var index = i;
            var verb = new Verb()
            {
                Text = Loc.GetString("chevron-remove-verb", ("chevron", chevronInfo)),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/eject.svg.192dpi.png")),
                Act = () => RemoveChevron(uid, component, prototypeId, index, args.User)
            };

            args.Verbs.Add(verb);
        }
    }

    private void RemoveChevron(EntityUid clothing, ChevronSlotComponent component, string prototypeId, int index, EntityUid user)
    {
        // Check if the index is still valid and the chevron at that index matches
        if (index < component.AttachedChevronPrototypes.Count &&
            component.AttachedChevronPrototypes[index] == prototypeId)
        {
            // Try to detach the chevron
            if (_chevronSystem.TryDetachChevron(clothing, prototypeId))
            {
                // Successfully detached - spawn the appropriate chevron item at the clothing's location
                var spawnPosition = Transform(clothing).Coordinates;
                var chevronItem = Spawn(prototypeId, spawnPosition);

                _popupSystem.PopupEntity(Loc.GetString("chevron-detached-success"), clothing, user);
                return;
            }
        }

        // If we get here, either the index was invalid or detachment failed
        _popupSystem.PopupEntity(Loc.GetString("chevron-detached-failure"), clothing, user);
    }

    private string GetChevronInfo(string prototypeId)
    {
        // Use the same mapping as in ChevronSystem
        return prototypeId switch
        {
            "ChevronCaptain" => "Captain",
            "ChevronHeadOfPersonnel" => "Head of Personnel",
            "ChevronHeadOfSecurity" => "Head of Security",
            "ChevronChiefMedicalOfficer" => "Chief Medical Officer",
            "ChevronResearchDirector" => "Research Director",
            _ => "Unknown Chevron"
        };
    }
}
