using Godot;
using System;
using System.Collections.Generic;

public partial class ForceField : Area2D
{
    [Export] public int BaseDamage { get; set; } = 15;
    [Export] public float KnockbackForce { get; set; } = 300f;
    [Export] public float CooldownTime { get; set; } = 3.0f;

    public bool UsePerEnemyCooldown { get; set; } = false;
    public float PerEnemyCooldownTime { get; set; } = 2.0f;
    public float RadiusMultiplier { get; set; } = 1.0f;

    public bool HasOuterSlowRing { get; set; } = false;
    public float SlowRingRadiusMultiplier { get; set; } = 2.0f;
    public float SlowAmount { get; set; } = 0.4f;

    private Timer _cooldownTimer;
    private CollisionShape2D _collider;
    private Node2D _visuals;

    private Dictionary<ulong, double> _enemyCooldowns = new Dictionary<ulong, double>();
    private Dictionary<ulong, float> _slowedEnemies = new Dictionary<ulong, float>();
    private Area2D _outerRing;

    public override void _Ready()
{
    TopLevel = false;
    Position = Vector2.Zero; // Perfectly centered on player

    _cooldownTimer = GetNodeOrNull<Timer>("CooldownTimer") ?? GetNode<Timer>("Timer");
    _collider = GetNode<CollisionShape2D>("CollisionShape2D");
    _visuals = GetNodeOrNull<Node2D>("Sprite2D") ?? GetNodeOrNull<Node2D>("Visuals");

    _cooldownTimer.WaitTime = CooldownTime;
    _cooldownTimer.OneShot = true;
    _cooldownTimer.Timeout += Reactivate;

    if (RadiusMultiplier != 1.0f && _collider.Shape is CircleShape2D innerCircle)
    {
        innerCircle.Radius *= RadiusMultiplier;
    }

    BodyEntered += OnBodyEntered;
    AreaEntered += OnAreaEntered;

    if (HasOuterSlowRing)
    {
        SpawnOuterSlowRing();
    }
}

public override void _Process(double delta)
{
    // Outer slow ring following the shield (Stage 3)
    if (_outerRing != null && IsInstanceValid(_outerRing))
    {
        _outerRing.GlobalPosition = GlobalPosition;
    }

    if (UsePerEnemyCooldown)
    {
        List<ulong> toRemove = new List<ulong>();
        foreach (var pair in _enemyCooldowns)
        {
            if (pair.Value - delta <= 0)
                toRemove.Add(pair.Key);
            else
                _enemyCooldowns[pair.Key] = pair.Value - delta;
        }
        foreach (var key in toRemove) _enemyCooldowns.Remove(key);
    }
}

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy) TriggerShield(enemy);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent() is Enemy enemyParent)
            TriggerShield(enemyParent);
    }

    private void TriggerShield(Enemy enemy)
    {
        ulong id = enemy.GetInstanceId();

        if (UsePerEnemyCooldown)
        {
            if (_enemyCooldowns.ContainsKey(id)) return;

            enemy.TakeDamage(BaseDamage);
            Vector2 pushDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
            enemy.ApplyKnockback(pushDir, KnockbackForce);

            _enemyCooldowns[id] = PerEnemyCooldownTime;
        }
        else
        {
            enemy.TakeDamage(BaseDamage);
            Vector2 pushDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
            enemy.ApplyKnockback(pushDir, KnockbackForce);
            Deactivate();
        }
    }

    private void Deactivate()
    {
        _collider.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        if (_visuals != null) _visuals.Visible = false;
        _cooldownTimer.Start();
    }

    private void Reactivate()
    {
        _collider.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        if (_visuals != null) _visuals.Visible = true;
    }

    private void SpawnOuterSlowRing()
    {
        float innerRadius = (_collider.Shape is CircleShape2D c) ? c.Radius * _collider.Scale.X : 58f * _collider.Scale.X;

        _outerRing = new Area2D();
        _outerRing.Name = "SlowRing";
        _outerRing.CollisionMask = CollisionMask;

        CollisionShape2D ringShape = new CollisionShape2D();
        CircleShape2D circle = new CircleShape2D();
        circle.Radius = innerRadius * SlowRingRadiusMultiplier * 0.50f;
        ringShape.Shape = circle;
        _outerRing.AddChild(ringShape);

        if (_visuals is Sprite2D mainSprite)
        {
            Sprite2D ringSprite = (Sprite2D)mainSprite.Duplicate();
            ringSprite.Scale = mainSprite.Scale * SlowRingRadiusMultiplier;
            ringSprite.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.25f); 
            _outerRing.AddChild(ringSprite);
        }

        // Connect both enter AND exit signals
        _outerRing.BodyEntered += OnOuterRingBodyEntered;
        _outerRing.BodyExited += OnOuterRingBodyExited;
        _outerRing.AreaEntered += OnOuterRingAreaEntered;
        _outerRing.AreaExited += OnOuterRingAreaExited;

        GetParent().AddChild(_outerRing);
        _outerRing.GlobalPosition = GlobalPosition;
    }

    private void OnOuterRingBodyEntered(Node2D body)
    {
        if (body is Enemy enemy) ApplySlow(enemy);
    }

    private void OnOuterRingBodyExited(Node2D body)
    {
        if (body is Enemy enemy) RemoveSlow(enemy);
    }

    private void OnOuterRingAreaEntered(Area2D area)
    {
        if (area.GetParent() is Enemy enemy) ApplySlow(enemy);
    }

    private void OnOuterRingAreaExited(Area2D area)
    {
        if (area.GetParent() is Enemy enemy) RemoveSlow(enemy);
    }

    private void ApplySlow(Enemy enemy)
    {
        ulong id = enemy.GetInstanceId();
        
        // Only slow them if they aren't already tracked (prevents overlapping bug)
        if (!_slowedEnemies.ContainsKey(id))
        {
            _slowedEnemies[id] = enemy.MoveSpeed; // Save their normal speed
            enemy.MoveSpeed *= SlowAmount;        // Apply the slow
        }
    }

    private void RemoveSlow(Enemy enemy)
    {
        ulong id = enemy.GetInstanceId();
        
        // If they leave the ring, restore their exact original speed
        if (_slowedEnemies.ContainsKey(id))
        {
            enemy.MoveSpeed = _slowedEnemies[id];
            _slowedEnemies.Remove(id);
        }
    }

    public override void _ExitTree()
    {
        // Restore all speeds if the ring is destroyed while enemies are inside
        foreach (var pair in _slowedEnemies)
        {
            Enemy enemy = (Enemy)InstanceFromId(pair.Key);
            if (enemy != null && IsInstanceValid(enemy))
            {
                enemy.MoveSpeed = pair.Value;
            }
        }

        _slowedEnemies.Clear();
        if (_outerRing != null && IsInstanceValid(_outerRing))
        {
            _outerRing.QueueFree();
        }
    }

    public void ApplyUpgrades(int stage)
    {
        if (stage >= 2)
        {
            UsePerEnemyCooldown = true;
            RadiusMultiplier = 1.5f; // Fixed: Set to 1.5 instead of 1.0

            if (_collider != null) 
                _collider.Scale = new Vector2(RadiusMultiplier, RadiusMultiplier);
            
            if (_visuals != null) 
                _visuals.Scale = new Vector2(RadiusMultiplier, RadiusMultiplier);
        }

        if (stage >= 3)
        {
            HasOuterSlowRing = true;
            SlowRingRadiusMultiplier = 2.0f;

            if (_outerRing == null)
            {
                SpawnOuterSlowRing();
            }
        }
    }
}