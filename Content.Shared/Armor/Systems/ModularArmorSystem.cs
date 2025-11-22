using Content.Shared.Armor.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Shared.Armor.Systems;

/// <summary>
/// System that handles modular armor with insertable plates.
/// </summary>
public sealed class ModularArmorSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Removed duplicate subscription - handled in SharedModularArmorSystem
    }
}
