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

        // Check if we can insert the plate (either empty slot or swapping)
        if (!_itemSlots.CanInsert(uid, args.Used, args.User, component.PlateSlot, swap: true))
            return;

        // Start the DoAfter for plate insertion (whether empty slot or swapping)
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
        if (args.Handled || args.Cancelled || args.User == null || args.Used == null)
            return;

        // Check if the used item is still valid
        if (!HasComp<ArmorPlateComponent>(args.Used.Value))
            return;

        // Check if we can still insert (either empty slot or swapping)
        if (!_itemSlots.CanInsert(uid, args.Used.Value, args.User, component.PlateSlot, swap: true))
            return;

        // If there's an item in the slot, try to eject it to the user's hands first
        if (component.PlateSlot.HasItem)
        {
            _itemSlots.TryEjectToHands(uid, component.PlateSlot, args.User);
        }

        // Insert the new plate using TryInsertFromHand since we're inserting from the user's hand
        _itemSlots.TryInsertFromHand(uid, component.PlateSlot, args.User);
        args.Handled = true;
    }
}
