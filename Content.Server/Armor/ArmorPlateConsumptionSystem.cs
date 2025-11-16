using Content.Shared.Armor;
using Content.Shared.Armor.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;
using Robust.Server.Containers;
using Robust.Shared.Utility;

namespace Content.Server.Armor;

/// <summary>
/// System that handles the consumption of spendable resistances on armor plates.
/// </summary>
public sealed class ArmorPlateConsumptionSystem : EntitySystem
{
    [Dependency] private readonly SharedArmorSystem _armor = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageableComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnDamageModify(EntityUid uid, DamageableComponent component, DamageModifyEvent args)
    {
        // Get all equipped armor items
        var armorEntities = new List<EntityUid>();
        if (_inventory.TryGetContainerSlotEnumerator(uid, out var containerSlotEnumerator))
        {
            while (containerSlotEnumerator.MoveNext(out var slot))
            {
                if (slot.ContainedEntity is { Valid: true } containedEntity && HasComp<ModularArmorComponent>(containedEntity))
                {
                    armorEntities.Add(containedEntity);
                }
            }
        }

        // For each armor item with plates, consume spendable resistances
        foreach (var armorEntity in armorEntities)
        {
            if (!TryComp<ModularArmorComponent>(armorEntity, out var modularArmor))
            {
                continue;
            }

            // Only consume if there are spendable resistances
            if (modularArmor.PlateHardSpendableResistances.Count > 0 ||
                modularArmor.PlateHardSpendablePercentResistances.Count > 0)
            {
                ConsumeSpendableResistances(armorEntity, modularArmor, args.Damage);
            }
        }
    }

    private void ConsumeSpendableResistances(EntityUid armorUid, ModularArmorComponent modularArmor, DamageSpecifier damage)
    {
        // We need to find the actual plate entity to update its component
        if (!_container.TryGetContainer(armorUid, "Plate", out var container) ||
            container.ContainedEntities.Count == 0)
        {
            return;
        }

        var plateEntity = container.ContainedEntities[0];
        if (!TryComp<ArmorPlateComponent>(plateEntity, out var armorPlate))
        {
            return;
        }

        bool needsUpdate = false;

        // Consume hard-spendable resistances
        foreach (var kvp in modularArmor.PlateHardSpendableResistances)
        {
            var damageType = kvp.Key;
            var availableResistance = kvp.Value;

            if (availableResistance <= 0 || !damage.DamageDict.TryGetValue(damageType, out var damageAmount) || damageAmount <= 0)
            {
                continue;
            }

            // Calculate how much resistance we can consume
            var consumed = MathF.Min(availableResistance, damageAmount.Float());

            // Update both the plate component and the modular armor component
            if (armorPlate.HardSpendableResistances.ContainsKey(damageType))
            {
                armorPlate.HardSpendableResistances[damageType] = MathF.Max(0, armorPlate.HardSpendableResistances[damageType] - consumed);
                modularArmor.PlateHardSpendableResistances[damageType] = MathF.Max(0, modularArmor.PlateHardSpendableResistances[damageType] - consumed);
                needsUpdate = true;
            }
        }

        // Consume hard-spendable-percent resistances
        foreach (var kvp in modularArmor.PlateHardSpendablePercentResistances)
        {
            var damageType = kvp.Key;
            var availablePercentage = kvp.Value; // This is a percentage value (0.0-1.0)

            if (availablePercentage <= 0 || !damage.DamageDict.TryGetValue(damageType, out var damageAmount) || damageAmount <= 0)
            {
                continue;
            }

            // Calculate how much resistance we can consume (percentage of damage)
            var consumed = MathF.Min(availablePercentage * damageAmount.Float(), damageAmount.Float());

            // For percentage resistances, we reduce the percentage value itself
            // Calculate how much percentage to consume based on the damage consumed
            if (consumed > 0 && damageAmount > 0)
            {
                var percentConsumed = consumed / damageAmount.Float();

                // Update both the plate component and the modular armor component
                if (armorPlate.HardSpendablePercentResistances.ContainsKey(damageType))
                {
                    armorPlate.HardSpendablePercentResistances[damageType] = MathF.Max(0, armorPlate.HardSpendablePercentResistances[damageType] - percentConsumed);
                    modularArmor.PlateHardSpendablePercentResistances[damageType] = MathF.Max(0, modularArmor.PlateHardSpendablePercentResistances[damageType] - percentConsumed);
                    needsUpdate = true;
                }
            }
        }

        // Only dirty the components if we actually consumed something
        if (needsUpdate)
        {
            Dirty(plateEntity, armorPlate);
            Dirty(armorUid, modularArmor);
        }
    }
}
