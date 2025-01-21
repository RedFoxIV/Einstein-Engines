using Content.Shared.Effects;
using Robust.Shared.Player;

namespace Content.Server.Effects;

public sealed class ColorFlashEffectSystem : SharedColorFlashEffectSystem
{
    public override void RaiseEffect(Color color, List<EntityUid> entities, Filter? filter = null)
    {
        if (entities.Count == 0)
            return;
        filter ??= Filter.Pvs(Transform(entities[0]).Coordinates, entityMan: EntityManager);
        RaiseNetworkEvent(new ColorFlashEffectEvent(color, GetNetEntityList(entities)), filter);
    }

    public override void RaiseEffect(Color color, float holdTime, float fadeTime, List<EntityUid> entities, Filter? filter = null)
    {
        if (entities.Count == 0)
            return;
        filter ??= Filter.Pvs(Transform(entities[0]).Coordinates, entityMan: EntityManager);
        RaiseNetworkEvent(new ColorFlashEffectEvent(color, GetNetEntityList(entities), holdTime, fadeTime), filter);
    }
}
