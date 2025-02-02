using Content.Client.Items;
using Content.Client.Light.Components;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Robust.Client.GameObjects;
using Content.Client.Light.EntitySystems;
using Robust.Client.Animations;
using Robust.Shared.Animations;
using static Content.Shared.Fax.AdminFaxEuiMsg;

namespace Content.Client.Light;

public sealed class RotatePointLightSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    private readonly string _animKey = "what the fuck";

    public override void Initialize()
    {
        SubscribeLocalEvent<RotatePointLightComponent, ComponentStartup>(CompInit);
    }

    /// <summary>
    /// Coderbus' ultimatum
    /// 
    /// Because <see cref="SharedPointLightComponent"/> and <see cref="SharedPointLightComponent.Rotation"/> in particular are
    /// restricted by <see cref="AccessAttribute"/> to be accessible only by <see cref="SharedPointLightSystem"/>,
    /// the only two options for changing pointlight rotation are either
    ///   * hijacking state update system to apply a state with the needed rotation, or
    ///   * this shit.
    /// </summary>
    private void CompInit(EntityUid uid, RotatePointLightComponent comp, ComponentStartup args)
    {
        var anim = new Animation()
        {
            Length = TimeSpan.Zero,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(PointLightComponent),
                    Property = nameof(PointLightComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(comp.Angle, 0),
                    }
                }
            }
        };
        _animation.Play(uid, anim, _animKey);
    }
}
