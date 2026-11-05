using Godot;


/// Spirit Kappa — projectile fired in a cross pattern.

/// Fades out and self-destructs after MaxLifetime seconds.

public partial class SpiritKappaProjectile : Area2D
{
    [Export] public float Speed { get; set; } = 340f;
    [Export] public float MaxLifetime { get; set; } = 1.4f;
    [Export] public int DamageAmount { get; set; } = 18;

    public Vector2 Direction { get; set; } = Vector2.Right;

    private float _age = 0f;
    private bool _isDestroyed = false;

    public override void _Ready()
    {
        // Connect to any body or area with player collision
        BodyEntered += OnBodyEntered;
        AreaEntered  += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        if (_isDestroyed) return;

        _age += (float)delta;
        // Travels in a fixed direction (set on spawn) so it does NOT track the player after initial launch position.
        GlobalPosition += Direction * Speed * (float)delta;

        // Fade out in the last 30% of life
        float fadeFraction = Mathf.Clamp((_age - MaxLifetime * 0.7f) / (MaxLifetime * 0.3f), 0f, 1f);
        Modulate = new Color(1f, 1f, 1f, 1f - fadeFraction);

        if (_age >= MaxLifetime)
        {
            _isDestroyed = true;
            QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (_isDestroyed) return;
        if (body.IsInGroup("player"))
        {
            ApplyDamage(body);
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (_isDestroyed) return;
        if (area.IsInGroup("player_hitbox") || area.GetParent()?.IsInGroup("player") == true)
        {
            ApplyDamage(area);
        }
    }

    private void ApplyDamage(Node target)
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        Movement player = target as Movement 
                       ?? target.GetParent() as Movement
                       ?? (IsInsideTree() ? GetTree().GetFirstNodeInGroup("player") as Movement : null);

        player?.TakeDamage(DamageAmount);
        QueueFree();
    }
}
