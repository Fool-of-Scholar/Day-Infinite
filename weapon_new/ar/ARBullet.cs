using Godot;
using System;
using System.Collections.Generic;

public partial class ARBullet : Area2D
{
    [Export] public float Speed { get; set; } = 900.0f;
    [Export] public int Damage { get; set; } = 15;
    [Export] public float FireRate { get; set; } = 0.5f;   // Balanced: was 0.3 (too spammy)
    [Export] private PackedScene _explosionScene = GD.Load<PackedScene>("res://weapon_new/ar/ar_explosion.tscn");
    [Export] public float MaxRange { get; set; } = 800.0f; // Balanced: was 1000 — more focused range
    private float _distanceTraveled = 0f;

    // --- STAGE PROPERTIES (set by WeaponManager) ---
    public int MaxPierce { get; set; } = 0;
    public bool CanScatter { get; set; } = false;
    [Export] public PackedScene ScatterScene { get; set; }

    public Vector2 Direction { get; set; } = Vector2.Zero;

    private int _pierceCount = 0;
    private HashSet<ulong> _hitEnemyIds = new HashSet<ulong>();

    public bool IsScatterBullet { get; set; } = false;

    public override void _Ready()
    {
        
        // Continuous checking is handled in _PhysicsProcess
    }

    public override void _PhysicsProcess(double delta)
    {
        // Move the bullet forward (single movement calculation — was double-moving before)
        float moveStep = Speed * (float)delta;
        GlobalPosition += Direction * moveStep;
        _distanceTraveled += moveStep;

        // Despawn if it flies past the MaxRange limit
        if (_distanceTraveled > MaxRange)
        {
            QueueFree();
            return;
        }
        // Check for continuous collision every frame
        CheckContinuousImpact();
    }

    private void CheckContinuousImpact()
    {
        var overlappingBodies = GetOverlappingBodies();
        var overlappingAreas = GetOverlappingAreas();

        foreach (Node2D body in overlappingBodies)
        {
            if (body is Enemy enemy) ProcessHit(enemy);
        }

        foreach (Area2D area in overlappingAreas)
        {
            if (area.GetParent() is Enemy enemy) ProcessHit(enemy);
        }
    }

    private void ProcessHit(Enemy enemy)
    {
        ulong id = enemy.GetInstanceId();

        // Damage & Pierce count increment ONCE per enemy
        if (!_hitEnemyIds.Contains(id))
        {
            _hitEnemyIds.Add(id);
            enemy.TakeDamage(Damage);
            SpawnHitEffect();

            _pierceCount++;
        }

        // Scatter triggers EVERY frame while inside an enemy's hitbox
        if (CanScatter && !IsScatterBullet)
        {
            SpawnScatter();
        }

        // Destroy main bullet only once max pierce limit is reached
        if (_pierceCount > MaxPierce)
        {
            QueueFree();
        }
    }

    private void SpawnHitEffect()
    {
        if (_explosionScene != null)
        {
            GpuParticles2D explosion = _explosionScene.Instantiate<GpuParticles2D>();
            explosion.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, explosion);
        }
    }

    private void SpawnScatter()
    {
        PackedScene sceneToUse = ScatterScene;

        if (sceneToUse == null)
        {
            sceneToUse = GD.Load<PackedScene>("res://weapon_new/ar/ar_bullet.tscn");
        }

        if (sceneToUse == null) return;

        float[] scatterAngles = { -0.45f, 0.0f, 0.45f };
        foreach (float angle in scatterAngles)
        {
            ARBullet scatter = sceneToUse.Instantiate<ARBullet>();
            scatter.IsScatterBullet = true; 
            scatter.CanScatter = false;
            scatter.MaxPierce = 0; 
            scatter.Damage = Damage;
            scatter.Speed = Speed;
            scatter.Direction = Direction.Rotated(angle);
            scatter.GlobalPosition = GlobalPosition;

            GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, scatter);
        }
    } 
}