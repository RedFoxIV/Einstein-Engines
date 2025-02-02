using Content.Shared.Hands.EntitySystems;
using Content.Shared.RenderOrderSystem;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client.DrawOrderVisualizer;

public sealed class RenderOrderSystem : SharedRenderOrderSystem
{
    public readonly uint DefaultRenderOrder = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RenderOrderComponent, AfterAutoHandleStateEvent>((uid, comp, args) => UpdateRenderOrder(uid, comp));
    }


    protected override void UpdateRenderOrder(EntityUid uid, RenderOrderComponent comp)
    {
        //base.UpdateRenderOrder(uid, comp);

        DebugTools.Assert(comp.ValueOrder.Count == comp.Values.Count, "comp.Values and comp.ValueOrder have different entry counts");
        var sprite = Comp<SpriteComponent>(uid);
        if(comp.ValueOrder.Count == 0)
        {
            sprite.RenderOrder = DefaultRenderOrder;
            return;
        }
        sprite.RenderOrder = comp.Values[comp.ValueOrder.Last()];
    }
}


