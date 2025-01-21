using Content.Shared.Effects;
using Content.Shared.Parrying;
using Robust.Client.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client.Parrying;

public sealed class ParrySystem : SharedParrySystem
{
    [Dependency] protected readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;

    protected override void OnParryAction(EntityUid uid, ParryComponent comp, ParryActionEvent args)
    {
        comp.ExpirationTime = _timing.CurTime + TimeSpan.FromSeconds(comp.ParryDuration + _player.LocalSession!.Channel.Ping/1000f);
        args.Handled = true;
    }

    protected override void DoParryEffect(EntityUid uid)
    {
        _color.RaiseEffect(Color.Ivory, new List<EntityUid>() { uid }, Filter.Local());
    }
}
