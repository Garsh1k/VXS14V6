using Content.Shared.Armor.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Tag;
using Robust.Shared.Containers;

namespace Content.Shared.Armor.Systems;

/// <summary>
/// Shared system for modular armor functionality.
/// </summary>
public abstract class SharedModularArmorSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModularArmorComponent, ComponentInit>(OnModularArmorInit);
        SubscribeLocalEvent<ModularArmorComponent, EntInsertedIntoContainerMessage>(OnPlateInserted);
        SubscribeLocalEvent<ModularArmorComponent, EntRemovedFromContainerMessage>(OnPlateRemoved);
    }

    private void OnModularArmorInit(EntityUid uid, ModularArmorComponent component, ComponentInit args)
    {
        // Initialize the item slot
        if (component.PlateSlot.ContainerSlot == null)
        {
            _itemSlots.AddItemSlot(uid, "Plate", component.PlateSlot);
        }
    }

    private void OnPlateInserted(EntityUid uid, ModularArmorComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != "Plate")
            return;

        // Get the plate's modifiers and apply them
        if (TryComp<ArmorPlateComponent>(args.Entity, out var plate))
        {
            component.PlateCoefficients = new Dictionary<string, float>(plate.Coefficients);
            component.PlateFlatReductions = new Dictionary<string, float>(plate.FlatReductions);
            component.PlateHardResistances = new Dictionary<string, float>(plate.HardResistances);
            component.PlateHardSpendableResistances = new Dictionary<string, float>(plate.HardSpendableResistances);
            component.PlateHardSpendablePercentResistances = new Dictionary<string, float>(plate.HardSpendablePercentResistances);
        }
    }

    private void OnPlateRemoved(EntityUid uid, ModularArmorComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != "Plate")
            return;

        // Clear all plate modifiers
        component.PlateCoefficients.Clear();
        component.PlateFlatReductions.Clear();
        component.PlateHardResistances.Clear();
        component.PlateHardSpendableResistances.Clear();
        component.PlateHardSpendablePercentResistances.Clear();
    }
}
