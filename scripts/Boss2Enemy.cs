using Godot;
using System;

/// <summary>
/// Boss 2 state machine.
/// Chase → RootAttack OR MistAttack → Recovery → Chase
/// Phase 2 (below 40% HP): cycles attacks faster, no recovery gap.
/// Does NOT trigger PerformWorldCleanse on death — just drops gems and dies normally.
/// </summary>
public enum Boss2State
{
    Chase,
    RootWindup,    // Brief lock-on before root burst
    RootAttack,    // Fires root projectiles (follows player position)
    MistWindup,    // Brief lock-on before mist wave
    MistAttack,    // Mist wave expanding outward
    Recovery,      // Brief pause between attacks
    Enrage,        // Phase 2 chase
    Dead
}

public partial class Boss2Enemy : Enemy
{
    // ── Exports ──────────────────────────────────────────────────────────────
    [ExportGroup("Boss 2 Specifics")]
    [Export] public PackedScene RootProjectileScene { get; set; }
    [Export] public PackedScene MistProjectileScene  { get; set; }

    [Export] public float RootProjectileSpeed  { get; set; } = 220f;
    [Export] public int   RootProjectileDamage { get; set; } = 25;
    [Export] public float MistProjectileSpeed  { get; set; } = 160f;
    [Export] public int   MistProjectileDamage { get; set; } = 18;

    // ── Private ───────────────────────────────────────────────────────────────
    private Boss2State _state = Boss2State.Chase;
    private float _stateTimer = 0f;
    private bool _phase2Triggered = false;

    // Which attack type to fire next (alternates Root → Mist → Root …)
    private bool _nextAttackIsRoot = true;

    private const float Phase2HpThreshold = 0.40f;

    public override void _Ready()
    {
        // Set boss HP before base._Ready() so CurrentHealth initialises correctly
        if (MaxHealth == 30) MaxHealth = 6500;
        if (MoveSpeed == 70f) MoveSpeed = 62f;   // Slower than boss 1 — it's more of an area-denial boss

        base._Ready();
        TransitionTo(Boss2State.Chase);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == Boss2State.Dead) return;

        if (TargetPlayer == null || !IsInstanceValid(TargetPlayer))
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        _stateTimer -= (float)delta;
        CheckPhase2();

        switch (_state)
        {
            // ── Chase ──────────────────────────────────────────────────────────
            case Boss2State.Chase:
            case Boss2State.Enrage:
            {
                float speed = _state == Boss2State.Enrage ? MoveSpeed * 1.45f : MoveSpeed;
                Vector2 dir = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
                Velocity = dir * speed;
                UpdateFacing(Velocity.X);
                MoveAndSlide();

                if (_stateTimer <= 0f)
                {
                    // Alternate between root and mist windup
                    TransitionTo(_nextAttackIsRoot ? Boss2State.RootWindup : Boss2State.MistWindup);
                    _nextAttackIsRoot = !_nextAttackIsRoot;
                }
                break;
            }

            // ── Root / Mist Windup — boss stops, telegraphs ───────────────────
            case Boss2State.RootWindup:
            case Boss2State.MistWindup:
            {
                Velocity = Vector2.Zero;
                UpdateFacing((TargetPlayer.GlobalPosition - GlobalPosition).X);
                MoveAndSlide();

                if (_stateTimer <= 0f)
                {
                    TransitionTo(_state == Boss2State.RootWindup
                        ? Boss2State.RootAttack
                        : Boss2State.MistAttack);
                }
                break;
            }

            // ── Attacks (movement stops while firing) ─────────────────────────
            case Boss2State.RootAttack:
            case Boss2State.MistAttack:
            {
                Velocity = Vector2.Zero;
                MoveAndSlide();

                if (_stateTimer <= 0f)
                    TransitionTo(Boss2State.Recovery);
                break;
            }

            // ── Recovery ──────────────────────────────────────────────────────
            case Boss2State.Recovery:
            {
                Velocity = Vector2.Zero;
                MoveAndSlide();
                if (_stateTimer <= 0f)
                    TransitionTo(_phase2Triggered ? Boss2State.Enrage : Boss2State.Chase);
                break;
            }
        }
    }

    // ── State Machine ─────────────────────────────────────────────────────────
    private void TransitionTo(Boss2State next)
    {
        if (_state == Boss2State.Dead) return;
        _state = next;

        switch (_state)
        {
            case Boss2State.Chase:
                _stateTimer = _phase2Triggered ? 2.8f : 4.5f;
                Modulate = Colors.White;
                PlayAnim("walk");
                break;

            case Boss2State.Enrage:
                _stateTimer = 2.0f;
                Modulate = new Color(1.0f, 0.15f, 0.15f);
                PlayAnim("walk");
                break;

            case Boss2State.RootWindup:
                _stateTimer = _phase2Triggered ? 0.55f : 0.9f;
                Modulate = new Color(0.3f, 1.0f, 0.4f);  // Green glow — root warning
                PlayAnim("root_attack");
                break;

            case Boss2State.RootAttack:
                _stateTimer = _phase2Triggered ? 0.5f : 0.7f;
                Modulate = new Color(0.4f, 1.0f, 0.5f);
                FireRootAttack();
                PlayAnim("root_attack");
                break;

            case Boss2State.MistWindup:
                _stateTimer = _phase2Triggered ? 0.55f : 0.9f;
                Modulate = new Color(0.6f, 0.6f, 1.0f);  // Blue glow — mist warning
                PlayAnim("mist_attack");
                break;

            case Boss2State.MistAttack:
                _stateTimer = _phase2Triggered ? 0.5f : 0.7f;
                Modulate = new Color(0.7f, 0.7f, 1.0f);
                FireMistAttack();
                PlayAnim("mist_attack");
                break;

            case Boss2State.Recovery:
                _stateTimer = _phase2Triggered ? 0.6f : 1.4f;
                Modulate = new Color(0.8f, 0.8f, 0.8f);
                PlayAnim("walk");
                break;

            case Boss2State.Dead:
                PlayDeathSequence();
                break;
        }
    }

    // ── Attack Implementations ────────────────────────────────────────────────

    /// <summary>
    /// Root attack: Summons erupting root hazards from the earth directly at the player's position.
    /// Telegraph indicator displays on the ground for 0.75s, giving the player time to dodge.
    /// In Phase 2 (enraged), spawns a cluster of 3 root traps to entrap the player.
    /// </summary>
    private void FireRootAttack()
    {
        if (RootProjectileScene == null || TargetPlayer == null) return;

        // Primary ground trap directly on the player's ground position
        SpawnRootHazard(TargetPlayer.GlobalPosition);

        if (_phase2Triggered)
        {
            Vector2 forward = Vector2.Right;
            if (TargetPlayer is Movement p && p.Velocity != Vector2.Zero)
                forward = p.Velocity.Normalized();
            else
                forward = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();

            SpawnRootHazard(TargetPlayer.GlobalPosition + forward.Rotated(0.7f) * 65f);
            SpawnRootHazard(TargetPlayer.GlobalPosition + forward.Rotated(-0.7f) * 65f);
        }
    }

    private void SpawnRootHazard(Vector2 position)
    {
        Node2D root = RootProjectileScene.Instantiate<Node2D>();
        root.GlobalPosition = position;
        root.Set("DamageAmount", RootProjectileDamage);
        GetTree().CurrentScene.AddChild(root);
    }

    /// <summary>
    /// Mist attack: fires 8 mist projectiles in all cardinal + diagonal directions.
    /// Slower, wider spread — area denial.
    /// </summary>
    private void FireMistAttack()
    {
        if (MistProjectileScene == null || TargetPlayer == null) return;

        // 8-way spread from the boss position
        for (int i = 0; i < 8; i++)
        {
            float angle = (Mathf.Tau / 8f) * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            Node2D proj = MistProjectileScene.Instantiate<Node2D>();
            proj.GlobalPosition = GlobalPosition;
            proj.Set("Direction", dir);
            proj.Set("Speed", MistProjectileSpeed);
            proj.Set("DamageAmount", MistProjectileDamage);
            GetTree().CurrentScene.AddChild(proj);
        }
    }

    private void CheckPhase2()
    {
        if (_phase2Triggered || MaxHealth <= 0) return;
        if ((float)CurrentHealth / MaxHealth <= Phase2HpThreshold)
        {
            _phase2Triggered = true;
            GD.Print("[Boss2] PHASE 2 — Enraged!");
            Modulate = new Color(1f, 0.1f, 0.1f);
            TransitionTo(Boss2State.Enrage);
        }
    }

    // ── Animation Helper ──────────────────────────────────────────────────────
    private void PlayAnim(string anim, string fallback = "walk")
    {
        if (_animSprite == null || _animSprite.SpriteFrames == null) return;
        string target = _animSprite.SpriteFrames.HasAnimation(anim)
            ? anim
            : (_animSprite.SpriteFrames.HasAnimation(fallback) ? fallback : "default");
        if (_animSprite.SpriteFrames.HasAnimation(target))
            _animSprite.Play(target);
    }

    // ── Death ─────────────────────────────────────────────────────────────────
    protected override void Die()
    {
        SpawnBlood();
        TransitionTo(Boss2State.Dead);
    }

    private void PlayDeathSequence()
    {
        Velocity = Vector2.Zero;
        Modulate = Colors.White;

        var col = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        col?.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

        Tween deathTween = CreateTween();
        if (deathTween == null)
        {
            PerformWorldCleanse();
            QueueFree();
            return;
        }

        // Brief flash before vanishing using safe node-bound Tween
        for (int i = 0; i < 3; i++)
        {
            deathTween.TweenProperty(this, "modulate", new Color(2f, 2f, 2f), 0.1f);
            deathTween.TweenProperty(this, "modulate", Colors.White, 0.1f);
        }

        deathTween.TweenCallback(Callable.From(() =>
        {
            PerformWorldCleanse();
            QueueFree();
        }));
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


