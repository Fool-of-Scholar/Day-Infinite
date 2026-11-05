using Godot;
using System;

/// <summary>
/// Dynamic Virtual Joystick for Mobile (Android) & Desktop touch emulation.
/// Drives Godot's standard input actions ("left", "right", "upward", "downward")
/// with analog strength values and exposes static OutputDirection.
/// </summary>
public partial class VirtualJoystick : Control
{
    public enum JoystickMode
    {
        Fixed,
        Dynamic,
        Following
    }

    public static Vector2 OutputDirection { get; private set; } = Vector2.Zero;

    [ExportGroup("Joystick Properties")]
    [Export] public JoystickMode Mode { get; set; } = JoystickMode.Following;
    [Export] public float MaxRadius { get; set; } = 70f;
    [Export] public float Deadzone { get; set; } = 8f;
    [Export] public Vector2 RestingOffset { get; set; } = new Vector2(135f, -135f);
    [Export] public float IdleAlpha { get; set; } = 0.45f;
    [Export] public float ActiveAlpha { get; set; } = 0.95f;

    [ExportGroup("Input Actions")]
    [Export] public string ActionLeft { get; set; } = "left";
    [Export] public string ActionRight { get; set; } = "right";
    [Export] public string ActionUp { get; set; } = "upward";
    [Export] public string ActionDown { get; set; } = "downward";
    [Export] public bool ForceVisibleForTesting { get; set; } = false;

    private Control _base;
    private Control _knob;
    private Vector2 _defaultCenterPos = Vector2.Zero;
    private Vector2 _centerPos = Vector2.Zero;
    private int _touchIndex = -1;
    private bool _isDragging = false;
    private bool _isPlatformAllowed = true;

    public override void _Ready()
    {
        // Only visible and active on Android (mobile), not on Windows desktop
        bool isAndroid = OS.GetName() == "Android" || OS.HasFeature("android");
        if (!isAndroid && !ForceVisibleForTesting)
        {
            _isPlatformAllowed = false;
            Visible = false;
            SetProcess(false);
            SetProcessInput(false);
            SetProcessUnhandledInput(false);
            return;
        }

        _base = GetNodeOrNull<Control>("Base");
        _knob = GetNodeOrNull<Control>("Knob");

        Modulate = new Color(1f, 1f, 1f, IdleAlpha);

        CallDeferred(nameof(RecalculateDefaultPosition));
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what == NotificationResized)
        {
            RecalculateDefaultPosition();
        }
    }

    private void RecalculateDefaultPosition()
    {
        float posX = RestingOffset.X >= 0 ? RestingOffset.X : Size.X + RestingOffset.X;
        float posY = RestingOffset.Y >= 0 ? RestingOffset.Y : Size.Y + RestingOffset.Y;
        _defaultCenterPos = new Vector2(posX, posY);

        if (!_isDragging)
        {
            ResetToRestingPosition();
        }
    }

    private void ResetToRestingPosition()
    {
        _centerPos = _defaultCenterPos;
        if (_base != null)
        {
            _base.Position = _centerPos - (_base.Size * 0.5f);
        }
        if (_knob != null)
        {
            _knob.Position = _centerPos - (_knob.Size * 0.5f);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!_isPlatformAllowed) return;

        // 1. Mobile Screen Touch
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed && _touchIndex == -1)
            {
                StartTouch(touch.Index, touch.Position);
                AcceptEvent();
            }
            else if (!touch.Pressed && touch.Index == _touchIndex)
            {
                ReleaseJoystick();
                AcceptEvent();
            }
        }
        // 2. Mobile Screen Drag
        else if (@event is InputEventScreenDrag drag && drag.Index == _touchIndex)
        {
            ProcessInput(drag.Position);
            AcceptEvent();
        }
        // 3. Mouse Fallback (Desktop testing & editor preview)
        else if (@event is InputEventMouseButton mouseBtn && mouseBtn.ButtonIndex == MouseButton.Left)
        {
            if (mouseBtn.Pressed && _touchIndex == -1)
            {
                StartTouch(999, mouseBtn.Position);
                AcceptEvent();
            }
            else if (!mouseBtn.Pressed && _touchIndex == 999)
            {
                ReleaseJoystick();
                AcceptEvent();
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _touchIndex == 999)
        {
            ProcessInput(mouseMotion.Position);
            AcceptEvent();
        }
    }

    private void StartTouch(int index, Vector2 localPos)
    {
        _touchIndex = index;
        _isDragging = true;
        Modulate = new Color(1f, 1f, 1f, ActiveAlpha);

        if (Mode == JoystickMode.Fixed)
        {
            ProcessInput(localPos);
        }
        else
        {
            // Dynamic & Following modes: position base directly under finger
            _centerPos = localPos;
            if (_base != null)
            {
                _base.Position = _centerPos - (_base.Size * 0.5f);
            }
            if (_knob != null)
            {
                _knob.Position = _centerPos - (_knob.Size * 0.5f);
            }
            OutputDirection = Vector2.Zero;
        }
    }

    private void ProcessInput(Vector2 localPos)
    {
        Vector2 diff = localPos - _centerPos;
        float dist = diff.Length();

        if (dist > MaxRadius)
        {
            if (Mode == JoystickMode.Following)
            {
                // The base smoothly follows the touch position to eliminate travel deadzone
                _centerPos = localPos - (diff.Normalized() * MaxRadius);
                if (_base != null)
                {
                    _base.Position = _centerPos - (_base.Size * 0.5f);
                }
                if (_knob != null)
                {
                    _knob.Position = localPos - (_knob.Size * 0.5f);
                }
            }
            else
            {
                // Clamped to MaxRadius around static/initial center
                Vector2 clampedDiff = diff.Normalized() * MaxRadius;
                if (_knob != null)
                {
                    _knob.Position = _centerPos + clampedDiff - (_knob.Size * 0.5f);
                }
            }
        }
        else
        {
            if (_knob != null)
            {
                _knob.Position = localPos - (_knob.Size * 0.5f);
            }
        }

        // Calculate analog output vector
        diff = localPos - _centerPos;
        dist = diff.Length();

        if (dist > Deadzone)
        {
            float normalizedDistance = Mathf.Clamp((dist - Deadzone) / (MaxRadius - Deadzone), 0f, 1f);
            OutputDirection = diff.Normalized() * normalizedDistance;
        }
        else
        {
            OutputDirection = Vector2.Zero;
        }

        // Feed directly into Godot input actions for Movement.cs
        ApplyActionStrength(ActionRight, Mathf.Clamp(OutputDirection.X, 0f, 1f));
        ApplyActionStrength(ActionLeft, Mathf.Clamp(-OutputDirection.X, 0f, 1f));
        ApplyActionStrength(ActionDown, Mathf.Clamp(OutputDirection.Y, 0f, 1f));
        ApplyActionStrength(ActionUp, Mathf.Clamp(-OutputDirection.Y, 0f, 1f));
    }

    private void ApplyActionStrength(string action, float strength)
    {
        if (string.IsNullOrEmpty(action)) return;

        if (strength > 0.05f)
        {
            Input.ActionPress(action, strength);
        }
        else
        {
            if (Input.IsActionPressed(action))
            {
                Input.ActionRelease(action);
            }
        }
    }

    public void ReleaseJoystick()
    {
        _touchIndex = -1;
        _isDragging = false;
        OutputDirection = Vector2.Zero;
        Modulate = new Color(1f, 1f, 1f, IdleAlpha);

        ResetToRestingPosition();

        // Release all simulated movement actions
        ReleaseAction(ActionRight);
        ReleaseAction(ActionLeft);
        ReleaseAction(ActionDown);
        ReleaseAction(ActionUp);
    }

    private void ReleaseAction(string action)
    {
        if (!string.IsNullOrEmpty(action) && Input.IsActionPressed(action))
        {
            Input.ActionRelease(action);
        }
    }

    public override void _ExitTree()
    {
        ReleaseJoystick();
        base._ExitTree();
    }
}
