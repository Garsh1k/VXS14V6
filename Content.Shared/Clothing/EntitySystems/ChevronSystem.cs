using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class ChevronSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateComponent, ExaminedEvent>(OnMobExamined);
        SubscribeLocalEvent<ChevronSlotComponent, ExaminedEvent>(OnClothingExamined);
    }

    private void OnMobExamined(EntityUid uid, MobStateComponent component, ExaminedEvent args)
    {
        // Check if the examiner can see details
        if (!args.IsInDetailsRange)
            return;

        // Check if the entity has an inventory
        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return;

        // Get all equipped clothing items
        var enumerator = _inventory.GetSlotEnumerator((uid, inventory));
        while (enumerator.NextItem(out var item, out var slot))
        {
            // Check if the item has a chevron slot component
            if (!TryComp<ChevronSlotComponent>(item, out var chevronSlot))
                continue;

            // Display information for each attached chevron
            foreach (var prototypeId in chevronSlot.AttachedChevronPrototypes)
            {
                var chevronInfo = GetChevronInfo(prototypeId);
                if (string.IsNullOrEmpty(chevronInfo))
                    continue;

                // Add the chevron information to the examination text
                args.PushMarkup(Loc.GetString("chevron-examine-text",
                    ("chevronInfo", chevronInfo)));
            }
        }
    }

    private void OnClothingExamined(EntityUid uid, ChevronSlotComponent component, ExaminedEvent args)
    {
        // Show chevron slot information when examining the clothing item directly
        args.PushMarkup(Loc.GetString("chevron-slot-component-examine-text",
            ("current", component.AttachedChevronPrototypes.Count),
            ("max", component.MaxChevrons)));
    }

    /// <summary>
    /// Attempts to attach a chevron to a clothing item.
    /// </summary>
    /// <param name="clothing">The clothing item to attach the chevron to.</param>
    /// <param name="chevron">The chevron to attach.</param>
    /// <returns>True if the chevron was successfully attached, false otherwise.</returns>
    public bool TryAttachChevron(EntityUid clothing, EntityUid chevron)
    {
        if (!TryComp<ChevronSlotComponent>(clothing, out var chevronSlot) ||
            !TryComp<ChevronComponent>(chevron, out var chevronComp))
        {
            return false;
        }

        // Get the prototype ID from the entity's metadata
        var prototypeId = MetaData(chevron).EntityPrototype?.ID;
        if (string.IsNullOrEmpty(prototypeId))
        {
            return false;
        }

        // Check if we have space for another chevron
        if (chevronSlot.AttachedChevronPrototypes.Count >= chevronSlot.MaxChevrons)
        {
            return false;
        }

        // Check if this chevron is already attached (prevent duplicates)
        if (chevronSlot.AttachedChevronPrototypes.Contains(prototypeId))
        {
            return false;
        }

        // Add the chevron prototype ID to the list
        chevronSlot.AttachedChevronPrototypes.Add(prototypeId);
        Dirty(clothing, chevronSlot);
        return true;
    }

    /// <summary>
    /// Attempts to detach a chevron from a clothing item.
    /// </summary>
    /// <param name="clothing">The clothing item to detach the chevron from.</param>
    /// <param name="prototypeId">The prototype ID of the chevron to detach.</param>
    /// <returns>True if the chevron was successfully detached, false otherwise.</returns>
    public bool TryDetachChevron(EntityUid clothing, string prototypeId)
    {
        if (!TryComp<ChevronSlotComponent>(clothing, out var chevronSlot))
        {
            return false;
        }

        // Remove the chevron prototype ID from the list
        if (chevronSlot.AttachedChevronPrototypes.Remove(prototypeId))
        {
            Dirty(clothing, chevronSlot);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the number of available chevron slots on a clothing item.
    /// </summary>
    /// <param name="clothing">The clothing item to check.</param>
    /// <returns>The number of available chevron slots.</returns>
    public int GetAvailableChevronSlots(EntityUid clothing)
    {
        if (!TryComp<ChevronSlotComponent>(clothing, out var chevronSlot))
        {
            return 0;
        }

        return Math.Max(0, chevronSlot.MaxChevrons - chevronSlot.AttachedChevronPrototypes.Count);
    }

    /// <summary>
    /// Gets the chevron information for a prototype ID.
    /// </summary>
    /// <param name="prototypeId">The prototype ID.</param>
    /// <returns>The chevron information, or null if not available.</returns>
    public string? GetChevronInfo(string prototypeId)
    {
        // For performance and safety, we'll use a cached approach or direct prototype lookup
        // For now, we'll use a simple mapping based on known prototype IDs
        return prototypeId switch
        {
            "ChevronCaptain" => "Captain",
            "ChevronHeadOfPersonnel" => "Head of Personnel",
            "ChevronHeadOfSecurity" => "Head of Security",
            "ChevronChiefMedicalOfficer" => "Chief Medical Officer",
            "ChevronResearchDirector" => "Research Director",
            _ => null
        };
    }
}
