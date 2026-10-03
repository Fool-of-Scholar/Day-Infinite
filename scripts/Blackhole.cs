using Godot;
using System;

public partial class Blackhole : Area2D
{
    [Export] public float Speed { get; set; } = 150.0f; // Moves slower so it has time to suck enemies
    [Export] public float PullForce { get; set; } = 250.0f; // Gravity strength
    [Export] public float Lifetime { get; set; } = 3.0f; // 3 second timer

    [Export] public PackedScene ExplosionScene {get; set;}

    public Vector2 Direction { get; set; } = Vector2.Zero;
    private Area2D _blastZone;

    public override void _Ready()
    {
        _blastZone = GetNode<Area2D>("BlastZone");
    }

    public override void _PhysicsProcess(double delta)
{
    // 1. Tick down the timer every frame
    Lifetime -= (float)delta;
    if (Lifetime <= 0)
    {
        Explode(); // <-- Instead of just disappearing, it blows up!
        return;
    }

    // 2. Move the blackhole forward
    GlobalPosition += Direction * Speed * (float)delta;

    // 3. Apply gravity to all enemies inside the massive BlastZone
    var overlappingBodies = _blastZone.GetOverlappingBodies();
    foreach (Node2D body in overlappingBodies)
    {
        if (body is Enemy enemy)
        {
            Vector2 pullDirection = (GlobalPosition - enemy.GlobalPosition).Normalized();
            enemy.GlobalPosition += pullDirection * PullForce * (float)delta;
        }
    }
}

private void Explode()
    {
        // 1. Deal damage to everything trapped in the radius
        var overlappingBodies = _blastZone.GetOverlappingBodies();
        foreach (Node2D body in overlappingBodies)
        {
            if (body is Enemy enemy)
            {
                enemy.TakeDamage(150);
            }
        }

        // 2. Spawn the particle explosion scene right where the blackhole died!
        if (ExplosionScene != null)
        {
            Node2D explosion = ExplosionScene.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(explosion);
            explosion.GlobalPosition = GlobalPosition;
        }

        // 3. Delete the blackhole
        QueueFree();
    }
}