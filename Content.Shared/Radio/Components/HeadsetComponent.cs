using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Radio.Components;

/// <summary>
/// This component relays radio messages to the parent entity's chat when equipped.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeadsetComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    [DataField, AutoNetworkedField]
    public bool IsEquipped = false;

    [DataField, AutoNetworkedField]
    public SlotFlags RequiredSlot = SlotFlags.EARS;

    /// <summary>
    /// The sound effect played when headset receive message
    /// </summary>
    [DataField]
    public SoundSpecifier MessageReceiveSound = new SoundPathSpecifier("/Audio/Effects/radio_receive.ogg");

    /// <summary>
    /// The sound effect played when headset send message
    /// </summary>
    [DataField]
    public SoundSpecifier MessageSendSound = new SoundPathSpecifier("/Audio/Effects/radio_talk.ogg");
}
