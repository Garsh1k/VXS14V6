using Content.Shared.Armor.Components;
using Content.Shared.Armor.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Armor;

namespace Content.Server.Armor;

/// <summary>
/// Server-side system for modular armor functionality.
/// </summary>
public sealed class ModularArmorSystem : SharedModularArmorSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModularArmorComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ModularArmorComponent, PlateInsertDoAfterEvent>(OnPlateInsertDoAfter);
    }

    private void OnInteractUsing(EntityUid uid, ModularArmorComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Check if the used item is an armor plate
        if (!HasComp<ArmorPlateComponent>(args.Used))
            return;

        // Check if the plate slot is available
        if (component.PlateSlot.HasItem)
            return;

        // Start the DoAfter for plate insertion
        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(8), new PlateInsertDoAfterEvent(), uid, target: uid, used: args.Used)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 0.1f
        };

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
        args.Handled = true;
    }

    private void OnPlateInsertDoAfter(EntityUid uid, ModularArmorComponent component, PlateInsertDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        // Check if the used item is still valid and the slot is still available
        if (!HasComp<ArmorPlateComponent>(args.Used) || component.PlateSlot.HasItem)
            return;

        // Insert the plate using TryInsertFromHand since we're inserting from the user's hand
        _itemSlots.TryInsertFromHand(uid, component.PlateSlot, args.User);
        args.Handled = true;
    }
}
