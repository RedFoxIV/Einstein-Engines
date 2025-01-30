using System.Numerics;
using Content.Client.Weapons.Ranged.Systems;
using Content.Shared.Contests;
using Content.Shared.Weapons.Ranged.Components;
using MathNet.Numerics.RootFinding;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Timing;


namespace Content.Client.Weapons.Ranged;

[Virtual]
public class GunSpreadOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    protected IEntityManager _entManager;
    protected readonly IEyeManager _eye;
    protected readonly IGameTiming _timing;
    protected readonly IInputManager _input;
    protected readonly IPlayerManager _player;
    protected readonly GunSystem _guns;
    protected readonly SharedTransformSystem _transform;
    protected readonly ContestsSystem _contest;

    public GunSpreadOverlay(IEntityManager entManager, IEyeManager eyeManager, IGameTiming timing, IInputManager input, IPlayerManager player, GunSystem system, SharedTransformSystem transform, ContestsSystem contest)
    {
        _entManager = entManager;
        _eye = eyeManager;
        _input = input;
        _timing = timing;
        _player = player;
        _guns = system;
        _transform = transform;
        _contest = contest;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;

        var player = _player.LocalEntity;

        if (player == null ||
            !_entManager.TryGetComponent<TransformComponent>(player, out var xform))
        {
            Reset();
            return;
        }

        var mapPos = _transform.GetMapCoordinates(player.Value, xform: xform);

        if (mapPos.MapId == MapId.Nullspace)
        {
            Reset();
            return;
        }
        if (!_guns.TryGetGun(player.Value, out var gunUid, out var gun))
        {
            Reset();
            return;
        }

        var contest = 1 / _contest.MassContest(player);

        var mouseScreenPos = _input.MouseScreenPosition;
        var mousePos = _eye.PixelToMap(mouseScreenPos);

        if (mapPos.MapId != mousePos.MapId)
        {
            Reset();
            return;
        }

        // (☞ﾟヮﾟ)☞
        var timeSinceLastFire = (_timing.CurTime - gun.CurrentAngleLastUpdate).TotalSeconds;
        var timeSinceLastBonusUpdate = (_timing.CurTime - gun.BonusAngleLastUpdate).TotalSeconds;
        var maxBonusSpread = gun.MaxBonusAngleModified;
        var bonusSpread = new Angle(MathHelper.Clamp(gun.BonusAngle - gun.BonusAngleDecayModified * timeSinceLastBonusUpdate,
            0, gun.MaxBonusAngleModified));

        var maxSpread = gun.MaxAngleModified;
        var minSpread = gun.MinAngleModified;
        var currentAngle = new Angle(MathHelper.Clamp(gun.CurrentAngle.Theta - gun.AngleDecayModified.Theta * timeSinceLastFire,
            gun.MinAngleModified.Theta, gun.MaxAngleModified.Theta));
        var direction = (mousePos.Position - mapPos.Position);

        Vector2 from = mapPos.Position;
        Vector2 to = mousePos.Position + direction;

        DrawSpread(worldHandle, gun, from, direction, contest, timeSinceLastFire, maxBonusSpread, bonusSpread, maxSpread, minSpread, currentAngle);
    }

    protected void DrawCone(DrawingHandleWorld handle, Vector2 from, Vector2 direction, Angle angle, float contestMul, Color color, float lerp = 1f)
    {
        angle *= contestMul;
        var dir1 = angle.RotateVec(direction);
        var dir2 = (-angle).RotateVec(direction);
        handle.DrawLine(from + dir1 * (1 - lerp), from + dir1 * (1 + lerp), color);
        handle.DrawLine(from + dir2 * (1 - lerp), from + dir2 * (1 + lerp), color);
    }

    protected virtual void DrawSpread(DrawingHandleWorld worldHandle, Shared.Weapons.Ranged.Components.GunComponent gun, Vector2 from, Vector2 direction, float contest, double timeSinceLastFire, Angle maxBonusSpread, Angle bonusSpread, Angle maxSpread, Angle minSpread, Angle currentAngle)
    {
        worldHandle.DrawLine(from, direction*2, Color.Orange);

        // Show max spread either side
        DrawCone(worldHandle, from, direction, maxSpread + bonusSpread, contest, Color.Red);

        // Show min spread either side
        DrawCone(worldHandle, from, direction, minSpread + bonusSpread, contest, Color.Green);

        // Show current angle
        DrawCone(worldHandle, from, direction, currentAngle + bonusSpread, contest, Color.Yellow);

        DrawCone(worldHandle, from, direction, maxBonusSpread, contest, Color.BetterViolet);

        DrawCone(worldHandle, from, direction, bonusSpread, contest, Color.Violet);

        var oldTheta = MathHelper.Clamp(gun.CurrentAngle - gun.AngleDecayModified * timeSinceLastFire, gun.MinAngleModified, gun.MaxAngleModified);
        var newTheta = MathHelper.Clamp(oldTheta + gun.AngleIncreaseModified.Theta, gun.MinAngleModified.Theta, gun.MaxAngleModified.Theta);
        var shit = new Angle(newTheta + bonusSpread);
        DrawCone(worldHandle, from, direction, shit, contest, Color.Gray);
    }

    protected virtual void Reset() { }

}


public sealed class PartialGunSpreadOverlay : GunSpreadOverlay
{

    private GunComponent? _lastGun;

    private double SmoothedCurrentAngle;
    private double SmoothedBonusSpread;

    public PartialGunSpreadOverlay(IEntityManager entManager, IEyeManager eyeManager, IGameTiming timing, IInputManager input, IPlayerManager player, GunSystem system, SharedTransformSystem transform, ContestsSystem contest) : base(entManager, eyeManager, timing, input, player, system, transform, contest) { }

    protected override void Reset() { _lastGun = null; }

    protected override void DrawSpread(DrawingHandleWorld worldHandle, GunComponent gun, Vector2 from, Vector2 direction, float contest, double timeSinceLastFire, Angle maxBonusSpread, Angle bonusSpread, Angle maxSpread, Angle minSpread, Angle currentAngle)
    {
        if(_lastGun != gun)
        {
            _lastGun = gun;
            SmoothedCurrentAngle = currentAngle;
            SmoothedBonusSpread = bonusSpread;
        }
        else
        {
            SmoothedCurrentAngle = Double.Lerp(SmoothedCurrentAngle, currentAngle, 0.7);
            SmoothedBonusSpread = Double.Lerp(SmoothedBonusSpread, bonusSpread, 0.35);
        }
        const float third = 1f / 3f;
        float L = (float) ((currentAngle - minSpread) / (maxSpread - minSpread)); // not smoothed
        float hue = Math.Clamp(third - third * L, 0, third);
        Color color = Color.FromHsv(new Robust.Shared.Maths.Vector4(hue, 1, 1, 1));
        // Show current angle
        DrawCone(worldHandle, from, direction, SmoothedCurrentAngle + SmoothedBonusSpread, contest, color, 0.15f);
        DrawCone(worldHandle, from, direction, SmoothedCurrentAngle, contest, color.WithAlpha(0.1f), 0.15f);
        //color = Color.AliceBlue;
        //DrawCone(worldHandle, from, direction, currentAngle + bonusSpread, contest, color, 0.15f);
        //DrawCone(worldHandle, from, direction, currentAngle, contest, color.WithAlpha(0.1f), 0.15f);
    }


    private class AverageRingBuffer
    {
        private double[] _buffer;

        private int WritePtr = 0;
        public int Size { get; private set; }
        public int Count { get; private set; } = 0;
        public double Sum { get; private set; } = 0;
        public double Average => Sum / Count;

        public AverageRingBuffer(int size)
        {
            Size = size;
            _buffer = new double[size];
        }

        public AverageRingBuffer(int size, double def)
        {
            Size = size;
            _buffer = new double[size];
            Fill(def);
        }

        public void Fill(double value)
        {
            Array.Fill(_buffer, value);
            Count = Size;
            Sum = value * Count;
        } 

        public void Write(double value)
        {
            Sum -= _buffer[WritePtr];
            _buffer[WritePtr] = value;
            Sum += _buffer[WritePtr];
            WritePtr = (WritePtr + 1) % Size;
            if (Count < Size)
                Count++;
        }
    }
}
