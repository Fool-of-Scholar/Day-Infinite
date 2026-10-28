using Godot;
using System;
using System.Collections.Generic;

public partial class RocketBullet : Node2D
{
    [Export] public float Speed { get; set; } = 380.0f;
    [Export] public int Damage { get; set; } = 90;   // Balanced: was 50 — high burst justifies slow fire rate
    [Export] public float FireRate { get; set; } = 2.0f;   // Balanced: was 1.0 — slow & impactful role
    [Export] public float MaxRange { get; set; } = 900.0f; // Balanced: was 1000
    private float _distanceTraveled = 0f;

    // --- STAGE PROPERTIES (set by WeaponManager) ---
    // Stage 2: bigger AOE radius multiplier (e.g. 1.0 = default, 1.8 = bigger)
    public float AoeRadiusMultiplier { get; set; } = 1.0f;
    // Stage 3: suck enemies in before detonating (true = pull then explode)
    public bool SuckBeforeExplode { get; set; } = false;
    public float SuckDuration { get; set; } = 1.5f;
    public float SuckForce { get; set; } = 200.0f;

    public Vector2 Direction { get; set; } = Vector2.Zero;
    private bool _hasExploded = false;
    private bool _isSucking = false;

    private Area2D _shotArea;
    private Area2D _explodeArea;
    private AnimationPlayer _animPlayer;
    private CollisionShape2D _explodeShape;
    private AudioStreamPlayer _shotAudio;
    private AudioStreamPlayer _suckAudio;
    private AudioStreamPlayer _explodeAudio;

    public override void _Ready()
    {
        _shotArea = GetNode<Area2D>("Shot");
        _explodeArea = GetNode<Area2D>("Explode");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _explodeShape = _explodeArea.GetNode<CollisionShape2D>("CollisionShape2D");
        _shotAudio = GetNodeOrNull<AudioStreamPlayer>("ShotAudio");
        _suckAudio = GetNodeOrNull<AudioStreamPlayer>("SuckAudio");
        _explodeAudio = GetNodeOrNull<AudioStreamPlayer>("ExplodeAudio");

        _shotAudio?.Play();


        // Apply AOE size scaling from Stage 2 with a unique duplicated shape
        if (AoeRadiusMultiplier != 1.0f && _explodeShape?.Shape is CircleShape2D circle)
        {
            var uniqueCircle = (CircleShape2D)circle.Duplicate();
            uniqueCircle.Radius *= AoeRadiusMultiplier;
            _explodeShape.Shape = uniqueCircle;
        }

        _animPlayer.Play("shot");

        _shotArea.AreaEntered += OnShotAreaEntered;
        _shotArea.BodyEntered += OnShotBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_hasExploded)
        {
            // Check for immediate point-blank overlap (e.g. enemy touching player when fired)
            CheckDirectImpact();

            if (_hasExploded) return;

            // Calculate how far we move this frame
            float moveStep = Speed * (float)delta; 
            
            // Move the bullet
            GlobalPosition += Direction * moveStep;
            
            // Add to our total distance tracker
            _distanceTraveled += moveStep;

            // Despawn if it flies past the MaxRange
            if (_distanceTraveled > MaxRange)
            {
                QueueFree();
                return; 
            }
        }

        // Stage 3: continuously pull enemies currently inside the explosion radius
        if (_isSucking)
        {
            // Dynamically grab all enemies inside the radius EVERY frame
            HashSet<ulong> processed = new HashSet<ulong>();

            foreach (Node2D body in _explodeArea.GetOverlappingBodies())
            {
                if (body is Enemy enemy && processed.Add(enemy.GetInstanceId()))
                {
                    Vector2 pullDir = (GlobalPosition - enemy.GlobalPosition).Normalized();
                    // Directly modify position so it overrides their normal walking AI
                    enemy.GlobalPosition += pullDir * SuckForce * (float)delta;
                }
            }

            foreach (Area2D area in _explodeArea.GetOverlappingAreas())
            {
                if (area.GetParent() is Enemy enemy && processed.Add(enemy.GetInstanceId()))
                {
                    Vector2 pullDir = (GlobalPosition - enemy.GlobalPosition).Normalized();
                    // Directly modify position so it overrides their normal walking AI
                    enemy.GlobalPosition += pullDir * SuckForce * (float)delta;
                }
            }
        }
    }

    private void CheckDirectImpact()
    {
        if (_hasExploded) return;

        foreach (Node2D body in _shotArea.GetOverlappingBodies())
        {
            if (body is Enemy)
            {
                TriggerExplosion();
                return;
            }
        }

        foreach (Area2D area in _shotArea.GetOverlappingAreas())
        {
            if (area.GetParent() is Enemy)
            {
                TriggerExplosion();
                return;
            }
        }
    }

    private void OnShotAreaEntered(Area2D area)
    {
        if (!_hasExploded && area.GetParent() is Enemy) TriggerExplosion();
    }

    private void OnShotBodyEntered(Node2D body)
    {
        if (!_hasExploded && body is Enemy) TriggerExplosion();
    }

    private void TriggerExplosion()
    {
        _hasExploded = true; // This stops forward movement in _PhysicsProcess

        if (SuckBeforeExplode)
        {
            // Do NOT play the explode animation yet. 
            // Letting it stay on the "shot" animation keeps it spinning!
            StartSuck();
        }
        else
        {
            // Normal behavior for Stages 1 & 2
            _explodeAudio?.Play();
            _animPlayer.Play("explode");
            DealDamage();
            WaitAndFree();
        }
    }

    private void StartSuck()
    {
        _isSucking = true;
        _suckAudio?.Play();

        GetTree().CreateTimer(SuckDuration).Timeout += () =>
        {
            if (IsInstanceValid(this))
            {
                _isSucking = false;
                _suckAudio?.Stop();

                _explodeAudio?.Play();
                _animPlayer.Play("explode");

                DealDamage();
                WaitAndFree();
            }
        };
    }

    private void DealDamage()
    {
        HashSet<ulong> damaged = new HashSet<ulong>();

        // 1. Instant distance calculation against active enemies.
        // Guarantees point-blank / frame-0 hits register even if Area2D broadphase hasn't updated yet!
        float radius = (_explodeShape?.Shape is CircleShape2D circle) ? circle.Radius : 220f;
        float effectiveRadius = radius * GlobalScale.X;

        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (Node node in enemies)
        {
            if (node is Enemy enemy && IsInstanceValid(enemy))
            {
                if (GlobalPosition.DistanceTo(enemy.GlobalPosition) <= effectiveRadius)
                {
                    if (damaged.Add(enemy.GetInstanceId()))
                    {
                        enemy.TakeDamage(Damage);
                    }
                }
            }
        }

        // 2. Also check Area2D overlaps for any non-standard or hurtbox children
        var overlappingBodies = _explodeArea.GetOverlappingBodies();
        var overlappingAreas = _explodeArea.GetOverlappingAreas();

        foreach (Node2D body in overlappingBodies)
        {
            if (body is Enemy enemy && damaged.Add(enemy.GetInstanceId()))
                enemy.TakeDamage(Damage);
        }
        foreach (Area2D area in overlappingAreas)
        {
            if (area.GetParent() is Enemy enemy && damaged.Add(enemy.GetInstanceId()))
                enemy.TakeDamage(Damage);
        }
    }

    private void WaitAndFree()
    {
        void DoFree()
        {
            if (_explodeAudio != null && _explodeAudio.Playing && _explodeAudio.GetParent() == this)
            {
                _explodeAudio.Reparent(GetTree().CurrentScene);
                _explodeAudio.Finished += () => _explodeAudio.QueueFree();
            }
            if (IsInstanceValid(this))
            {
                QueueFree();
            }
        }

        _animPlayer.AnimationFinished += (anim) => DoFree();

        // Safety fallback timer
        GetTree().CreateTimer(0.35f).Timeout += () => DoFree();
    }
}
