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
    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        // Connect to any body or area with player collision
        BodyEntered += OnBodyEntered;
        AreaEntered  += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        _age += (float)delta;
        // Travels in a fixed direction (set on spawn) so it  does NOT track the player after initial launch position.
        // slingshot feel 
        GlobalPosition += Direction * Speed * (float)delta;

        // Fade out in the last 30% of life
        float fadeFraction = Mathf.Clamp((_age - MaxLifetime * 0.7f) / (MaxLifetime * 0.3f), 0f, 1f);
        Modulate = new Color(1f, 1f, 1f, 1f - fadeFraction);

        if (_age >= MaxLifetime)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            ApplyDamage(body);
            QueueFree();
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("player_hitbox") || area.GetParent()?.IsInGroup("player") == true)
        {
            ApplyDamage(area);
            QueueFree();
        }
    }

    private void ApplyDamage(Node target)
    {
        // Calls TakeDamage on the player if it has it
        if (target.HasMethod("TakeDamage"))
            target.Call("TakeDamage", DamageAmount);
        else if (target.GetParent()?.HasMethod("TakeDamage") == true)
            target.GetParent().Call("TakeDamage", DamageAmount);
    }
}
