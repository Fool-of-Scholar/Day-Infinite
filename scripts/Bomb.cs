using Godot;
using System;

public partial class Bomb : Area2D
{
    [Export] public float Speed { get; set; } = 300.0f; // Slower bullet
    [Export] public int ExplosionDamage { get; set; } = 100; // Massive damage

    public Vector2 Direction { get; set; } = Vector2.Zero;
    private Area2D _blastZone;

    public override void _Ready()
    {
        _blastZone = GetNode<Area2D>("BlastZone");
        
        // Trigger the explosion when we hit an enemy
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Direction * Speed * (float)delta;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent() is Enemy) Explode();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy) Explode();
    }

    private void Explode()
    {
        // 1. Grab EVERY body and area currently sitting inside the massive BlastZone circle
        var overlappingBodies = _blastZone.GetOverlappingBodies();
        var overlappingAreas = _blastZone.GetOverlappingAreas();

        // 2. Loop through all of them and deal damage
        foreach (Node2D body in overlappingBodies)
        {
            if (body is Enemy enemy) enemy.TakeDamage(ExplosionDamage);
        }
        foreach (Area2D area in overlappingAreas)
        {
            if (area.GetParent() is Enemy enemy) enemy.TakeDamage(ExplosionDamage);
        }

        // 3. Delete the bullet
        QueueFree(); 
    }
}

