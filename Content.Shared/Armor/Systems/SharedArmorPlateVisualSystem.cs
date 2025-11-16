using Content.Shared.Armor.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Content.Shared.Examine;

namespace Content.Shared.Armor.Systems;

/// <summary>
/// System that handles the visual states of armor plates based on their remaining resistances.
/// </summary>
public abstract class SharedArmorPlateVisualSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private protected readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArmorPlateComponent, ComponentInit>(OnArmorPlateInit);
        SubscribeLocalEvent<ArmorPlateComponent, EntInsertedIntoContainerMessage>(OnPlateInserted);
        SubscribeLocalEvent<ArmorPlateComponent, EntRemovedFromContainerMessage>(OnPlateRemoved);
    }

    private void OnArmorPlateInit(EntityUid uid, ArmorPlateComponent component, ComponentInit args)
    {
        UpdatePlateVisualState(uid, component);
    }

    private void OnPlateInserted(EntityUid uid, ArmorPlateComponent component, EntInsertedIntoContainerMessage args)
    {
        UpdatePlateVisualState(uid, component);
    }

    private void OnPlateRemoved(EntityUid uid, ArmorPlateComponent component, EntRemovedFromContainerMessage args)
    {
        // Reset to normal state when removed
        SetPlateState(uid, ArmorPlateState.Normal, component);
    }

    /// <summary>
    /// Updates the visual state of an armor plate based on its remaining resistances.
    /// </summary>
    public void UpdatePlateVisualState(EntityUid uid, ArmorPlateComponent? plate = null)
    {
        if (!Resolve(uid, ref plate))
            return;

        // Calculate the overall condition of the plate based on its spendable resistances
        var condition = CalculatePlateCondition(plate);

        // Determine the appropriate visual state
        var state = GetPlateStateFromCondition(condition, plate);

        // Update the visual state
        SetPlateState(uid, state, plate);
    }

    /// <summary>
    /// Calculates the overall condition of the plate based on its spendable resistances.
    /// Returns a value between 0.0 (completely depleted) and 1.0 (fully charged).
    /// </summary>
    private float CalculatePlateCondition(ArmorPlateComponent plate)
    {
        // For now, we'll use a simple average of all spendable resistances
        // In a more complex implementation, we might weight different resistance types differently

        var totalCurrent = 0f;
        var totalMax = 0f;

        // Check hard-spendable resistances
        foreach (var resistance in plate.HardSpendableResistances.Values)
        {
            totalCurrent += resistance;
            // We don't have the max values stored, so we'll assume current = max for now
            // In a real implementation, we'd need to store the maximum values somewhere
            totalMax += resistance;
        }

        // If no spendable resistances, assume full condition
        if (totalMax <= 0)
            return 1.0f;

        return totalCurrent / totalMax;
    }

    /// <summary>
    /// Determines the appropriate visual state based on the plate's condition.
    /// </summary>
    private ArmorPlateState GetPlateStateFromCondition(float condition, ArmorPlateComponent plate)
    {
        if (condition <= plate.BrokenThreshold)
            return ArmorPlateState.Broken;
        else if (condition <= plate.DamagedThreshold)
            return ArmorPlateState.Damaged;
        else
            return ArmorPlateState.Normal;
    }

    /// <summary>
    /// Sets the visual state of an armor plate.
    /// </summary>
    public void SetPlateState(EntityUid uid, ArmorPlateState state, ArmorPlateComponent? plate = null)
    {
        if (!Resolve(uid, ref plate))
            return;

        // Update the appearance
        _appearance.SetData(uid, ArmorPlateVisuals.State, state);
    }
}
