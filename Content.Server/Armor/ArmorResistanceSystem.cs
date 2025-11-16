using Content.Shared.Armor;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Server.Armor;

/// <summary>
/// System that handles the consumption of hard-spendable resistances on armor
/// </summary>
public sealed class ArmorResistanceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArmorComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnDamageModify(EntityUid uid, ArmorComponent component, DamageModifyEvent args)
    {
        // Apply hard resistance consumption logic
        ConsumeHardResistances(uid, component, args.Damage);
    }

    /// <summary>
    /// Consumes hard-spendable resistances based on damage taken
    /// </summary>
    private void ConsumeHardResistances(EntityUid uid, ArmorComponent component, DamageSpecifier damage)
    {
        // Initialize current resistances if they're empty
        component.InitializeCurrentResistances();

        // Consume hard-spendable resistances
        foreach (var (damageType, damageValue) in damage.DamageDict)
        {
            if (damageValue <= 0)
                continue;

            var damageFloat = damageValue.Float();

            // Consume hard-spendable resistances
            if (component.CurrentHardSpendableResistances.TryGetValue(damageType, out var currentHardSpendable) && currentHardSpendable > 0)
            {
                var consumed = Math.Min(damageFloat, currentHardSpendable);
                component.ConsumeHardSpendableResistance(damageType, consumed);
            }

            // Consume hard-spendable-percent resistances
            if (component.CurrentHardSpendablePercentResistances.TryGetValue(damageType, out var currentHardSpendablePercent) && currentHardSpendablePercent > 0)
            {
                // For percent-based resistances, we reduce the percentage value
                // If we have 25% resistance, we might reduce it by a fixed amount or percentage
                // For this implementation, let's reduce it by a fixed amount per damage point
                var reduction = Math.Min(0.1f, currentHardSpendablePercent); // Reduce by 0.1% per damage point, up to the current value
                component.ConsumeHardSpendablePercentResistance(damageType, reduction);
            }
        }
    }

    /// <summary>
    /// Resets all consumable resistances to their maximum values
    /// </summary>
    public void ResetResistances(EntityUid uid, ArmorComponent component)
    {
        component.ResetResistances();
    }
}
