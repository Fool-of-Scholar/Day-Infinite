using Godot;
using System;
using System.Collections.Generic;

public partial class Mine : Area2D
{
    [Export] public float FireRate { get; set; } = 3.5f;   // Balanced: was 2.0 — a deliberate tactical placement
    [Export] public int Damage { get; set; } = 120;          // Balanced: was 100 — highest burst to reward patience
    [Export] public PackedScene ExplosionScene { get; set; } 

    // --- STAGE PROPERTIES (set by WeaponManager) ---
    // Stage 2: on explode, spawn this many child mines scattered around (0 = no duplication)
    public int DuplicateCount { get; set; } = 0;
    public float DuplicateScatterRadius { get; set; } = 80f;
    // Stage 3: suck enemies in before detonating
    public bool SuckBeforeExplode { get; set; } = false;
    public float SuckDuration { get; set; } = 1.5f;   // Balanced: was 1.0 — more dramatic
    public float SuckForce { get; set; } = 180.0f;

    // Flag so duplicated mines don't duplicate themselves again
    public bool IsDuplicate { get; set; } = false;

    private Area2D _areaexplosion;
    private bool _hasExploded = false;
    private bool _isSucking = false;
    private AnimationPlayer _animPlayer;
    private AudioStreamPlayer _warningAudio;
    private AudioStreamPlayer _explodeAudio;

    public override async void _Ready()
    {
        _areaexplosion = GetNode<Area2D>("AreaExplosion");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _warningAudio = GetNodeOrNull<AudioStreamPlayer>("WarningAudio");
        _explodeAudio = GetNodeOrNull<AudioStreamPlayer>("ExplodeAudio");
        
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;

        // NEW: 10-Second Despawn Timer
        await ToSignal(GetTree().CreateTimer(12.0f), SceneTreeTimer.SignalName.Timeout);  // Balanced: was 10s
        
        // Only delete it if it's still alive and hasn't started exploding/sucking
        if (!_hasExploded && IsInstanceValid(this))
        {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isSucking)
        {
            // Dynamically grab all enemies inside the radius EVERY frame
            HashSet<ulong> processed = new HashSet<ulong>();

            foreach (Node2D body in _areaexplosion.GetOverlappingBodies())
            {
                if (body is Enemy enemy && processed.Add(enemy.GetInstanceId()))
                {
                    Vector2 pullDir = (GlobalPosition - enemy.GlobalPosition).Normalized();
                    // Directly modify position to overpower walking AI
                    enemy.GlobalPosition += pullDir * SuckForce * (float)delta;
                }
            }

            foreach (Area2D area in _areaexplosion.GetOverlappingAreas())
            {
                if (area.GetParent() is Enemy enemy && processed.Add(enemy.GetInstanceId()))
                {
                    Vector2 pullDir = (GlobalPosition - enemy.GlobalPosition).Normalized();
                    enemy.GlobalPosition += pullDir * SuckForce * (float)delta;
                }
            }
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!_hasExploded && body is Enemy enemy) TriggerExplosion(enemy);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (!_hasExploded && area.GetParent() is Enemy enemy)
            TriggerExplosion(enemy);
    }

    private void TriggerExplosion(Enemy trigger)
    {
        if (_hasExploded) return;
        _hasExploded = true;

        if (SuckBeforeExplode)
        {
            StartSuck(trigger);
        }
        else
        {
            Explode(trigger);
        }
    }

    private async void StartSuck(Enemy trigger)
    {
        _isSucking = true;
        _warningAudio?.Play();

        // Play the warning animation!
        _animPlayer.Play("warning");
        
        // Wait for the suck duration while _PhysicsProcess aggressively pulls them in
        await ToSignal(GetTree().CreateTimer(SuckDuration), SceneTreeTimer.SignalName.Timeout);

        _isSucking = false;
        _warningAudio?.Stop();
        Explode(trigger);
    }

    private void Explode(Enemy triggerEnemy)
    {
        _explodeAudio?.Play();
        HashSet<ulong> damaged = new HashSet<ulong>();

        // Always damage the trigger enemy even if they stepped off
        if (IsInstanceValid(triggerEnemy))
        {
            damaged.Add(triggerEnemy.GetInstanceId());
            triggerEnemy.TakeDamage(Damage);
        }

        // Damage everyone in the AOE
        foreach (Node2D body in _areaexplosion.GetOverlappingBodies())
        {
            if (body is Enemy enemy && damaged.Add(enemy.GetInstanceId()))
                enemy.TakeDamage(Damage);
        }
        foreach (Area2D area in _areaexplosion.GetOverlappingAreas())
        {
            if (area.GetParent() is Enemy enemy && damaged.Add(enemy.GetInstanceId()))
                enemy.TakeDamage(Damage);
        }

        // Spawn visual effect
        if (ExplosionScene != null)
        {
            GpuParticles2D explosion = ExplosionScene.Instantiate<GpuParticles2D>();
            explosion.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.AddChild(explosion);
        }

        // Stage 2: spawn duplicate mines in a cross/scatter pattern
        if (!IsDuplicate && DuplicateCount > 0)
        {
            SpawnDuplicates();
        }

        if (_explodeAudio != null && _explodeAudio.Playing)
        {
            _explodeAudio.Reparent(GetTree().CurrentScene);
            _explodeAudio.Finished += () => _explodeAudio.QueueFree();
        }

        QueueFree();
    }

    private void SpawnDuplicates()
    {
        // Place 4 mines in a cross pattern around the explosion point
        Vector2[] offsets = {
            new Vector2(DuplicateScatterRadius, 0),
            new Vector2(-DuplicateScatterRadius, 0),
            new Vector2(0, DuplicateScatterRadius),
            new Vector2(0, -DuplicateScatterRadius)
        };

        // Clamp to the requested duplicate count (default 4)
        int count = Mathf.Min(DuplicateCount, offsets.Length);

        string minePath = SceneFilePath;
        if (string.IsNullOrEmpty(minePath)) return;
        PackedScene mineScene = GD.Load<PackedScene>(minePath);
        if (mineScene == null) return;

        for (int i = 0; i < count; i++)
        {
            Mine child = mineScene.Instantiate<Mine>();
            child.GlobalPosition = GlobalPosition + offsets[i];
            child.Damage = Damage;
            child.IsDuplicate = true;         // Prevent chain duplication
            child.DuplicateCount = 0;
            child.SuckBeforeExplode = SuckBeforeExplode; // Duplicates inherit suck stage if also stage 3
            child.SuckDuration = SuckDuration;
            GetTree().CurrentScene.AddChild(child);
        }
    }
}