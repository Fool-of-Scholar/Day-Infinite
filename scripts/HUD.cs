using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public partial class HUD : CanvasLayer
{
    EnemySpawner _enemySpawner;

    private int _weaponPickInt;
    private PackedScene[] _weaponPick = new PackedScene[]
    {
        GD.Load<PackedScene>("res://weapon_new/ar/ar_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/rocket/rocket_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/rocket/rocket_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/mine/mine.tscn"),
        GD.Load<PackedScene>("res://weapon_new/force field/force_field.tscn"),
    };

    private int[] _statsUpgradeTimes = new int[]
    {
        0,
        0,
        0,
        0,
        0
    };
    //paused label
    Label _levelLabelP;
    Label _healthLabel;
    Label _damageLabel;
    Label _firerateLabel;
    Label _projectileLabel;
    Label _speedLabel;

    private SaveManager _saveManager;
    private WeaponManager _wCollect;
    private int _localScrap;
    private bool _isPaused = false;
    private int coins;
    private int score;

    private Label ScorePoints;
    private Label ScrapsPoints;
    private Label CoinsPoints;

    private ProgressBar _xpBar;
    private ProgressBar _healthBar;
    private Label _levelLabel;
    private Label _TimeLabel;
    private Label _scrapLabel; // NEW: Added scrap label
    public Timer _Timer;

    private AnimationPlayer _openAnimation;

    private Control _upgradeMenu;
    private Control _diedInterface;
    private Button _leftButton;
    private Button _midButton;
    private Button _rightButton;
    private UpgradeType _leftUpgrade;
    private UpgradeType _midUpgrade;  
    private UpgradeType _rightUpgrade;
    private int MinimumLevel = 3;  // Balanced: first weapon choice at level 3 (was 5 — too long to wait)
    private readonly Random _random = new Random();

    private Movement _player;

    public override void _Ready()
    {
        GD.Print("--- HUD SCRIPT INITIALIZING ---");
        _levelLabelP = GetNode<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/Lvl");
        _healthLabel = GetNode<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/Health2");
        _damageLabel = GetNode<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Damage");
        _firerateLabel = GetNode<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Fire Rate");
        _projectileLabel = GetNode<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Projectile");
        _speedLabel = GetNode<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Weapons");

        ScrapsPoints = GetNode<Label>("DiedInterface/PanelContainer/VBoxContainer/Scraps");
        ScorePoints = GetNode<Label>("DiedInterface/PanelContainer/VBoxContainer/Label");
        CoinsPoints = GetNode<Label>("DiedInterface/PanelContainer/VBoxContainer/Coins");

            _saveManager = GetNode<SaveManager>("/root/SaveManager");
        _wCollect = GetNode<WeaponManager>("Player/WeaponManager");

        // 1. Basic Controls
        _xpBar = GetNodeOrNull<ProgressBar>("Control/XpBar");
        _healthBar = GetNodeOrNull<ProgressBar>("Control/HealthBar");
        _levelLabel = GetNodeOrNull<Label>("Control/LevelLabel");
        _TimeLabel = GetNodeOrNull<Label>("Control/Time");
        _scrapLabel = GetNodeOrNull<Label>("Control/ScrapLabel"); // Ensure you have this node in your scene
        
        // 2. Parents/Siblings
        _Timer = GetNodeOrNull<Timer>("../Timer");
        _openAnimation = GetNodeOrNull<AnimationPlayer>("../AnimationPlayer");
        _upgradeMenu = GetNodeOrNull<Control>("UpgradeMenu");
        _diedInterface = GetNodeOrNull<Control>("DiedInterface");

        // 3. Buttons (Checking multiple fallback methods automatically)
        _leftButton = GetNodeOrNull<Button>("%BtnLeft") ?? 
                    GetNodeOrNull<Button>("UpgradeMenu/PanelContainer/MarginContainer/HBoxContainer/BtnLeft");
                    
        _midButton = GetNodeOrNull<Button>("%BtnMiddle") ?? 
                    GetNodeOrNull<Button>("UpgradeMenu/PanelContainer/MarginContainer/HBoxContainer/BtnMiddle");
                    
        _rightButton = GetNodeOrNull<Button>("%BtnRight") ?? 
                    GetNodeOrNull<Button>("UpgradeMenu/PanelContainer/MarginContainer/HBoxContainer/BtnRight");

        // 4. Connect Events safely
        if (_leftButton != null) _leftButton.Pressed += OnLeftSelected;
        if (_midButton != null) _midButton.Pressed += OnMiddleSelected;
        if (_rightButton != null) _rightButton.Pressed += OnRightSelected;

        if (_upgradeMenu != null) _upgradeMenu.Visible = false;

        // Use lowercase "player" or look for sibling "../Player"
    _player = (GetTree().GetFirstNodeInGroup("player") as Movement) 
            ?? GetNodeOrNull<Movement>("../Player");

    if (_player != null)
    {
        Initialize(_player);
        UpdateHealth(_player.CurrentHealth, _player.MaxHealth);
        UpdateXp(_player.CurrentXp, _player.XpToNextLevel, _player.CurrentLevel);
        UpdateScrap(_player.CurrentScraps);
    }

    TriggerLoad();
    
    _wCollect.DefaultBulletScene = _weaponPick[_weaponPickInt - 1];


        for(int i = 0; i < _statsUpgradeTimes.Length; i++)
        {
            GD.Print("Checking Stats");
            for(int j = 0; j < _statsUpgradeTimes[i]; j++)
            {
                GD.Print("Pairing Stats");
                switch (i)
                    {
                        case 0:
                            _player.UpgradeHealth(30); 
                            GD.Print("Upgrade Health");
                            break;
                        case 1:
                            _player.UpgradeSpeed(25);   
                            GD.Print("Upgrade Speed");
                            break;
                        case 2:
                            _wCollect.UpgradeDamage();
                            GD.Print("Upgrade Damage");
                            break;
                        case 3:
                            _wCollect.UpgradeFireRate();
                            GD.Print("Upgrade FireRate");
                            break;
                        case 4:
                            _wCollect.UpgradeProjectile();
                            GD.Print("Upgrade Projectile");
                            break;
                    }
            }
        }
        
        GD.Print("--- HUD INITIALIZATION COMPLETE ---");
    }

    public override void _Process(double delta)
    {
        if (_Timer != null && _TimeLabel != null)
        {
            float timeLeft = (float)_Timer.TimeLeft;
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            _TimeLabel.Text = $"{minutes:D2}:{seconds:D2}";
            if (seconds >= 58)
            {
                coins += minutes;
                score += seconds;
                
            }
            
        }
    }

    // NEW: The game manager or player should call this and pass the player reference
    public void Initialize(Movement player)
    {
        _player = player;

        // Automatically connect all player signals to the HUD
        _player.HealthChanged += UpdateHealth;
        _player.XpChanged += UpdateXp;
        _player.ScrapChanged += UpdateScrap;
        _player.LeveledUp += OpenUpgradeMenu; // Automatically open menu when player levels up!
        _player.PlayerDied += OnPlayerDied;   // Show death screen when player dies
    }

    private void OnPlayerDied()
    {
        TriggerSave();
        // Play the drop-in animation for the death screen
        if (_openAnimation != null)
        {
            _openAnimation.Play("died");
        }

        // Make DiedInterface visible so it catches input when the animation finishes
        if (_diedInterface != null)
        {
            _diedInterface.Visible = true;
            CoinsPoints.Text = $"Coins: {coins}";
            ScorePoints.Text = $"Score: {score}";
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            if (_isPaused == false && _openAnimation != null)
            {
                TriggerSave();
                GetTree().Paused = true;
                _isPaused = true;
                _openAnimation.Play("pause_in");

                    _levelLabelP.Text = $"Level: {_player.CurrentLevel}";
                    _healthLabel.Text = $"Health: {_player.CurrentHealth}/{_player.MaxHealth}";
                    _damageLabel.Text = $"Damage: {_statsUpgradeTimes[4]}";
                    _firerateLabel.Text = $"Fire Rate: {_statsUpgradeTimes[1]}";
                    _projectileLabel.Text = $"Projectile: {_statsUpgradeTimes[3]}";
                    _speedLabel.Text = $"Speed: {_statsUpgradeTimes[2]}";
            }
            else if (_isPaused && _openAnimation != null)
            {
                _isPaused = false;
                _openAnimation.Play("pause_out");
                GetTree().Paused = false; 
            }
        }
    }

    // Added FireRate and Projectile to match your WeaponManager!
    public enum UpgradeType
    {
        Speed,
        Health,
        Damage,
        FireRate, 
        Projectile,
        
        // Menu Navigation
        MenuAddNewWeapon,
        MenuUpgradeWeapon,

        // Weapons
        WeaponAR,
        WeaponMine,
        WeaponRocket,
        WeaponForceField,
        WeaponTesla
    }

    private string FormatUpgradeName(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Damage     => "Damage +15",
            UpgradeType.FireRate   => "Fire Rate +20%",
            UpgradeType.Projectile => "Extra Projectile +1",
            UpgradeType.Health     => "Max Health +30",
            UpgradeType.Speed      => "Speed +25",
            UpgradeType.MenuAddNewWeapon  => "Add Weapon [+]",
            UpgradeType.MenuUpgradeWeapon => "Upgrade Weapon [^]",
            UpgradeType.WeaponAR         => "Assault Rifle [Rapid]",
            UpgradeType.WeaponMine       => "Landmine [Trap]",
            UpgradeType.WeaponRocket     => "Rocket [AoE]",
            UpgradeType.WeaponForceField => "Force Field",
            UpgradeType.WeaponTesla => "Tesla",
            _                      => "Unknown"
        };
    }

    public void OpenUpgradeMenu() 
    {
        if (_openAnimation != null && _openAnimation.HasAnimation("upgrade_in"))
        {
            _openAnimation.Play("upgrade_in");
        }
        else if (_upgradeMenu != null)
        {
            _upgradeMenu.Visible = true;
        }
        
        // Make sure buttons are visible in case they were hidden by the sub-menu previously
        if (_leftButton != null) _leftButton.Visible = true;
        if (_midButton != null) _midButton.Visible = true;
        if (_rightButton != null) _rightButton.Visible = true;

        if (_player != null && _player.CurrentLevel == MinimumLevel)
        {
            WeaponManager weapon = _player.GetNodeOrNull<WeaponManager>("WeaponManager");
            bool canAdd = weapon != null && weapon.EquippedWeapons.Count < WeaponManager.MaxWeapons;
            bool canUpgrade = weapon != null && weapon.TotalUpgradePointsUsed < WeaponManager.MaxUpgradePoints;

            if (canAdd && canUpgrade)
            {
                // Show the 2-choice menu
                _leftUpgrade = UpgradeType.MenuAddNewWeapon;
                _midUpgrade = UpgradeType.MenuUpgradeWeapon;
                
                _leftButton.Text = FormatUpgradeName(_leftUpgrade);
                _midButton.Text = FormatUpgradeName(_midUpgrade);
                _rightButton.Visible = false; // Hide 3rd button for this step
            }
            else if (canAdd)
            {
                OpenSubMenuAddWeapon();
            }
            else if (canUpgrade)
            {
                OpenSubMenuUpgradeWeapon();
            }
            else
            {
                // Fully maxed out on weapons/upgrades! Give minor stats instead
                SetButtonsToMinorStats();
            }
        }
        else
        {
            SetButtonsToMinorStats();
        }

        GetTree().Paused = true; 
    }

    private void SetButtonsToMinorStats()
    {
        List<UpgradeType> upgrades = GetRandomMinorUpgrades(3);
        SetButtonUpgrades(upgrades);
    }

    private void OpenSubMenuAddWeapon()
    {
        if (_rightButton != null) _rightButton.Visible = true;

        WeaponManager weapon = _player.GetNodeOrNull<WeaponManager>("WeaponManager");
        List<UpgradeType> possible = new List<UpgradeType> { UpgradeType.WeaponAR, 
        UpgradeType.WeaponMine, UpgradeType.WeaponRocket, UpgradeType.WeaponForceField, 
        UpgradeType.WeaponTesla};
        
        // Remove weapons we already have equipped
        if (weapon != null)
        {
            foreach (var w in weapon.EquippedWeapons)
            {
                if (w.WeaponId == "AR") possible.Remove(UpgradeType.WeaponAR);
                if (w.WeaponId == "Mine") possible.Remove(UpgradeType.WeaponMine);
                if (w.WeaponId == "Rocket") possible.Remove(UpgradeType.WeaponRocket);
                if (w.WeaponId == "Tesla") possible.Remove(UpgradeType.WeaponTesla);
                if (w.WeaponId == "ForceField") possible.Remove(UpgradeType.WeaponForceField);
            }
        }

        // Pick up to 3 random unequipped weapons
        List<UpgradeType> choices = new List<UpgradeType>();
        for (int i = 0; i < 3; i++)
        {
            if (possible.Count == 0) break;
            int r = _random.Next(possible.Count);
            choices.Add(possible[r]);
            possible.RemoveAt(r);
        }

        SetButtonUpgrades(choices);
    }

    private void OpenSubMenuUpgradeWeapon()
    {
        if (_rightButton != null) _rightButton.Visible = true;

        WeaponManager weapon = _player.GetNodeOrNull<WeaponManager>("WeaponManager");
        List<UpgradeType> choices = new List<UpgradeType>();

        // Only show options for weapons we currently have equipped
        if (weapon != null)
        {
            foreach (var w in weapon.EquippedWeapons)
            {
                if (w.WeaponId == "AR") choices.Add(UpgradeType.WeaponAR);
                if (w.WeaponId == "Mine") choices.Add(UpgradeType.WeaponMine);
                if (w.WeaponId == "Rocket") choices.Add(UpgradeType.WeaponRocket);
                if (w.WeaponId == "Tesla") choices.Add(UpgradeType.WeaponTesla);
                if (w.WeaponId == "ForceField") choices.Add(UpgradeType.WeaponForceField);
            }
        }

        SetButtonUpgrades(choices);
    }

    private void SetButtonUpgrades(List<UpgradeType> upgrades)
    {
        if (_leftButton != null) _leftButton.Visible = false;
        if (_midButton != null) _midButton.Visible = false;
        if (_rightButton != null) _rightButton.Visible = false;

        if (upgrades.Count > 0)
        {
            _leftUpgrade = upgrades[0];
            if (_leftButton != null) { _leftButton.Text = FormatUpgradeName(_leftUpgrade); _leftButton.Visible = true; }
        }
        if (upgrades.Count > 1)
        {
            _midUpgrade = upgrades[1];
            if (_midButton != null) { _midButton.Text = FormatUpgradeName(_midUpgrade); _midButton.Visible = true; }
        }
        if (upgrades.Count > 2)
        {
            _rightUpgrade = upgrades[2];
            if (_rightButton != null) { _rightButton.Text = FormatUpgradeName(_rightUpgrade); _rightButton.Visible = true; }
        }
    }

    private List<UpgradeType> GetRandomMinorUpgrades(int count)
    {
        List<UpgradeType> pool = new List<UpgradeType> 
        { 
            UpgradeType.Speed, UpgradeType.Health, UpgradeType.Damage, UpgradeType.FireRate, UpgradeType.Projectile 
        };
        
        List<UpgradeType> selected = new List<UpgradeType>();

        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) break;
            
            int randomIndex = _random.Next(pool.Count);
            selected.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex); 
        }

        return selected;
    }

    // Returns true if the menu should close, false if we are just navigating a sub-menu
    private bool ApplyUpgrade(UpgradeType upgrade)
    {
        if (_player == null) return true;

        WeaponManager weapon = _player.GetNodeOrNull<WeaponManager>("WeaponManager");

        // Sub-menu navigation checks
        if (upgrade == UpgradeType.MenuAddNewWeapon)
        {
            OpenSubMenuAddWeapon();
            return false; // Keep menu open
        }
        if (upgrade == UpgradeType.MenuUpgradeWeapon)
        {
            OpenSubMenuUpgradeWeapon();
            return false; // Keep menu open
        }

        // Weapon Selection Logic (either Adding or Upgrading)
        if (upgrade == UpgradeType.WeaponAR || upgrade == UpgradeType.WeaponMine || 
            upgrade == UpgradeType.WeaponRocket || upgrade == UpgradeType.WeaponForceField || upgrade == UpgradeType.WeaponTesla)
        {
            if (weapon != null)
            {
                string id = "";
                if (upgrade == UpgradeType.WeaponAR) id = "AR";
                if (upgrade == UpgradeType.WeaponMine) id = "Mine";
                if (upgrade == UpgradeType.WeaponRocket) id = "Rocket";
                if (upgrade == UpgradeType.WeaponTesla) id = "Tesla";
                if (upgrade == UpgradeType.WeaponForceField) id = "ForceField";

                // Check if they already have it
                bool hasWeapon = false;
                foreach(var w in weapon.EquippedWeapons)
                {
                    if (w.WeaponId == id) hasWeapon = true;
                }

                if (hasWeapon)
                {
                    weapon.UpgradeWeaponStage(id);
                }
                else
                {
                    weapon.EquipNewWeapon(id);
                }
            }
            
            MinimumLevel += 4; // Balanced: was +5 — more frequent weapon/upgrade decisions during 10 min
            return true; 
        }

        // Minor upgrades
        switch (upgrade)
        {
            case UpgradeType.Health:
                _player.UpgradeHealth(30);  _statsUpgradeTimes[2] += 1; // Balanced: was +25
                break;
            case UpgradeType.Speed:
                _player.UpgradeSpeed(25);  _statsUpgradeTimes[0] += 1; // Balanced: was +35 (too large per step)
                break;
            case UpgradeType.Damage:
                if (weapon != null) weapon.UpgradeDamage(); _statsUpgradeTimes[4] += 1;
                break;
            case UpgradeType.FireRate:
                if (weapon != null) weapon.UpgradeFireRate(); _statsUpgradeTimes[1] += 1;
                break;
            case UpgradeType.Projectile:
                if (weapon != null) weapon.UpgradeProjectile(); _statsUpgradeTimes[3] += 1;
                break;
        }

        return true; // Close menu after stat upgrade
    }

    public void CloseUpgradeMenu()
    {
        if (_openAnimation != null) _openAnimation.Play("upgrade_out");
        GetTree().Paused = false; 
        GD.Print("Upgrade menu closed, game resumed.");
    }

    public void OnLeftSelected()
    {
        if (ApplyUpgrade(_leftUpgrade)) CloseUpgradeMenu();
    }

    public void OnMiddleSelected()
    {
        if (ApplyUpgrade(_midUpgrade)) CloseUpgradeMenu();
    }

    public void OnRightSelected()
    {
        if (ApplyUpgrade(_rightUpgrade)) CloseUpgradeMenu();
    }

    // --- UI Update Callbacks ---

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (_healthBar.Value == 0)
        {
            
        }
        
        if (_healthBar == null) return;
        _healthBar.MaxValue = maxHealth;
        _healthBar.Value = currentHealth;
    }

    public void UpdateXp(int currentXp, int xpToNextLevel, int currentLevel)
    {
        if (_xpBar == null || _levelLabel == null) return;
        _xpBar.MaxValue = xpToNextLevel;
        _xpBar.Value = currentXp;
        _levelLabel.Text = $"LV. {currentLevel}"; 
    }

    // NEW: Updates the Scrap UI
    public void UpdateScrap(int currentScraps)
    {
        if (_scrapLabel != null)
        {
            _scrapLabel.Text = $"Scrap: {currentScraps}";
            
            _localScrap = currentScraps;
            ScrapsPoints.Text = $"Scrap: {currentScraps}";
        }
    }

    public void TriggerSave()
    {
        if (_saveManager != null)
        {
            _saveManager.SaveGame(_localScrap, coins);
            GD.Print("Scraps saved!");
        }
    }

    public void TriggerLoad()
    {
        if (_saveManager != null)
		{
            SaveData loadedData = _saveManager.LoadGame();
		    _statsUpgradeTimes = loadedData._statsRepeatUpgrades.ToArray();
            _weaponPickInt = loadedData._weaponPick;
        }
    }
}