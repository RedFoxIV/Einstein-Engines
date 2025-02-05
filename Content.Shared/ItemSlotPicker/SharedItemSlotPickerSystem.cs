using Content.Shared.ActionBlocker;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.ItemSlotPicker.UI;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.ItemSlotPicker;

public abstract class SharedItemSlotPickerSystem : EntitySystem
{
    [Dependency] protected readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] protected readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] protected readonly ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ItemSlotPickerComponent, ComponentInit>(CompInit);
        SubscribeLocalEvent<ItemSlotPickerComponent, AlternativeInteractionEvent>(AltInteract);
        SubscribeLocalEvent<ItemSlotPickerComponent, ItemSlotPickerSlotPickedMessage>(OnMessage);
        SubscribeLocalEvent<ItemSlotPickerComponent, EntInsertedIntoContainerMessage>(EntInserted);
        SubscribeLocalEvent<ItemSlotPickerComponent, EntRemovedFromContainerMessage>(EntRemoved);
        SubscribeLocalEvent<ItemSlotPickerComponent, ItemSlotPickerSlotPickedMessage>(OnMessage);
    }

    protected virtual void CompInit(EntityUid uid, ItemSlotPickerComponent comp, ComponentInit args)
    {
        _ui.SetUi(uid, ItemSlotPickerKey.Key, new InterfaceData("ItemSlotPickerBoundUserInterface"));
    }

    protected virtual void AltInteract(EntityUid uid, ItemSlotPickerComponent comp, AlternativeInteractionEvent args)
    {
        var user = args.User;
        if (!TryComp<ItemSlotsComponent>(uid, out var slots) ||
            !TryComp<HandsComponent>(user, out var hands) ||
            !_blocker.CanComplexInteract(user) ||
            !_blocker.CanInteract(user, uid))
            return;

        //if (hands.ActiveHandEntity is not null) // let the ItemSlotSystem handle the insertion, if it wants to.
        //    return;

        if(hands.ActiveHandEntity is EntityUid item)
        {
            // simulating normal ItemSlotComponent behaviour when alt-clicked with an item
            // either this item goes into an item slot,
            // or some other alt verb gets called.

            // In our case, either this item gets inserted, or we open the pick menu.
            foreach(ItemSlot slot in slots.Slots.Values)
                if (!slot.InsertOnInteract && _itemSlots.TryInsert(uid, slot, item, user, slots))
                    return;
        }

        args.Handled = true;
        _ui.TryToggleUi(uid, ItemSlotPickerKey.Key, user);
    }

    protected virtual void EntInserted(EntityUid uid, ItemSlotPickerComponent comp, EntInsertedIntoContainerMessage args)
    {
        _ui.RaiseUiMessage(uid, ItemSlotPickerKey.Key, new ItemSlotPickerContentsChangedMessage());
    }

    protected virtual void EntRemoved(EntityUid uid, ItemSlotPickerComponent comp, EntRemovedFromContainerMessage args)
    {
        _ui.RaiseUiMessage(uid, ItemSlotPickerKey.Key, new ItemSlotPickerContentsChangedMessage());
    }


    protected virtual void OnMessage(EntityUid uid, ItemSlotPickerComponent comp, ItemSlotPickerSlotPickedMessage args)
    {
        if (!comp.ItemSlots.Contains(args.SlotId) ||
            !_itemSlots.TryGetSlot(uid, args.SlotId, out var slot))
            return;

        _itemSlots.TryEjectToHands(uid, slot, args.Actor);
        _ui.CloseUi(uid, ItemSlotPickerKey.Key, args.Actor);
    }
}
[Serializable, NetSerializable]
public enum ItemSlotPickerKey { Key };
