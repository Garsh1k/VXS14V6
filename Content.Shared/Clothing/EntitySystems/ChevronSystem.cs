using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class ChevronSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

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
            _ = slot;

            // Check if the item has a chevron slot component
            if (!TryComp<ChevronSlotComponent>(item, out var chevronSlot))
                continue;

            // Display information for each attached chevron
            for (var i = 0; i < chevronSlot.AttachedChevronPrototypes.Count; i++)
            {
                var chevronInfo = GetChevronInfo(chevronSlot, i);
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
        chevronSlot.AttachedChevronInfos.Add(chevronComp.ChevronInfo);
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

        var index = chevronSlot.AttachedChevronPrototypes.IndexOf(prototypeId);
        if (index < 0)
            return false;

        chevronSlot.AttachedChevronPrototypes.RemoveAt(index);

        if (index < chevronSlot.AttachedChevronInfos.Count)
            chevronSlot.AttachedChevronInfos.RemoveAt(index);

        Dirty(clothing, chevronSlot);
        return true;
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

    public string? GetChevronInfo(ChevronSlotComponent component, int index)
    {
        if (index < 0 || index >= component.AttachedChevronPrototypes.Count)
            return null;

        if (index < component.AttachedChevronInfos.Count && !string.IsNullOrWhiteSpace(component.AttachedChevronInfos[index]))
            return component.AttachedChevronInfos[index];

        return component.AttachedChevronPrototypes[index];
    }
}
