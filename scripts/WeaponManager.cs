using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponManager : Node2D
{
    SaveManager _saveManager;
    private int _weaponPickInt;
    private PackedScene[] _weaponPick = new PackedScene[]
    {
        GD.Load<PackedScene>("res://weapon_new/ar/ar_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/rocket/rocket_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/rocket/rocket_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/mine/mine.tscn"),
        GD.Load<PackedScene>("res://weapon_new/force field/force_field.tscn"),
    };

    [Export] public PackedScene GunVisualScene { get; set; } 
    
    // Default starting bullet (AR)
    [Export] public PackedScene DefaultBulletScene { get; set; } 

    // --- UPGRADE LIMITS ---
    public int TotalUpgradePointsUsed = 0;
    public const int MaxUpgradePoints = 8;   // Balanced: was 5 — allows richer build depth over 10 min
    public const int MaxWeapons = 3;
    
    public class WeaponSlot 
    {
        public string WeaponId;
        public int Stage = 1; // Tracks the upgrade level for this specific weapon
        public PackedScene BulletScene;
        public Timer FireTimer;
    }

    public List<WeaponSlot> EquippedWeapons = new List<WeaponSlot>();
    
    // --- PATHS FOR DYNAMIC LOADING ---
    public Dictionary<string, string> WeaponPaths = new Dictionary<string, string>
    {
        { "AR", "res://weapon_new/ar/ar_bullet.tscn" },
        { "Mine", "res://weapon_new/mine/mine.tscn" },
        { "Rocket", "res://weapon_new/rocket/rocket_bullet.tscn" },
        {"Tesla", "res://weapon_new/tesla/tesla_burst.tscn"},
        { "ForceField", "res://weapon_new/force field/force_field.tscn" } // You can create this tonight!
    };

    // --- GLOBAL STAT TRACKERS ---
    public int BonusDamage { get; set; } = 0;
    public int ProjectileCount { get; set; } = 1;
    public float SpreadAngle { get; set; } = 0.25f; 
    public float FireRateMultiplier { get; set; } = 1.0f;
    [Export] public float Range { get; set; } = 600f;

    private Node2D _gunContainer;
    private Node2D _bulletSpawn;

    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        TriggerLoad();
        DefaultBulletScene = _weaponPick[_weaponPickInt - 1];

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

        // --- Bullet TESTING ---
        if (DefaultBulletScene != null)
        {
            EquipNewWeapon("AR", DefaultBulletScene);
        }
        else 
        {
            EquipNewWeapon("AR", GD.Load<PackedScene>(WeaponPaths["AR"]));
        }

        // --- INSTANTLY EQUIP FORCE FIELD ---
        // Comment Bullet TESTING first before uncommenting this
        // EquipNewWeapon("ForceField");
    }

    // --- WEAPON INVENTORY LOGIC ---
    public void EquipNewWeapon(string weaponId, PackedScene bulletScene = null)
    {
        if (EquippedWeapons.Count >= MaxWeapons) return; 
        
        foreach(var w in EquippedWeapons) 
        {
            if (w.WeaponId == weaponId) return; // Already equipped, shouldn't equip again
        }

        if (bulletScene == null && WeaponPaths.ContainsKey(weaponId))
        {
            bulletScene = GD.Load<PackedScene>(WeaponPaths[weaponId]);
        }

        if (bulletScene == null) 
        {
            GD.PrintErr($"Could not load bullet scene for {weaponId}");
            return;
        }

        WeaponSlot newSlot = new WeaponSlot 
        {
            WeaponId = weaponId,
            Stage = 1,
            BulletScene = bulletScene
        };

        // ───────────── FORCE FIELD SPECIAL HANDLING ─────────────
        if (weaponId == "ForceField")
        {
            ForceField field = bulletScene.Instantiate<ForceField>();
            field.Name = "ForceField";
            
            // Add as child directly to this WeaponManager so it follows the player everywhere!
            AddChild(field); 
            field.Position = Vector2.Zero; // Perfectly center on player

            newSlot.FireTimer = null; // No timer needed!
            EquippedWeapons.Add(newSlot);
            GD.Print("ForceField attached directly to player!");
            return;
        }

        // ───────────── STANDARD WEAPON TIMER ─────────────
        Timer t = new Timer();
        AddChild(t);
        newSlot.FireTimer = t;

        float baseInterval = GetIntervalFromBullet(bulletScene);
        t.WaitTime = baseInterval * FireRateMultiplier;
        
        t.Timeout += () => FireWeapon(newSlot);
        t.Start();
        
        EquippedWeapons.Add(newSlot);
        GD.Print($"Equipped {weaponId}. Total weapons: {EquippedWeapons.Count}/{MaxWeapons}");
    }

    public void UpgradeWeaponStage(string weaponId)
    {
        if (TotalUpgradePointsUsed >= MaxUpgradePoints) 
        {
            GD.Print("Max upgrade points reached!");
            return;
        }

        WeaponSlot slot = EquippedWeapons.Find(w => w.WeaponId == weaponId);
        if (slot != null)
        {
            slot.Stage += 1;
            TotalUpgradePointsUsed += 1;
            GD.Print($"Upgraded {weaponId} to Stage {slot.Stage}! Total points used: {TotalUpgradePointsUsed}/{MaxUpgradePoints}");
            
            // If upgrading Force Field, update the active shield properties instantly!
            // If upgrading Force Field, apply the visual and mechanical upgrades instantly!
            if (weaponId == "ForceField")
            {
                ForceField field = GetNodeOrNull<ForceField>("ForceField");
                if (field != null)
                {
                    // This single line triggers the scaling and Stage 3 ring!
                    field.ApplyUpgrades(slot.Stage);
                }
            }
            // Note: If you make a totally new scene for Stage 2 (e.g. force_field_stage2.tscn),
            // you can load and assign it to slot.BulletScene here!
        }
    }

    private float GetIntervalFromBullet(PackedScene scene)
    {
        if (scene == null) return 1.0f;
        Node2D tempBullet = scene.Instantiate<Node2D>();
        float interval = 1.0f;

        if (tempBullet is ARBullet ar) interval = ar.FireRate;
        else if (tempBullet is RocketBullet rocket) interval = rocket.FireRate;
        else if (tempBullet is TeslaBurst tesla) interval = tesla.FireRate; // Fixed: was commented out!
        else if (tempBullet is Mine mine) interval = mine.FireRate; 
        
        // If it's a ForceField, you might not use a script property yet, default to 1s pulses.
        
        tempBullet.QueueFree();
        return interval;
    }

    // --- MULTI-WEAPON FIRE LOGIC ---
    private void FireWeapon(WeaponSlot slot)
    {
        if (slot.BulletScene == null) return;
        
        // Keep fire rate dynamically updated if the player got global FireRate upgrades
        float expectedInterval = GetIntervalFromBullet(slot.BulletScene) * FireRateMultiplier;
        if (Mathf.Abs(slot.FireTimer.WaitTime - expectedInterval) > 0.01f)
        {
            slot.FireTimer.WaitTime = expectedInterval;
        }

        

        // ───────────── STANDARD PROJECTILES ─────────────
        Node2D nearestEnemy = GetNearestEnemy();
        if (nearestEnemy == null && slot.WeaponId != "Mine") return;

        Vector2 baseDir = Vector2.Zero;
        if (nearestEnemy != null)
        {
            baseDir = (nearestEnemy.GlobalPosition - _bulletSpawn.GlobalPosition).Normalized();
        }

        // ───────────── TESLA ─────────────
        if (slot.WeaponId == "Tesla")
        {
            int baseBeams = (slot.Stage >= 3) ? 3 : 1; // Stage 3: 3 side-by-side beams
            int beamCount = baseBeams + (ProjectileCount - 1);
            float beamSpacing = 24f; // pixel offset between beams (side by side, not fanned)
            Vector2 perpendicular = new Vector2(-baseDir.Y, baseDir.X); // 90° rotation

            for (int b = 0; b < beamCount; b++)
            {
                Node2D beamNode = slot.BulletScene.Instantiate<Node2D>();
                GetTree().CurrentScene.AddChild(beamNode);

                // Offset each beam sideways from center
                float offset = (b - (beamCount - 1) / 2.0f) * beamSpacing;
                beamNode.GlobalPosition = _bulletSpawn.GlobalPosition + perpendicular * offset;
                beamNode.Rotation = baseDir.Angle();

                if (beamNode is TeslaBurst tesla)
                {
                    tesla.Damage += BonusDamage;
                    if (slot.Stage >= 2) tesla.ChainCount = 2; 
                    
                    // Force this to false so EVERY beam is allowed to chain!
                    if (slot.Stage >= 3) tesla.IsChainedBeam = false;
                }
            }
            return;
        }

        // ───────────── AR ─────────────
        if (slot.WeaponId == "AR")
        {
            int projectilesToFire = ProjectileCount;

            for (int i = 0; i < projectilesToFire; i++)
            {
                Node2D boltNode = slot.BulletScene.Instantiate<Node2D>();
                GetTree().CurrentScene.AddChild(boltNode);
                boltNode.GlobalPosition = _bulletSpawn.GlobalPosition;

                float angleOffset = 0;
                if (projectilesToFire > 1)
                    angleOffset = SpreadAngle * (i - (projectilesToFire - 1) / 2.0f);

                if (boltNode is ARBullet arBolt)
                {
                    arBolt.Direction = baseDir.Rotated(angleOffset);
                    arBolt.Damage += BonusDamage;
                    if (slot.Stage >= 2) arBolt.MaxPierce = 5;   // Stage 2: pierce 5 enemies
                    if (slot.Stage >= 3) arBolt.CanScatter = true; // Stage 3: scatter on each hit
                }
            }
            return;
        }

        // ───────────── ROCKET ─────────────
        if (slot.WeaponId == "Rocket")
        {
            int projectilesToFire = ProjectileCount;

            for (int i = 0; i < projectilesToFire; i++)
            {
                Node2D boltNode = slot.BulletScene.Instantiate<Node2D>();
                GetTree().CurrentScene.AddChild(boltNode);
                boltNode.GlobalPosition = _bulletSpawn.GlobalPosition;

                float angleOffset = 0;
                if (projectilesToFire > 1)
                    angleOffset = SpreadAngle * (i - (projectilesToFire - 1) / 2.0f);

                if (boltNode is RocketBullet rocket)
                {
                    rocket.Direction = baseDir.Rotated(angleOffset);
                    rocket.Damage += BonusDamage;
                    if (slot.Stage >= 2) rocket.AoeRadiusMultiplier = 1.8f; // Stage 2: bigger AOE
                    if (slot.Stage >= 3) rocket.SuckBeforeExplode = true;   // Stage 3: suck then explode
                }
            }
            return;
        }

        // ───────────── MINE ─────────────
        if (slot.WeaponId == "Mine")
        {
            Node2D boltNode = slot.BulletScene.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(boltNode);
            boltNode.GlobalPosition = _bulletSpawn.GlobalPosition;

            if (boltNode is Mine mine)
            {
                mine.Damage += BonusDamage;
                if (slot.Stage >= 2) mine.DuplicateCount = 4;      // Stage 2: spawn 4 child mines
                if (slot.Stage >= 3) mine.SuckBeforeExplode = true; // Stage 3: suck then explode
            }
            return;
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

    // --- GLOBAL HUD UPGRADES ---
    public void UpgradeFireRate()
    {
        FireRateMultiplier *= 0.80f; // Balanced: reduces interval by 20% (was 30%) — less explosive per upgrade
        GD.Print($"Global Fire Rate Upgraded! Multiplier: {FireRateMultiplier}");
    }

    public void UpgradeProjectile()
    {
        ProjectileCount += 1;
        GD.Print($"Global Projectiles Upgraded! Shooting {ProjectileCount} bullets.");
    }

    public void UpgradeDamage()
    {
        BonusDamage += 15; // Balanced: was +20 — more gradual damage scaling
        GD.Print($"Global Damage Upgraded! Bonus is now +{BonusDamage}");
    }

    public void TriggerLoad()
    {
        if (_saveManager != null)
		{
            SaveData loadedData = _saveManager.LoadGame();
            _weaponPickInt = loadedData._weaponPick;
        }
    }
}