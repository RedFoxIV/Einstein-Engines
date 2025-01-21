using Robust.Shared.Player;

namespace Content.Shared.Effects;

public abstract class SharedColorFlashEffectSystem : EntitySystem
{
    protected const float AnimationHoldTimeDefault = 0f;
    protected const float AnimationFadeTimeDefault = 0.30f;

    public abstract void RaiseEffect(Color color, float holdTime, float fadeTime, List<EntityUid> entities, Filter? filter = null);
    public abstract void RaiseEffect(Color color, List<EntityUid> entities, Filter? filter = null);
}
