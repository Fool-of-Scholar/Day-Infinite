using Godot;
using System;
using System.Collections.Generic;

public partial class ARBullet : Area2D
{
    [Export] public float Speed { get; set; } = 1000.0f;
    [Export] public int Damage { get; set; } = 15;
    [Export] public float FireRate { get; set; } = .3f;
    [Export] private PackedScene _explosionScene = GD.Load<PackedScene>("res://weapon_new/ar/ar_explosion.tscn");
    [Export] public float MaxRange { get; set; } = 1000.0f; // Adjust this in Inspector!
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
        GlobalPosition += Direction * Speed * (float)delta;
        // 1. Calculate how far we move this exact frame
        float moveStep = Speed * (float)delta; 
        
        // 2. Move the bullet forward
        GlobalPosition += Direction * moveStep;
        
        // 3. Add that step to our total distance tracker
        _distanceTraveled += moveStep;

        // 4. Despawn if it flies past the MaxRange limit
        if (_distanceTraveled > MaxRange)
        {
            QueueFree();
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