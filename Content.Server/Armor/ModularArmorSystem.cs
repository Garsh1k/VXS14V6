using Content.Shared.Armor.Components;
using Content.Shared.Armor.Systems;
using Content.Shared.Containers.ItemSlots;

namespace Content.Server.Armor;

/// <summary>
/// Server-side system for modular armor functionality.
/// </summary>
public sealed class ModularArmorSystem : SharedModularArmorSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
