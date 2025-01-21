using Robust.Shared.Serialization;

namespace Content.Shared.Effects;

/// <summary>
/// Raised on the server and sent to a client to play the color flash animation.
/// </summary>
[Serializable, NetSerializable]
public sealed class ColorFlashEffectEvent : EntityEventArgs
{
    /// <summary>
    /// Color to play for the flash.
    /// </summary>
    public Color Color;

    public List<NetEntity> Entities;

    public float? HoldTime, FadeTime;

    public ColorFlashEffectEvent(Color color, List<NetEntity> entities, float? holdTime = null, float? fadeTime = null)
    {
        Color = color;
        Entities = entities;
        HoldTime = holdTime;
        FadeTime = fadeTime;
    }
}
