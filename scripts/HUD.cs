using Godot;
using System;
using System.Collections.Generic;

public partial class HUD : CanvasLayer
{
    private bool _isPaused = false;

    private ProgressBar _xpBar;
    private ProgressBar _healthBar;
    private Label _levelLabel;
    private Label _TimeLabel;
    private Label _scrapLabel; // NEW: Added scrap label
    public Timer _Timer;

    private AnimationPlayer _openAnimation;

    private Control _upgradeMenu;
    private Button _leftButton;
    private Button _midButton;
    private Button _rightButton;
    private UpgradeType _leftUpgrade;
    private UpgradeType _midUpgrade;  
    private UpgradeType _rightUpgrade;
    private readonly Random _random = new Random();

    private Movement _player;

    public override void _Ready()
    {
        GD.Print("--- HUD SCRIPT INITIALIZING ---");

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
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            if (_isPaused == false && _openAnimation != null)
            {
                GetTree().Paused = true;
                _isPaused = true;
                _openAnimation.Play("pause_in");
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
        Weapon
    }

    private string FormatUpgradeName(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Damage     => "Damage +20",
            UpgradeType.FireRate   => "Fire Rate +30%",
            UpgradeType.Projectile => "Extra Projectile +1",
            UpgradeType.Health     => "Max Health +25",
            UpgradeType.Speed      => "Speed +35",
            UpgradeType.Weapon     => "Upgrade Weapon",
            _                      => "Unknown Upgrade"
        };
    }

    public void OpenUpgradeMenu() 
    {
        if (_openAnimation != null)
        {
            _openAnimation.Play("upgrade_in");
        }
        
        List<UpgradeType> upgrades = GetRandomUpgrades(3);
        
        if (upgrades.Count >= 3)
        {
            _leftUpgrade = upgrades[0];
            _midUpgrade = upgrades[1];
            _rightUpgrade = upgrades[2];

            if (_leftButton != null && _midButton != null && _rightButton != null)
            {
                _leftButton.Text = FormatUpgradeName(_leftUpgrade);
                _midButton.Text = FormatUpgradeName(_midUpgrade);
                _rightButton.Text = FormatUpgradeName(_rightUpgrade);
            }
        }

        GetTree().Paused = true; 
    }

    private List<UpgradeType> GetRandomUpgrades(int count)
    {
        List<UpgradeType> pool = new List<UpgradeType>((UpgradeType[])Enum.GetValues(typeof(UpgradeType)));
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

    private void ApplyUpgrade(UpgradeType upgrade)
    {
        if (_player == null) return;

        // Try to find the WeaponManager attached to the player (assuming it's a child node)
        WeaponManager weapon = _player.GetNodeOrNull<WeaponManager>("WeaponManager");

        switch (upgrade)
        {
            case UpgradeType.Health:
                _player.UpgradeHealth(25);
                break;
            case UpgradeType.Speed:
                _player.UpgradeSpeed(35);
                break;
            case UpgradeType.Damage:
                if (weapon != null) weapon.UpgradeDamage();
                break;
            case UpgradeType.FireRate:
                if (weapon != null) weapon.UpgradeFireRate();
                break;
            case UpgradeType.Projectile:
                if (weapon != null) weapon.UpgradeProjectile();
                break;
						case UpgradeType.Weapon:
                _player.EvolveWeapon(); 
                break;
        }
    }

    public void CloseUpgradeMenu()
    {
        if (_openAnimation != null) _openAnimation.Play("upgrade_out");
        GetTree().Paused = false; 
        GD.Print("Upgrade menu closed, game resumed.");
    }

    public void OnLeftSelected()
    {
        ApplyUpgrade(_leftUpgrade);
        CloseUpgradeMenu();
    }

    public void OnMiddleSelected()
    {
        ApplyUpgrade(_midUpgrade);
        CloseUpgradeMenu();
    }

    public void OnRightSelected()
    {
        ApplyUpgrade(_rightUpgrade);
        CloseUpgradeMenu();
    }

    // --- UI Update Callbacks ---

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
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
        }
    }
}