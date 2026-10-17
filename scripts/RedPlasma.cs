using Godot;
using System;

public partial class RedPlasma : PlasmaBolt
{

    [Export] new private float Speed { get; set; } = 1000.0f;
    [Export] new private int Damage { get; set; } = 25;

    new public Vector2 Direction { get; set; } = Vector2.Zero;

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
