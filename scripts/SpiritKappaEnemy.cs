using Godot;

/// <summary>
/// Area 2 — Ranged Spirit: "Spirit Kappa"
/// Stationary attacker — does NOT chase the player.
/// Every AttackCooldown seconds it fires 4 projectiles in a cross (+) pattern
/// aimed at the player's current position (slingshot-style, no homing after launch).
/// Uses Spirit Kappa.png (192x32 → 6 frames @ 32x32).
/// </summary>
public partial class SpiritKappaEnemy : Enemy
{
    [ExportGroup("Kappa Settings")]
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public float AttackCooldown { get; set; } = 2.8f;   // seconds between cross-shots
    [Export] public float ProjectileSpeed { get; set; } = 340f;
    [Export] public int ProjectileDamage { get; set; } = 18;

    private float _attackTimer = 0f;

    // Spirit Kappa does NOT move — it teleports near the player then stays put.
    private const float TeleportRadius = 380f;    // Spawns this far from the player
    private bool _positioned = false;

    public override void _Ready()
    {
        if (MaxHealth == 30) MaxHealth = 55;
        if (MoveSpeed == 70.0f) MoveSpeed = 0f;  // stationary

        base._Ready();

        // Snap to a random edge position near the player on spawn
        PositionNearPlayer();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Kappa does not move after positioning
        Velocity = Vector2.Zero;
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        if (TargetPlayer == null || !IsInstanceValid(TargetPlayer))
        {
            FindPlayer();
            return;
        }

        _attackTimer -= (float)delta;
        if (_attackTimer <= 0f)
        {
            FireCrossPattern();
            _attackTimer = AttackCooldown;
        }
    }

    private void PositionNearPlayer()
    {
        if (TargetPlayer == null) return;

        // Place Kappa at a random angle at TeleportRadius from the player
        float angle = GD.Randf() * Mathf.Tau;
        GlobalPosition = TargetPlayer.GlobalPosition + new Vector2(
            Mathf.Cos(angle) * TeleportRadius,
            Mathf.Sin(angle) * TeleportRadius
        );
        _positioned = true;
    }

    private void FireCrossPattern()
    {
        if (ProjectileScene == null || TargetPlayer == null) return;

        // The primary arm always points AT the player (slingshot aim)
        Vector2 toPlayer = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();

        // Fire 4 arms: toward player, away, left-perp, right-perp
        Vector2[] directions = new Vector2[]
        {
            toPlayer,
            -toPlayer,
            toPlayer.Rotated(Mathf.Pi * 0.5f),
            toPlayer.Rotated(-Mathf.Pi * 0.5f),
        };

        foreach (var dir in directions)
        {
            SpiritKappaProjectile proj = ProjectileScene.Instantiate<SpiritKappaProjectile>();
            proj.GlobalPosition = GlobalPosition;
            proj.Direction = dir;
            proj.Speed = ProjectileSpeed;
            proj.DamageAmount = ProjectileDamage;
            GetTree().CurrentScene.AddChild(proj);
        }
    }
}
