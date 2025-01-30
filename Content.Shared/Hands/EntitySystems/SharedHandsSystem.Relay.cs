using Content.Shared.Hands.Components;
using Content.Shared.MouseRotator;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<HandsComponent, RefreshMovementSpeedModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<HandsComponent, MoveEvent>(RelayMoveEvent); // WD EDIT
        //SubscribeLocalEvent<HandsComponent, RequestMouseRotatorRotationEvent>(RelayMouseRotatorEvent, after: [typeof(SharedMouseRotatorSystem)]); // WD EDIT
    }

    private void RelayEvent<T>(Entity<HandsComponent> entity, ref T args) where T : EntityEventArgs
    {
        var ev = new HeldRelayedEvent<T>(args);
        foreach (var held in EnumerateHeld(entity, entity.Comp))
        {
            RaiseLocalEvent(held, ref ev);
        }
    }

    //WD EDIT START
    private void RelayMoveEvent(EntityUid uid, HandsComponent comp, ref MoveEvent args)
    {
        var ev = new HolderMoveEvent(args);
        foreach (var itemUid in EnumerateHeld(uid, comp))
        {
            RaiseLocalEvent(itemUid, ref ev);
        }
    }

    //private void RelayMouseRotatorEvent(EntityUid uid, HandsComponent comp, RequestMouseRotatorRotationEvent args)
    //{
    //    var ev = new HolderRotateEvent(args);
    //    foreach (var itemUid in EnumerateHeld(uid, comp))
    //    {
    //        RaiseLocalEvent(itemUid, ref ev);
    //    }
    //}
    //WD EDIT END
}

[ByRefEvent]
public readonly struct HolderMoveEvent(MoveEvent ev)
{
    public readonly MoveEvent Ev = ev;
}

//[ByRefEvent]
//public readonly struct HolderRotateEvent(RequestMouseRotatorRotationEvent ev)
//{
//    public readonly RequestMouseRotatorRotationEvent Ev = ev;
//}
