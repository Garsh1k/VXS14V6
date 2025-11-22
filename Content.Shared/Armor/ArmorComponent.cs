using Content.Shared.Damage;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Armor;

/// <summary>
/// Used for clothing that reduces damage when worn.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ArmorComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    /// <summary>
    /// Hard resistances that fully consume a set amount of damage
    /// These are static and don't get consumed
    /// </summary>
    [DataField]
    public Dictionary<string, float> HardResistances = new();

    /// <summary>
    /// Hard-spendable resistances that consume damage but lower by that amount
    /// These get consumed as damage is taken
    /// </summary>
    [DataField]
    public Dictionary<string, float> HardSpendableResistances = new();

    /// <summary>
    /// Hard-spendable-percent resistances that consume damage but lower by a percentage of the consumed damage
    /// These get consumed as damage is taken
    /// </summary>
    [DataField]
    public Dictionary<string, float> HardSpendablePercentResistances = new();

    /// <summary>
    /// Current values of hard-spendable resistances (gets consumed)
    /// </summary>
    [DataField]
    public Dictionary<string, float> CurrentHardSpendableResistances = new();

    /// <summary>
    /// Current values of hard-spendable-percent resistances (gets consumed)
    /// </summary>
    [DataField]
    public Dictionary<string, float> CurrentHardSpendablePercentResistances = new();

    /// <summary>
    /// Gets a DamageModifierSet with current resistance values
    /// </summary>
    public DamageModifierSet GetCurrentModifiers()
    {
        var modifiers = new DamageModifierSet
        {
            Coefficients = new Dictionary<string, float>(Modifiers.Coefficients),
            FlatReduction = new Dictionary<string, float>(Modifiers.FlatReduction),
            HardResistances = new Dictionary<string, float>(HardResistances)
        };

        // Add current hard-spendable resistances
        foreach (var kvp in CurrentHardSpendableResistances)
        {
            modifiers.HardSpendableResistances[kvp.Key] = kvp.Value;
        }

        // Add current hard-spendable-percent resistances
        foreach (var kvp in CurrentHardSpendablePercentResistances)
        {
            modifiers.HardSpendablePercentResistances[kvp.Key] = kvp.Value;
        }

        return modifiers;
    }

    /// <summary>
    /// Resets all consumable resistances to their maximum values
    /// </summary>
    public void ResetResistances()
    {
        CurrentHardSpendableResistances.Clear();
        CurrentHardSpendablePercentResistances.Clear();
    }

    /// <summary>
    /// Consumes hard-spendable resistances
    /// </summary>
    public void ConsumeHardSpendableResistance(string damageType, float amount)
    {
        if (CurrentHardSpendableResistances.TryGetValue(damageType, out var current) && current > 0)
        {
            CurrentHardSpendableResistances[damageType] = Math.Max(0, current - amount);
        }
    }

    /// <summary>
    /// Consumes hard-spendable-percent resistances
    /// </summary>
    public void ConsumeHardSpendablePercentResistance(string damageType, float amount)
    {
        if (CurrentHardSpendablePercentResistances.TryGetValue(damageType, out var current) && current > 0)
        {
            CurrentHardSpendablePercentResistances[damageType] = Math.Max(0, current - amount);
        }
    }

    /// <summary>
    /// Initializes current resistances if they're empty
    /// </summary>
    public void InitializeCurrentResistances()
    {
        if (CurrentHardSpendableResistances.Count == 0)
        {
            foreach (var kvp in HardSpendableResistances)
            {
                CurrentHardSpendableResistances[kvp.Key] = kvp.Value;
            }
        }

        if (CurrentHardSpendablePercentResistances.Count == 0)
        {
            foreach (var kvp in HardSpendablePercentResistances)
            {
                CurrentHardSpendablePercentResistances[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// A multiplier applied to the calculated point value
    /// to determine the monetary value of the armor
    /// </summary>
    [DataField]
    public float PriceMultiplier = 1;

    /// <summary>
    /// If true, you can examine the armor to see the protection. If false, the verb won't appear.
    /// </summary>
    [DataField]
    public bool ShowArmorOnExamine = true;
}

/// <summary>
/// Event raised on an armor entity to get additional examine text relating to its armor.
/// </summary>
/// <param name="Msg"></param>
[ByRefEvent]
public record struct ArmorExamineEvent(FormattedMessage Msg);

/// <summary>
/// A Relayed inventory event, gets the total Armor for all Inventory slots defined by the Slotflags in TargetSlots
/// </summary>
public sealed class CoefficientQueryEvent : EntityEventArgs, IInventoryRelayEvent
{
    /// <summary>
    /// All slots to relay to
    /// </summary>
    public SlotFlags TargetSlots { get; set; }

    /// <summary>
    /// The Total of all Coefficients.
    /// </summary>
    public DamageModifierSet DamageModifiers { get; set; } = new DamageModifierSet();

    public CoefficientQueryEvent(SlotFlags slots)
    {
        TargetSlots = slots;
    }
}
