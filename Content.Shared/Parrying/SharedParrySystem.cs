using Content.Shared.Actions;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Parrying;

public abstract class SharedParrySystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming _timing = default!;
    [Dependency] protected readonly SharedStunSystem _stun = default!;
    [Dependency] protected readonly SharedActionsSystem _action = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<ParryComponent, ComponentInit>(OnParryInit);
        SubscribeLocalEvent<ParryComponent, ComponentRemove>(OnParryRemove);
        SubscribeLocalEvent<ParryComponent, ParryActionEvent>(OnParryAction);
    }

    // todo override on client to add ping to duration
    // todo give up, this shit works awful with high ping (and ping is always high in this mess of a game)
    protected abstract void OnParryAction(EntityUid uid, ParryComponent comp, ParryActionEvent args);

    protected abstract void DoParryEffect(EntityUid uid);

    private void OnParryInit(EntityUid uid, ParryComponent comp, ComponentInit args)
    {
        _action.AddAction(uid, ref comp.ParryActionEntity, comp.ParryAction);
    }

    private void OnParryRemove(EntityUid uid, ParryComponent comp, ComponentRemove args)
    {
        _action.RemoveAction(comp.ParryActionEntity);
    }

    public bool IsParrying(EntityUid uid, ParryComponent? comp = null) => Resolve(uid, ref comp, false) && _timing.CurTime <= comp.ExpirationTime;
    public bool IsParrying(EntityUid uid, [NotNullWhen(true)] ref ParryComponent? comp) => Resolve(uid, ref comp, false) && _timing.CurTime <= comp.ExpirationTime;

    public void StartParry(EntityUid uid, float seconds = 1f, ParryComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.ExpirationTime = _timing.CurTime + TimeSpan.FromSeconds(seconds);
        //return comp;
    }

    public bool Rekt(EntityUid uid, EntityUid parriedBy, ParryComponent? comp = null)
    {
        if (!Resolve(parriedBy, ref comp))
            return false;

        return Rekt(uid, comp);
    }

    public bool Rekt(EntityUid uid, ParryComponent comp)
    {
        // maybe don't parry your own attack?
        // right now only prevents stun and slowdown, the attack is still forced to miss
        if (uid == comp.Owner)
            return false;
        _stun.TryStun(uid, TimeSpan.FromSeconds(comp.StunDuration), false);
        _stun.TrySlowdown(uid, TimeSpan.FromSeconds(comp.SlowdownDuration + comp.StunDuration), false);
        return true;
    }
}



[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ParryComponent : Component
{
    [DataField]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float StunDuration = 0.5f;

    [DataField]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float SlowdownDuration = 1f;

    [DataField]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float ParryDuration = 1f;

    [DataField]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier ActivationSound = new SoundPathSpecifier("/Audio/Weapons/soup.ogg");

    [DataField]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId? EffectPrototype = "EffectEmpDisabled";

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public string ParryAction = "ActionParry";

    [DataField]
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ParryActionEntity;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ExpirationTime;
}


public sealed partial class ParryActionEvent : InstantActionEvent
{

}
