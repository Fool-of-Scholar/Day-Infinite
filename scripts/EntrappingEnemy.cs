using Godot;
using System;
using System.Collections.Generic;


public partial class EntrappingEnemy : Enemy
{
    [ExportGroup("Entrapping Behaviour")]
    // An Angle offset in degrees from the ring spawn point. Set by the spawner ;[
    [Export] public float RingAngleDeg { get; set; } = 0f;

    // TO Distance from the player at which this enemy first appears.
    [Export] public float SpawnRingRadius { get; set; } = 520f;

    // Intentional slowess
    [Export] public float InwardSpeed { get; set; } = 42f;

    // Once the ring has closed to roughly melee range we just let base chase logic take over.
    private bool _ringClosed = false;

    public override void _Ready()
    {
        if (MaxHealth == 30) MaxHealth = 280;
        if (MoveSpeed == 70.0f) MoveSpeed = 42f;   

        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (TargetPlayer == null || !IsInstanceValid(TargetPlayer))
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        float distToPlayer = GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition);

        // Once within 120 px, treat as regular follower
        if (distToPlayer < 120f)
        {
            _ringClosed = true;
        }

        if (_ringClosed)
        {
            
            base._PhysicsProcess(delta);
            return;
        }

        // Walk slow squeeze-in
        Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * InwardSpeed;
        UpdateFacing(Velocity.X);
        MoveAndSlide();
    }
}
