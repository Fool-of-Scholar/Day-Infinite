using Godot;
using System;

public partial class PlasmaBolt : Area2D
{
    [Export] public float Speed { get; set; } = 500.0f;
    [Export] public int Damage { get; set; } = 25;

    public Vector2 Direction { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Direction * Speed * (float)delta;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent() is Enemy enemy)
        {
            Hit(enemy);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            Hit(enemy);
        }
    }

    private void Hit(Enemy enemy)
    {
        enemy.TakeDamage(Damage);
        QueueFree(); // Despawn bolt upon hit
    }
}