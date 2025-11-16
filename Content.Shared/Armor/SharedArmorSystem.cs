using Content.Shared.Armor.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared.Armor;

/// <summary>
///     This handles logic relating to <see cref="ArmorComponent" />
/// </summary>
public abstract class SharedArmorSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArmorComponent, InventoryRelayedEvent<CoefficientQueryEvent>>(OnCoefficientQuery);
        SubscribeLocalEvent<ArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<ArmorComponent, BorgModuleRelayedEvent<DamageModifyEvent>>(OnBorgDamageModify);
        SubscribeLocalEvent<ArmorComponent, GetVerbsEvent<ExamineVerb>>(OnArmorVerbExamine);
    }

    /// <summary>
    /// Get the total Damage reduction value of all equipment caught by the relay.
    /// </summary>
    /// <param name="ent">The item that's being relayed to</param>
    /// <param name="args">The event, contains the running count of armor percentage as a coefficient</param>
    private void OnCoefficientQuery(Entity<ArmorComponent> ent, ref InventoryRelayedEvent<CoefficientQueryEvent> args)
    {
        if (TryComp<MaskComponent>(ent, out var mask) && mask.IsToggled)
            return;

        // Use current modifiers for coefficient calculation
        var currentModifiers = ent.Comp.GetCurrentModifiers();
        foreach (var armorCoefficient in currentModifiers.Coefficients)
        {
            args.Args.DamageModifiers.Coefficients[armorCoefficient.Key] = args.Args.DamageModifiers.Coefficients.TryGetValue(armorCoefficient.Key, out var coefficient) ? coefficient * armorCoefficient.Value : armorCoefficient.Value;
        }

        // Apply modular armor plate modifiers if present
        if (TryComp<ModularArmorComponent>(ent, out var modularArmor))
        {
            foreach (var plateModifier in modularArmor.PlateModifiers)
            {
                // Apply plate modifiers as additional coefficients
                args.Args.DamageModifiers.Coefficients[plateModifier.Key] = args.Args.DamageModifiers.Coefficients.TryGetValue(plateModifier.Key, out var coefficient)
                    ? coefficient * plateModifier.Value
                    : plateModifier.Value;
            }
        }
    }

    private void OnDamageModify(EntityUid uid, ArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (TryComp<MaskComponent>(uid, out var mask) && mask.IsToggled)
            return;

        // Use current modifiers for damage modification
        var currentModifiers = component.GetCurrentModifiers();
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, currentModifiers);

        // Apply modular armor plate modifiers if present
        if (TryComp<ModularArmorComponent>(uid, out var modularArmor))
        {
            var plateModifiers = new DamageModifierSet
            {
                Coefficients = new Dictionary<string, float>(modularArmor.PlateModifiers)
            };
            args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, plateModifiers);
        }
    }

    private void OnBorgDamageModify(EntityUid uid, ArmorComponent component,
        ref BorgModuleRelayedEvent<DamageModifyEvent> args)
    {
        if (TryComp<MaskComponent>(uid, out var mask) && mask.IsToggled)
            return;

        // Use current modifiers for damage modification
        var currentModifiers = component.GetCurrentModifiers();
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, currentModifiers);

        // Apply modular armor plate modifiers if present
        if (TryComp<ModularArmorComponent>(uid, out var modularArmor))
        {
            var plateModifiers = new DamageModifierSet
            {
                Coefficients = new Dictionary<string, float>(modularArmor.PlateModifiers)
            };
            args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, plateModifiers);
        }
    }

    private void OnArmorVerbExamine(EntityUid uid, ArmorComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !component.ShowArmorOnExamine)
            return;

        var currentModifiers = component.GetCurrentModifiers();
        var examineMarkup = GetArmorExamine(currentModifiers);

        // Add plate information if present
        if (TryComp<ModularArmorComponent>(uid, out var modularArmor) && modularArmor.PlateModifiers.Count > 0)
        {
            examineMarkup.PushNewline();
            examineMarkup.AddMarkupOrThrow("[color=yellow]Plate Bonuses:[/color]");

            foreach (var plateModifier in modularArmor.PlateModifiers)
            {
                examineMarkup.PushNewline();
                var armorType = Loc.GetString("armor-damage-type-" + plateModifier.Key.ToLower());
                examineMarkup.AddMarkupOrThrow(Loc.GetString("armor-coefficient-value",
                    ("type", armorType),
                    ("value", MathF.Round((1f - plateModifier.Value) * 100, 1))
                ));
            }
        }

        var ev = new ArmorExamineEvent(examineMarkup);
        RaiseLocalEvent(uid, ref ev);

        _examine.AddDetailedExamineVerb(args, component, examineMarkup,
            Loc.GetString("armor-examinable-verb-text"), "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("armor-examinable-verb-message"));
    }

    private FormattedMessage GetArmorExamine(DamageModifierSet armorModifiers)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("armor-examine"));

        foreach (var coefficientArmor in armorModifiers.Coefficients)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + coefficientArmor.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-coefficient-value",
                ("type", armorType),
                ("value", MathF.Round((1f - coefficientArmor.Value) * 100, 1))
            ));
        }

        foreach (var flatArmor in armorModifiers.FlatReduction)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + flatArmor.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-reduction-value",
                ("type", armorType),
                ("value", flatArmor.Value)
            ));
        }

        // Add hard resistances to examination
        foreach (var hardResistance in armorModifiers.HardResistances)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + hardResistance.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-hard-resistance-value",
                ("type", armorType),
                ("value", hardResistance.Value)
            ));
        }

        // Add hard-spendable resistances to examination
        foreach (var hardSpendableResistance in armorModifiers.HardSpendableResistances)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + hardSpendableResistance.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-hard-spendable-resistance-value",
                ("type", armorType),
                ("value", hardSpendableResistance.Value)
            ));
        }

        // Add hard-spendable-percent resistances to examination
        foreach (var hardSpendablePercentResistance in armorModifiers.HardSpendablePercentResistances)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + hardSpendablePercentResistance.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-hard-spendable-percent-resistance-value",
                ("type", armorType),
                ("value", hardSpendablePercentResistance.Value)
            ));
        }

        return msg;
    }
}
