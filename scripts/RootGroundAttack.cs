using Godot;

/// <summary>
/// Root Ground Attack — Hazard summoned by Boss 2 directly onto the player's ground.
/// 
/// 1. Telegraph Phase (0.75s):
///    Draws a visible red/thorn danger zone with an expanding countdown circle
///    giving the player a fair reaction window to dash or move away.
/// 
/// 2. Eruption Phase (0.60s):
///    The warning indicator vanishes, roots burst from the ground playing the
///    6-frame eruption animation, and the hitbox activates to damage the player.
/// 
/// 3. Dissipation & Cleanup:
///    Fades away cleanly and frees itself.
/// </summary>
public partial class RootGroundAttack : Area2D
{
    [ExportGroup("Timing & Balance")]
    [Export] public float TelegraphDuration { get; set; } = 0.75f; // Warning time to dodge
    [Export] public float ActiveDuration    { get; set; } = 0.60f; // Eruption damage window
    [Export] public float Radius            { get; set; } = 28f;   // Radius of ground danger circle
    [Export] public int   DamageAmount      { get; set; } = 25;

    private AnimatedSprite2D _animSprite;
    private CollisionShape2D _collisionShape;
    private CircleShape2D    _circleShape;

    private float _timer = 0f;
    private bool  _isErupted = false;
    private bool  _hasDamagedPlayer = false;

    // Telegraph visual parameters
    private Color _telegraphBgColor   = new Color(0.85f, 0.2f, 0.15f, 0.22f); // Semi-transparent red danger zone
    private Color _telegraphRingColor = new Color(1.0f, 0.35f, 0.2f, 0.85f); // Bright warning perimeter ring

    public override void _Ready()
    {
        _animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (_collisionShape?.Shape is CircleShape2D circle)
        {
            _circleShape = circle;
            _circleShape.Radius = Radius;
        }

        // Roots stay hidden underground while telegraphing
        if (_animSprite != null)
        {
            _animSprite.Visible = false;
            _animSprite.Stop();
        }

        // Hitbox disabled during telegraph
        Monitoring = false;
        Monitorable = false;

        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;

        if (!_isErupted)
        {
            // Continuously redraw the telegraph indicator
            QueueRedraw();

            if (_timer >= TelegraphDuration)
            {
                Erupt();
            }
        }
        else
        {
            if (_timer >= TelegraphDuration + ActiveDuration)
            {
                // Smoothly fade out the erupting roots
                Tween tween = CreateTween();
                tween.TweenProperty(this, "modulate:a", 0f, 0.25f);
                tween.TweenCallback(Callable.From(QueueFree));
                SetProcess(false);
            }
        }
    }

    public override void _Draw()
    {
        // Only draw the ground telegraph before eruption
        if (!_isErupted)
        {
            float progress = Mathf.Clamp(_timer / TelegraphDuration, 0f, 1f);

            // 1. Red danger circle
            DrawCircle(Vector2.Zero, Radius, _telegraphBgColor);

            // 2. Inner expanding countdown fill
            DrawCircle(Vector2.Zero, Radius * progress, new Color(0.9f, 0.25f, 0.15f, 0.35f));

            // 3. Outer warning boundary ring with subtle pulse
            float pulse = 1f + 0.06f * Mathf.Sin(_timer * 16f);
            DrawArc(Vector2.Zero, Radius * pulse, 0, Mathf.Tau, 32, _telegraphRingColor, 2f);
        }
    }

    private void Erupt()
    {
        _isErupted = true;
        QueueRedraw(); // Clear the telegraph circle

        if (_animSprite != null)
        {
            _animSprite.Visible = true;
            _animSprite.Frame = 0;
            _animSprite.Play("default");
        }

        // Activate hitbox
        Monitoring = true;
        Monitorable = true;

        // Check if player is standing in the circle upon eruption
        CheckOverlap();
    }

    private void CheckOverlap()
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (TryDamage(body)) return;
        }

        if (!_hasDamagedPlayer)
        {
            foreach (Area2D area in GetOverlappingAreas())
            {
                if (TryDamage(area)) return;
            }
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        TryDamage(body);
    }

    private void OnAreaEntered(Area2D area)
    {
        TryDamage(area);
    }

    private bool TryDamage(Node node)
    {
        if (_hasDamagedPlayer || !_isErupted) return false;

        Movement player = null;
        if (node is Movement m)
        {
            player = m;
        }
        else if (node.IsInGroup("player") && node is Movement p)
        {
            player = p;
        }
        else if (node.GetParent() is Movement parentPlayer)
        {
            player = parentPlayer;
        }

        if (player != null)
        {
            _hasDamagedPlayer = true;
            player.TakeDamage(DamageAmount);
            return true;
        }

        return false;
    }
}
