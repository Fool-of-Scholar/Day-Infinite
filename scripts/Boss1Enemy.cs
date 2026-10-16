using Godot;
using System;

public enum BossState
{
    Chase,
    Windup,
    Dash,
    Rest,
    Enrage,  // Phase 2: below 40% HP — faster, more aggressive
    Dead
}

public partial class Boss1Enemy : Enemy
{
    [ExportGroup("Boss Specific")]
    [Export] public float DashSpeed { get; set; } = 420f;  // Balanced: was 380 — feels more threatening

    // --- PHASE THRESHOLDS ---
    private const float Phase2HpThreshold = 0.40f; // Triggers Enrage at 40% HP
    private bool _phase2Triggered = false;

    private BossState _state = BossState.Chase;
    private AnimatedSprite2D _bloodSprite;
    private CollisionShape2D _collisionShape;
    private Vector2 _dashDirection = Vector2.Zero;
    private float _stateTimer = 0f;

    public override void _Ready()
    {
        // Set HP BEFORE base._Ready() so Enemy initializes CurrentHealth correctly
        MaxHealth = 1500;

        // Executes base Enemy initialization (adds to "enemies" group, sets CurrentHealth = MaxHealth)
        base._Ready();

        _bloodSprite = GetNodeOrNull<AnimatedSprite2D>("Blood");
        _collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (_bloodSprite != null)
        {
            _bloodSprite.Visible = false;
            _bloodSprite.AnimationFinished += () => _bloodSprite.Visible = false;
        }

        TransitionTo(BossState.Chase);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == BossState.Dead) return;

        if (TargetPlayer == null || !IsInstanceValid(TargetPlayer))
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        _stateTimer -= (float)delta;

        // Check Phase 2 threshold
        CheckPhaseTransition();

        switch (_state)
        {
            case BossState.Chase:
                Vector2 dir = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
                Velocity = dir * MoveSpeed;
                UpdateFacing(Velocity.X);
                MoveAndSlide();

                if (_stateTimer <= 0f) TransitionTo(BossState.Windup);
                break;

            case BossState.Windup:
                Velocity = Vector2.Zero;
                UpdateFacing((TargetPlayer.GlobalPosition - GlobalPosition).X);

                if (_stateTimer <= 0f) TransitionTo(BossState.Dash);
                break;

            case BossState.Dash:
                Velocity = _dashDirection * DashSpeed;
                UpdateFacing(Velocity.X);
                MoveAndSlide();

                if (_stateTimer <= 0f) TransitionTo(BossState.Rest);
                break;

            case BossState.Rest:
                Velocity = Vector2.Zero;

                if (_stateTimer <= 0f) TransitionTo(BossState.Chase);
                break;

            case BossState.Enrage:
                // Phase 2: relentlessly chases, no rest state between dashes
                Vector2 enrageDir = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
                Velocity = enrageDir * (MoveSpeed * 1.5f);  // 50% faster chase
                UpdateFacing(Velocity.X);
                MoveAndSlide();

                if (_stateTimer <= 0f) TransitionTo(BossState.Windup); // Goes straight to windup again
                break;
        }
    }

    private void CheckPhaseTransition()
    {
        if (!_phase2Triggered && MaxHealth > 0)
        {
            float hpPercent = (float)CurrentHealth / MaxHealth;
            if (hpPercent <= Phase2HpThreshold)
            {
                _phase2Triggered = true;
                EnterPhase2();
            }
        }
    }

    private void EnterPhase2()
    {
        GD.Print("[Boss] PHASE 2 TRIGGERED — Boss is ENRAGED!");

        // Flash a strong warning color
        Modulate = new Color(1f, 0.1f, 0.1f);

        // Reduce dash speed timer (shorter rests = more aggressive)
        DashSpeed = 520f;

        // Give boss a brief speed boost as phase 2 visual cue
        TransitionTo(BossState.Enrage);
    }

    public void TransitionTo(BossState newState)
    {
        if (_state == BossState.Dead) return;
        _state = newState;

        switch (_state)
        {
            case BossState.Chase:
                // Phase 2: shorter chase window before attacking
                _stateTimer = _phase2Triggered ? 2.5f : 4.0f;
                Modulate = new Color(1f, 1f, 1f);
                PlayAnimation("walk");
                break;

            case BossState.Windup:
                // Phase 2: shorter windup = less time to dodge
                _stateTimer = _phase2Triggered ? 0.6f : 1.0f;
                _dashDirection = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
                Modulate = new Color(1f, 0.3f, 0.3f); // Red warning glow
                PlayAnimation("windup", fallback: "rest");
                break;

            case BossState.Dash:
                // Phase 2: faster dash duration
                _stateTimer = _phase2Triggered ? 0.5f : 0.7f;
                Modulate = new Color(1f, 0.9f, 0.3f);
                PlayAnimation("dash", fallback: "walk");
                break;

            case BossState.Rest:
                // Phase 2: barely rests
                _stateTimer = _phase2Triggered ? 0.8f : 1.8f;
                Modulate = new Color(0.6f, 0.6f, 0.8f);
                PlayAnimation("rest");
                break;

            case BossState.Enrage:
                // Brief aggressive pursuit before first P2 dash
                _stateTimer = 1.5f;
                Modulate = new Color(1.0f, 0.15f, 0.15f);
                PlayAnimation("walk");
                break;

            case BossState.Dead:
                PlayDeathSequence();
                break;
        }
    }

    private void PlayAnimation(string animName, string fallback = "walk")
    {
        if (_animSprite == null || _animSprite.SpriteFrames == null) return;

        string target = _animSprite.SpriteFrames.HasAnimation(animName) ? animName : fallback;
        if (!_animSprite.SpriteFrames.HasAnimation(target))
        {
            target = "default";
        }
        if (_animSprite.SpriteFrames.HasAnimation(target))
        {
            _animSprite.Play(target);
        }
    }

    // Overrides base Enemy.TakeDamage with hit reactions and state handling
    public override void TakeDamage(int damageAmount)
    {
        if (_state == BossState.Dead) return;

        base.TakeDamage(damageAmount);
        GD.Print($"[Boss Damaged] HP: {CurrentHealth}/{MaxHealth} ({(float)CurrentHealth / MaxHealth * 100f:F0}%)");
    }

    protected override void Die()
    {
        // Spawn blood effect on boss death
        SpawnBlood();

        // Begin boss death animation / world cleanse
        TransitionTo(BossState.Dead);
    }

    private void TriggerBlood()
    {
        if (_bloodSprite == null) return;
        _bloodSprite.Visible = true;
        _bloodSprite.Frame = 0;
        _bloodSprite.Play();
    }

    private async void PlayDeathSequence()
    {
        Velocity = Vector2.Zero;
        Modulate = new Color(1f, 1f, 1f);
        _collisionShape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

        if (_animSprite != null && _animSprite.SpriteFrames.HasAnimation("die"))
        {
            _animSprite.Play("die");
            await ToSignal(_animSprite, AnimatedSprite2D.SignalName.AnimationFinished);
        }

        PerformWorldCleanse();
        QueueFree();
    }

    private void PerformWorldCleanse()
    {
        // 1. Wipe all active minions
        Node2D container = GetTree().CurrentScene.GetNodeOrNull<Node2D>("EnemyContainer");
        if (container != null)
        {
            foreach (Node child in container.GetChildren())
            {
                if (child != this) child.QueueFree();
            }
        }
        else
        {
            var regularEnemies = GetTree().GetNodesInGroup("enemies");
            foreach (Node node in regularEnemies)
            {
                if (node != this && node is Node2D minion) minion.QueueFree();
            }
        }

        // 2. Vacuum all loose gems on the field
        if (TargetPlayer != null)
        {
            var gems = GetTree().GetNodesInGroup("gems");
            foreach (Node node in gems)
            {
                if (node is ExperienceGem gem) gem.TriggerVacuum(TargetPlayer);
            }
        }

        // 3. Drop boss core gem — big XP reward for winning
        if (GemScene != null)
        {
            ExperienceGem bossGem = GemScene.Instantiate<ExperienceGem>();
            bossGem.GlobalPosition = GlobalPosition;
            bossGem.ExperienceValue = 1000;   // Balanced: was 500 — more rewarding final boss drop
            GetTree().CurrentScene.CallDeferred("add_child", bossGem);
            if (TargetPlayer != null) bossGem.TriggerVacuum(TargetPlayer);
        }
    }
}