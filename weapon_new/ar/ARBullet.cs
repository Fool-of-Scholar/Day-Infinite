using Godot;
using System;

public partial class ARBullet : Area2D
{
    [Export] public float Speed { get; set; } = 1000.0f; // AR bullets are fast!
    [Export] public int Damage { get; set; } = 15;
    private PackedScene _explosionScene = GD.Load<PackedScene>("res://scene/weapon_new/ar_explosion.tscn");

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
        if (area.GetParent() is Enemy enemy) Hit(enemy);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy) Hit(enemy);
    }

    private void Hit(Enemy enemy)
    {
        enemy.TakeDamage(Damage);

        if (_explosionScene != null)
        {
            Node2D explosion = _explosionScene.Instantiate<Node2D>();
            explosion.GlobalPosition = this.GlobalPosition;
            GetTree().CurrentScene.AddChild(explosion);
        }

        QueueFree(); 
    }
}