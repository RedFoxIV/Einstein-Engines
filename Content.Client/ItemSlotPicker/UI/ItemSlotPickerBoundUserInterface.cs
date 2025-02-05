using Content.Client.Chat.UI;
using Content.Client.UserInterface.Controls;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.ItemSlotPicker;
using Content.Shared.ItemSlotPicker.UI;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using static Robust.Client.UserInterface.Control;
using System.Numerics;

namespace Content.Client.ItemSlotPicker.UI;

// i don't know what the others have been thinking, this should go into shared
// so the bui can be tracked on server to avoid any desync / sudden menu closing
// bullshit.
// Also because it makes fucking sense.
[UsedImplicitly]
public sealed class ItemSlotPickerBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEyeManager _eye = default!;

    private readonly ItemSlotsSystem _itemSlots;
    private readonly SharedTransformSystem _transform;

    private RadialMenu? _menu;
    private RadialContainer? _layer;

    public ItemSlotPickerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _itemSlots = EntMan.System<ItemSlotsSystem>();
        _transform = EntMan.System<SharedTransformSystem>();
    }

    protected override void Open()
    {
        _menu = new EntityCenteredRadialMenu(Owner, EntMan, _eye, _clyde);
        _menu.OnClose += Close;
        _menu.CloseButtonStyleClass = "RadialMenuCloseButton";
        _menu.BackButtonStyleClass = "RadialMenuBackButton";
        
        
        UpdateLayer();
        _menu.OpenCenteredAt(_eye.WorldToScreen(_transform.GetWorldPosition(Owner)) / _clyde.ScreenSize);
    }

    private void UpdateLayer()
    {
        var picker = EntMan.GetComponent<ItemSlotPickerComponent>(Owner);
        _layer?.Dispose();
        _layer = new RadialContainer();
        foreach (string slotID in picker.ItemSlots)
        {
            if (!_itemSlots.TryGetSlot(Owner, slotID, out var slot) ||
                !slot.HasItem)
                continue;

            // i see no value in having 99 different radial button types with the only difference being what data they hold
            // hence i'm just setting all relevant parameters after constructing the button.
            var button = new RadialMenuTextureButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64f, 64f),
                ToolTip = Loc.GetString(slot.Name),
            };
            button.AddStyleClass("RadialMenuButton");

            var tex = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = EntMan.GetComponent<SpriteComponent>(slot.Item!.Value).Icon?.Default,
                TextureScale = new Vector2(2f, 2f),
            };

            button.AddChild(tex);
            button.OnButtonUp += _ => { SendMessage(new ItemSlotPickerSlotPickedMessage(slotID)); };
            _layer.AddChild(button);
        }
        if (_layer.ChildCount == 0)
            Close();
        _menu!.AddChild(_layer);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is not ItemSlotPickerContentsChangedMessage)
            return;
        UpdateLayer();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        _menu?.Dispose();
    }
}

[Virtual]
public class EntityCenteredRadialMenu : RadialMenu
{
    public EntityUid Entity;
    private readonly IClyde _clyde;
    private readonly IEyeManager _eye;
    private readonly IEntityManager _entMan;
    private readonly SharedTransformSystem _transform;

    private System.Numerics.Vector2 _cachedPos;

    public EntityCenteredRadialMenu(EntityUid entity, IEntityManager man, IEyeManager eye, IClyde clyde) : base()
    {
        Entity = entity;
        _eye = eye;
        _clyde = clyde;
        _entMan = man;
        _transform = _entMan.System<SharedTransformSystem>();
        
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        if (_entMan.Deleted(Entity) ||
            !_entMan.TryGetComponent<TransformComponent>(Entity, out var transform))
            return;
        var pos = _eye.WorldToScreen(_transform.GetWorldPosition(Entity)) / _clyde.ScreenSize;
        if (pos == _cachedPos)
            return;
        _cachedPos = pos;
        RecenterWindow(pos);
    }
}
