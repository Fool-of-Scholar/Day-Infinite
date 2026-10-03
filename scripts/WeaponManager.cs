using Godot;
using System;

public partial class WeaponManager : Node2D
{
    [Export] public PackedScene GunVisualScene { get; set; } 
    [Export] public PackedScene BulletScene { get; set; } 
    
    [Export] public float AttackInterval { get; set; } = 0.3f;
    [Export] public float Range { get; set; } = 600f;

    // --- NEW STAT TRACKERS ---
    public int BonusDamage { get; set; } = 0;
    public int ProjectileCount { get; set; } = 1;
    public float SpreadAngle { get; set; } = 0.25f; // Radians for how far apart bullets spread

    private Timer _timer;
    private Node2D _gunContainer;
    private Node2D _bulletSpawn;

    public override void _Ready()
    {
        _gunContainer = GetNodeOrNull<Node2D>("Gun");
        if (_gunContainer == null) {
            _gunContainer = new Node2D();
            _gunContainer.Name = "Gun";
            AddChild(_gunContainer);
        }

        _bulletSpawn = GetNodeOrNull<Node2D>("Bullet");
        if (_bulletSpawn == null) {
            _bulletSpawn = new Node2D();
            _bulletSpawn.Name = "Bullet";
            AddChild(_bulletSpawn);
        }

        if (GunVisualScene != null)
        {
            Node2D visual = GunVisualScene.Instantiate<Node2D>();
            _gunContainer.AddChild(visual);
        }

        _timer = new Timer();
        AddChild(_timer);
        _timer.WaitTime = AttackInterval;
        _timer.Timeout += Fire;
        _timer.Start(); 
    }

    private void Fire()
    {
        if (BulletScene == null) return;

        Node2D nearestEnemy = GetNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 baseDir = (nearestEnemy.GlobalPosition - _bulletSpawn.GlobalPosition).Normalized();

        // Loop to spawn multiple projectiles if upgraded
        for (int i = 0; i < ProjectileCount; i++)
        {
            Node2D boltNode = BulletScene.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(boltNode);
            boltNode.GlobalPosition = _bulletSpawn.GlobalPosition;

            // Math to fan the bullets out evenly
            float angleOffset = 0;
            if (ProjectileCount > 1)
            {
                angleOffset = SpreadAngle * (i - (ProjectileCount - 1) / 2.0f);
            }
            Vector2 finalDir = baseDir.Rotated(angleOffset);

            if (boltNode is ARBullet arBolt)
            {
                arBolt.Direction = finalDir;
                // Add the accumulated bonus damage directly to the bullet!
                arBolt.Damage += BonusDamage; 
            }
            else if (boltNode is RocketBullet rocketBolt)
            {
                rocketBolt.Direction = finalDir;
                rocketBolt.Damage += BonusDamage;
            }
        }
    }

    private Node2D GetNearestEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D closest = null;
        float shortestDistance = Range;

        foreach (Node node in enemies)
        {
            if (node is Node2D enemy)
            {
                float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closest = enemy;											
                }
            }
        }
        return closest;
    }

    // --- HUD UPGRADE METHODS ---
    
    // HUD Option 1: +30% Fire Rate
    public void UpgradeFireRate()
    {
        AttackInterval *= 0.70f; // Reduces interval by 30%, making it shoot faster
        _timer.WaitTime = AttackInterval;
        GD.Print($"Upgraded Fire Rate! New interval: {AttackInterval}");
    }

    // HUD Option 2: +1 Projectile
    public void UpgradeProjectile()
    {
        ProjectileCount += 1;
        GD.Print($"Upgraded Projectiles! Shooting {ProjectileCount} bullets.");
    }

    // HUD Option 3: +20 Damage
    public void UpgradeDamage()
    {
        BonusDamage += 20;
        GD.Print($"Upgraded Damage! Bonus is now +{BonusDamage}");
    }
    
    // Add this inside WeaponManager.cs
    public void ChangeWeapon(PackedScene newGunScene, PackedScene newBulletScene, float newAttackInterval)
    {
        GunVisualScene = newGunScene;
        BulletScene = newBulletScene;
        
        AttackInterval = newAttackInterval;
        _timer.WaitTime = AttackInterval;

        // 1. Remove the old gun visual
        foreach (Node child in _gunContainer.GetChildren())
        {
            child.QueueFree();
        }

        // 2. Instantiate and add the new gun visual
        if (GunVisualScene != null)
        {
            Node2D visual = GunVisualScene.Instantiate<Node2D>();
            _gunContainer.AddChild(visual);
        }

        GD.Print("Weapon swapped successfully!");
    }
}