using Content.Shared.Effects;
using Content.Shared.Parrying;
using Robust.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Parrying;

public sealed class ParrySystem : SharedParrySystem
{
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;


    // todo: parryactivatedevent; move visual stuff to client handling that event
    protected override void DoParryEffect(EntityUid uid)
    {
        _color.RaiseEffect(Color.DarkCyan, 0.4f, 0.1f, new List<EntityUid>() { uid });
    }

    protected override void OnParryAction(EntityUid uid, ParryComponent comp, ParryActionEvent args)
    {
        comp.ExpirationTime = _timing.CurTime + TimeSpan.FromSeconds(comp.ParryDuration);
        _audio.PlayPvs(comp.ActivationSound, uid);
        //if (comp.EffectPrototype.HasValue && !_timing.InPrediction)
        //    Spawn(comp.EffectPrototype.Value, new Robust.Shared.Map.EntityCoordinates(uid, 0, 0));
        //Dirty(uid, comp);
        DoParryEffect(uid);
        args.Handled = true;
    }
}
