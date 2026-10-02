using Godot;
using System;

public enum FleeState
{
    Idle,
    Flee,
    Dead
}

public partial class FleeingEnemy : CharacterBody2D
{
    [ExportGroup("Movement & Speeds")]
    [Export] public float FleeSpeed { get; set; } = 220f;
    [Export] public float WanderSpeed { get; set; } = 45f;

    [ExportGroup("Detection Boundaries")]
    [Export] public float PanicRadius { get; set; } = 50f;       // Player breaches this circle -> triggers panic
    [Export] public float SafeEscapeDistance { get; set; } = 500f; // Enemy must reach this distance to calm down

    [ExportGroup("Stats & Drops")]
    [Export] public int MaxHealth { get; set; } = 30;
    [Export] public PackedScene GemScene { get; set; }

    public int CurrentHealth { get; private set; }
    private FleeState _currentState = FleeState.Idle;
    private Node2D _player;
    private Sprite2D _sprite;
    private Vector2 _wanderDirection = Vector2.Zero;
    private float _wanderTimer = 0f;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();
        CurrentHealth = MaxHealth;
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        FindPlayer();
        TransitionTo(FleeState.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_currentState == FleeState.Dead) return;

        // Reacquire player if reference was lost or not loaded yet
        if (_player == null || !IsInstanceValid(_player))
        {
            FindPlayer();
            if (_player == null) return;
        }

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        switch (_currentState)
        {
            case FleeState.Idle:
                HandleIdleState(distanceToPlayer, (float)delta);
                break;

            case FleeState.Flee:
                HandleFleeState(distanceToPlayer);
                break;
        }

        MoveAndSlide();
    }

    private void FindPlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;

        if (_player == null && GetTree().CurrentScene != null)
        {
            _player = GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");
        }
    }

    public void TransitionTo(FleeState newState)
    {
        if (_currentState == FleeState.Dead) return;

        _currentState = newState;

        switch (_currentState)
        {
            case FleeState.Idle:
                Modulate = new Color(1f, 1f, 1f); // Calm white tint
                PickNewWanderDirection();
                break;

            case FleeState.Flee:
                Modulate = new Color(1f, 0.85f, 0.2f); // Golden panic tell
                break;

            case FleeState.Dead:
                Die();
                break;
        }
    }

    private void HandleIdleState(float distanceToPlayer, float delta)
    {
        // 1. Check if the player entered the enemy's personal danger bubble
        if (distanceToPlayer <= PanicRadius)
        {
            TransitionTo(FleeState.Flee);
            return;
        }

        // 2. Otherwise, wander around lazily
        _wanderTimer -= delta;
        if (_wanderTimer <= 0f)
        {
            PickNewWanderDirection();
        }

        Velocity = _wanderDirection * WanderSpeed;
        UpdateFacing(Velocity.X);
    }

    private void HandleFleeState(float distanceToPlayer)
    {
        // 1. Check if the enemy successfully escaped beyond the safe distance
        if (distanceToPlayer >= SafeEscapeDistance)
        {
            TransitionTo(FleeState.Idle);
            return;
        }

        // 2. Sprint directly away from the player: (EnemyPosition - PlayerPosition)
        Vector2 fleeDirection = (GlobalPosition - _player.GlobalPosition).Normalized();
        Velocity = fleeDirection * FleeSpeed;
        UpdateFacing(Velocity.X);
    }

    private void PickNewWanderDirection()
    {
        float angle = _rng.RandfRange(0f, Mathf.Tau);
        _wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        _wanderTimer = _rng.RandfRange(1.5f, 3.0f);
    }

    private void UpdateFacing(float moveX)
    {
        if (_sprite != null && MathF.Abs(moveX) > 1f)
        {
            _sprite.FlipH = moveX < 0;
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;

        // An attack immediately panics the enemy even if outside the radius
        if (_currentState == FleeState.Idle)
        {
            TransitionTo(FleeState.Flee);
        }

        if (CurrentHealth <= 0 && _currentState != FleeState.Dead)
        {
            TransitionTo(FleeState.Dead);
        }
    }

    private void Die()
    {
        if (GemScene != null)
        {
            ExperienceGem gem = GemScene.Instantiate<ExperienceGem>();
            gem.GlobalPosition = GlobalPosition;
            gem.ExperienceValue = 50;
            GetTree().CurrentScene.CallDeferred("add_child", gem);
        }

        QueueFree();
    }
}