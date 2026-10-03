using Godot;
using System;

public partial class RocketBullet : Node2D
{
    [Export] public float Speed { get; set; } = 400.0f; // Rockets are generally a bit slower than AR bullets
    [Export] public int Damage { get; set; } = 50; 
    
    public Vector2 Direction { get; set; } = Vector2.Zero;
    private bool _hasExploded = false;

    private Area2D _shotArea;
    private Area2D _explodeArea;
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _shotArea = GetNode<Area2D>("Shot");
        _explodeArea = GetNode<Area2D>("Explode");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        _animPlayer.Play("shot");
        
        _shotArea.AreaEntered += OnShotAreaEntered;
        _shotArea.BodyEntered += OnShotBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_hasExploded)
        {
            GlobalPosition += Direction * Speed * (float)delta;
        }
    }

    private void OnShotAreaEntered(Area2D area)
    {
        if (!_hasExploded && area.GetParent() is Enemy) Explode();
    }

    private void OnShotBodyEntered(Node2D body)
    {
        if (!_hasExploded && body is Enemy) Explode();
    }

    private async void Explode()
    {
        _hasExploded = true;
        _animPlayer.Play("explode");

        // Deal AoE damage
        var overlappingBodies = _explodeArea.GetOverlappingBodies();
        var overlappingAreas = _explodeArea.GetOverlappingAreas();

        foreach (Node2D body in overlappingBodies)
        {
            if (body is Enemy enemy) enemy.TakeDamage(Damage);
        }
        foreach (Area2D area in overlappingAreas)
        {
            if (area.GetParent() is Enemy enemy) enemy.TakeDamage(Damage);
        }

        // Wait for the explosion animation to finish
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
        QueueFree();
    }
}
