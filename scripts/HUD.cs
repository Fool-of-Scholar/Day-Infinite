using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HUD : CanvasLayer
{
    private EnemySpawner _enemySpawner;

    private int _weaponPickInt;
    private PackedScene[] _weaponPick = new PackedScene[]
    {
        GD.Load<PackedScene>("res://weapon_new/ar/ar_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/rocket/rocket_bullet.tscn"),
        GD.Load<PackedScene>("res://weapon_new/tesla/tesla_burst.tscn"),
        GD.Load<PackedScene>("res://weapon_new/mine/mine.tscn"),
        GD.Load<PackedScene>("res://weapon_new/force field/force_field.tscn"),
    };

    private int[] _statsUpgradeTimes = new int[6] { 0, 0, 0, 0, 0, 0 };
    private int _equippedPassive = 0;

    // Paused labels
    private Label _levelLabelP;
    private Label _healthLabel;
    private Label _damageLabel;
    private Label _firerateLabel;
    private Label _projectileLabel;
    private Label _speedLabel;

    private SaveManager _saveManager;
    private WeaponManager _wCollect;
    private int _localScrap;
    private bool _isPaused = false;
    private int coins = 0;
    private int score = 0;

    private Label ScorePoints;
    private Label ScrapsPoints;
    private Label CoinsPoints;
    private Label TimePoints;

    private ProgressBar _xpBar;
    private ProgressBar _healthBar;
    private Label _levelLabel;
    private Label _TimeLabel;
    private Label _scrapLabel;
    public Timer _Timer;

    private AnimationPlayer _openAnimation;
    private AnimationPlayer _upgradeAnimation;

    private Control _upgradeMenu;
    private Control _diedInterface;
    private Button _pauseButton;
    private Button _dashButton;
    private VirtualJoystick _joystick;
    private Button _leftButton;
    private Button _midButton;
    private Button _rightButton;
    private UpgradeType _leftUpgrade;
    private UpgradeType _midUpgrade;  
    private UpgradeType _rightUpgrade;
    private int MinimumLevel = 3;
    private readonly Random _random = new Random();

    private Movement _player;

    // Coin & Run Balance Tracking
    private float _survivalTime = 0.0f;
    private float _coinMinuteTimer = 0.0f;
    private int _lastProcessedScrap = 0;
    private int _scrapCoinCounter = 0;
    private bool _runSaved = false;
    private bool _isInfiniteMode = false;

    // Achievement Toast UI
    private PanelContainer _toastContainer;
    private Label _toastLabel;
    private float _toastTimer = 0.0f;

    public override void _Ready()
    {
        GD.Print("--- HUD SCRIPT INITIALIZING ---");

        _levelLabelP = GetNodeOrNull<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/Lvl");
        _healthLabel = GetNodeOrNull<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/Health2");
        _damageLabel = GetNodeOrNull<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Damage");
        _firerateLabel = GetNodeOrNull<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Fire Rate");
        _projectileLabel = GetNodeOrNull<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Projectile");
        _speedLabel = GetNodeOrNull<Label>("PauseMenu/Stats/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Weapons");

        ScrapsPoints = GetNodeOrNull<Label>("DiedInterface/PanelContainer/VBoxContainer/Scraps");
        ScorePoints = GetNodeOrNull<Label>("DiedInterface/PanelContainer/VBoxContainer/Label");
        CoinsPoints = GetNodeOrNull<Label>("DiedInterface/PanelContainer/VBoxContainer/Coins");
        TimePoints = GetNodeOrNull<Label>("DiedInterface/PanelContainer/VBoxContainer/Time");

        _saveManager = GetNodeOrNull<SaveManager>("/root/SaveManager");
        _wCollect = GetNodeOrNull<WeaponManager>("../Player/WeaponManager") 
                 ?? (GetTree().GetFirstNodeInGroup("player") as Node)?.GetNodeOrNull<WeaponManager>("WeaponManager");

        // 1. Basic Controls
        _xpBar = GetNodeOrNull<ProgressBar>("Control/XpBar");
        _healthBar = GetNodeOrNull<ProgressBar>("Control/HealthBar");
        _levelLabel = GetNodeOrNull<Label>("Control/LevelLabel");
        _TimeLabel = GetNodeOrNull<Label>("Control/Time");
        _scrapLabel = GetNodeOrNull<Label>("Control/ScrapLabel");
        
        // 2. Parents/Siblings
        _Timer = GetNodeOrNull<Timer>("../Timer");
        _openAnimation = GetNodeOrNull<AnimationPlayer>("../AnimationPlayer");
        _upgradeAnimation = GetNodeOrNull<AnimationPlayer>("../UpgradePopUp") ?? _openAnimation;
        _upgradeMenu = GetNodeOrNull<Control>("UpgradeMenu");
        _diedInterface = GetNodeOrNull<Control>("DiedInterface");

        // 3. Buttons & Joystick
        _leftButton = GetNodeOrNull<Button>("%BtnLeft") ?? 
                    GetNodeOrNull<Button>("UpgradeMenu/PanelContainer/MarginContainer/HBoxContainer/BtnLeft");
                    
        _midButton = GetNodeOrNull<Button>("%BtnMiddle") ?? 
                    GetNodeOrNull<Button>("UpgradeMenu/PanelContainer/MarginContainer/HBoxContainer/BtnMiddle");
                    
        _rightButton = GetNodeOrNull<Button>("%BtnRight") ?? 
                    GetNodeOrNull<Button>("UpgradeMenu/PanelContainer/MarginContainer/HBoxContainer/BtnRight");

        _pauseButton = GetNodeOrNull<Button>("Control/PauseButton");
        if (_pauseButton != null) _pauseButton.Pressed += TogglePause;

        _dashButton = GetNodeOrNull<Button>("Control/DashButton");
        if (_dashButton != null) _dashButton.ButtonDown += OnDashButtonPressed;

        _joystick = GetNodeOrNull<VirtualJoystick>("Control/VirtualJoystick");
        bool isAndroid = OS.GetName() == "Android" || OS.HasFeature("android");
        if (!isAndroid && _joystick != null)
        {
            _joystick.Visible = false;
            _joystick.SetProcess(false);
            _joystick.SetProcessInput(false);
        }

        if (_leftButton != null) _leftButton.Pressed += OnLeftSelected;
        if (_midButton != null) _midButton.Pressed += OnMiddleSelected;
        if (_rightButton != null) _rightButton.Pressed += OnRightSelected;

        if (_upgradeMenu != null) _upgradeMenu.Visible = false;

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
        
        if (_wCollect != null && _weaponPickInt >= 1 && _weaponPickInt <= _weaponPick.Length)
        {
            _wCollect.DefaultBulletScene = _weaponPick[_weaponPickInt - 1];
        }

        // Apply ONLY the single equipped upgrade chosen in the menu
        ApplyEquippedPassive();

        // Apply infinite mode
        if (_isInfiniteMode)
        {
            _Timer?.Stop();
            if (_TimeLabel != null) _TimeLabel.Text = "∞";
        }

        CreateAchievementToastNode();
        
        GD.Print("--- HUD INITIALIZATION COMPLETE ---");
    }

    private void ApplyEquippedPassive()
    {
        int level = (_statsUpgradeTimes != null && _equippedPassive >= 0 && _equippedPassive < _statsUpgradeTimes.Length)
            ? Mathf.Clamp(_statsUpgradeTimes[_equippedPassive], 0, 5)
            : 0;

        string[] passiveNames = { "Speed", "Fire Rate", "Health", "Projectile", "Damage", "Dash" };
        string traitName = _equippedPassive >= 0 && _equippedPassive < passiveNames.Length ? passiveNames[_equippedPassive] : "None";
        GD.Print($"[HUD] Equipping trait: {traitName} (Level {level}/5)");

        if (level <= 0) return;

        switch (_equippedPassive)
        {
            case 0: // Speed (+15 per level)
                _player?.UpgradeSpeed(level * 15);
                break;
            case 1: // Fire Rate (reduces interval by ~10% per level)
                if (_wCollect != null)
                {
                    for (int j = 0; j < level; j++) _wCollect.UpgradeFireRate();
                }
                break;
            case 2: // Health (+25 max health per level)
                _player?.UpgradeHealth(level * 25);
                break;
            case 3: // Projectile (+1 extra bullet every 2 levels)
                if (_wCollect != null)
                {
                    int extra = (level + 1) / 2;
                    for (int j = 0; j < extra; j++) _wCollect.UpgradeProjectile();
                }
                break;
            case 4: // Damage (+8 bonus damage per level)
                if (_wCollect != null)
                {
                    _wCollect.BonusDamage += (level * 8);
                }
                break;
            case 5: // Dash
                _player?.ApplyDashUpgrade(level);
                break;
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _survivalTime += dt;

        // Balanced Coin Accrual: 15 coins per full minute survived
        _coinMinuteTimer += dt;
        if (_coinMinuteTimer >= 60.0f)
        {
            _coinMinuteTimer -= 60.0f;
            coins += 15;
            GD.Print($"[HUD] Survival bonus: +15 coins! Total run coins: {coins}");
        }

        // Timer Display
        if (_isInfiniteMode)
        {
            if (_TimeLabel != null && _TimeLabel.Text != "∞")
            {
                _TimeLabel.Text = "∞";
            }
        }
        else if (_Timer != null && _TimeLabel != null)
        {
            float timeLeft = Mathf.Max(0f, (float)_Timer.TimeLeft);
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            _TimeLabel.Text = $"{minutes:D2}:{seconds:D2}";
        }

        score = (int)_survivalTime * 10 + _localScrap * 5;

        // Check Achievements
        // CheckAchievements();

        // Dash Button Cooldown Visuals
        if (_dashButton != null && _player != null)
        {
            if (_player.DashCooldownRemaining > 0.05f)
            {
                _dashButton.Text = $"{_player.DashCooldownRemaining:F1}";
                _dashButton.Modulate = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            }
            else
            {
                _dashButton.Text = "DASH";
                _dashButton.Modulate = new Color(1f, 1f, 1f, 1f);
            }
        }

        // Achievement Toast Timer
        if (_toastTimer > 0f)
        {
            _toastTimer -= dt;
            if (_toastTimer <= 0f && _toastContainer != null)
            {
                _toastContainer.Visible = false;
            }
        }
    }

    // private void CheckAchievements()
    // {
    //     // 1. Mania: Equip 4 weapons
    //     if (_wCollect != null && _wCollect.EquippedWeapons != null && _wCollect.EquippedWeapons.Count >= 4)
    //     {
    //         CheckAndUnlockAchievement(3, "Mania", "Equipped 4 weapons in one run!");
    //     }

    //     // 2. Area Survival & No Damage Achievements
    //     string scenePath = GetTree().CurrentScene?.SceneFilePath ?? "";
    //     bool isArea2 = scenePath.Contains("area_2.tscn");
    //     bool isArea1 = !isArea2;

    //     if (isArea1 && _survivalTime >= 600.0f) // 10 minutes in Area 1
    //     {
    //         CheckAndUnlockAchievement(0, "Escaped", "Survived 10 minutes in Area 1!");
    //         if (_player != null && !_player.HasTakenDamageThisRun)
    //         {
    //             CheckAndUnlockAchievement(2, "No Damage", "Completed run without taking damage!");
    //         }
    //     }
    //     else if (isArea2 && _survivalTime >= 1200.0f) // 20 minutes in Area 2
    //     {
    //         CheckAndUnlockAchievement(1, "Lost in Wilderness", "Survived 20 minutes in Area 2!");
    //         if (_player != null && !_player.HasTakenDamageThisRun)
    //         {
    //             CheckAndUnlockAchievement(2, "No Damage", "Completed run without taking damage!");
    //         }
    //     }
    // }

    // public void CheckAndUnlockAchievement(int index, string title, string description)
    // {
    //     if (_saveManager != null && _saveManager.UnlockAchievement(index))
    //     {
    //         ShowAchievementToast(title, description);
    //     }
    // }

    private void CreateAchievementToastNode()
    {
        _toastContainer = new PanelContainer();
        _toastContainer.ZIndex = 20;
        _toastContainer.Visible = false;
        _toastContainer.CustomMinimumSize = new Vector2(400, 70);
        _toastContainer.AnchorsPreset = (int)Control.LayoutPreset.CenterTop;
        _toastContainer.AnchorLeft = 0.5f;
        _toastContainer.AnchorRight = 0.5f;
        _toastContainer.OffsetLeft = -200f;
        _toastContainer.OffsetRight = 200f;
        _toastContainer.OffsetTop = 20f;
        _toastContainer.OffsetBottom = 90f;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.08f, 0.12f, 0.18f, 0.95f);
        style.BorderWidthLeft = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = new Color(1.0f, 0.85f, 0.2f, 1.0f); // Radiant gold glow
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.ShadowColor = new Color(1.0f, 0.8f, 0.1f, 0.6f);
        style.ShadowSize = 14;
        _toastContainer.AddThemeStyleboxOverride("panel", style);

        _toastLabel = new Label();
        _toastLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _toastLabel.VerticalAlignment = VerticalAlignment.Center;
        _toastLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _toastLabel.Text = "ACHIEVEMENT UNLOCKED!";
        _toastLabel.Modulate = new Color(1.0f, 0.95f, 0.4f, 1.0f);

        _toastContainer.AddChild(_toastLabel);
        AddChild(_toastContainer);
    }

    public void ShowAchievementToast(string title, string description)
    {
        if (_toastContainer != null && _toastLabel != null)
        {
            _toastLabel.Text = $"★ ACHIEVEMENT UNLOCKED! ★\n{title}: {description}";
            _toastContainer.Visible = true;
            _toastTimer = 4.0f;
        }
        GD.Print($"[HUD] Toast: Achievement Unlocked: {title} - {description}");
    }

    private void OnDashButtonPressed()
    {
        _player?.TriggerDash();
    }

    public void Initialize(Movement player)
    {
        _player = player;

        _player.HealthChanged += UpdateHealth;
        _player.XpChanged += UpdateXp;
        _player.ScrapChanged += UpdateScrap;
        _player.LeveledUp += OpenUpgradeMenu;
        _player.PlayerDied += OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        SaveRunResults();

        if (_openAnimation != null)
        {
            _openAnimation.Play("died");
        }

        if (_diedInterface != null)
        {
            _diedInterface.Visible = true;
            if (CoinsPoints != null) CoinsPoints.Text = $"Coins: {coins}";
            if (ScorePoints != null) ScorePoints.Text = $"Score: {score}";
            if (ScrapsPoints != null) ScrapsPoints.Text = $"Scraps: {_localScrap}";

            int minutes = Mathf.FloorToInt(_survivalTime / 60);
            int seconds = Mathf.FloorToInt(_survivalTime % 60);
            string timeStr = $"{minutes:D2}:{seconds:D2}";
            if (TimePoints != null)
            {
                TimePoints.Text = $"Time Survived: {timeStr}";
            }
            else
            {
                // Fallback dynamically create label if not in scene tree
                var vbox = _diedInterface.GetNodeOrNull<VBoxContainer>("PanelContainer/VBoxContainer");
                if (vbox != null)
                {
                    TimePoints = new Label();
                    TimePoints.Name = "Time";
                    TimePoints.HorizontalAlignment = HorizontalAlignment.Center;
                    if (ScorePoints != null) TimePoints.LabelSettings = ScorePoints.LabelSettings;
                    TimePoints.Text = $"Time Survived: {timeStr}";
                    vbox.AddChild(TimePoints);
                    vbox.MoveChild(TimePoints, 1);
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!_isPaused && _openAnimation != null)
        {
            GetTree().Paused = true;
            _isPaused = true;
            _openAnimation.Play("pause_in");

            string[] passiveNames = { "Speed", "Fire Rate", "Health", "Projectile", "Damage", "Dash" };
            int activeLvl = (_statsUpgradeTimes != null && _equippedPassive >= 0 && _equippedPassive < _statsUpgradeTimes.Length) 
                ? _statsUpgradeTimes[_equippedPassive] 
                : 0;
            string traitText = $"{passiveNames[_equippedPassive]} Lvl {activeLvl}/5";

            if (_levelLabelP != null && _player != null) _levelLabelP.Text = $"Level: {_player.CurrentLevel}";
            if (_healthLabel != null && _player != null) _healthLabel.Text = $"Health: {_player.CurrentHealth}/{_player.MaxHealth}";
            if (_damageLabel != null && _wCollect != null) _damageLabel.Text = $"Dmg Bonus: +{_wCollect.BonusDamage}";
            if (_firerateLabel != null && _wCollect != null) _firerateLabel.Text = $"FireRate: {_wCollect.FireRateMultiplier:F2}x";
            if (_projectileLabel != null && _wCollect != null) _projectileLabel.Text = $"Proj: {_wCollect.ProjectileCount}";
            if (_speedLabel != null) _speedLabel.Text = $"Trait: {traitText}";
        }
        else if (_isPaused && _openAnimation != null)
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        if (_isPaused && _openAnimation != null)
        {
            _isPaused = false;
            _openAnimation.Play("pause_out");
            GetTree().Paused = false;
        }
    }

    public enum UpgradeType
    {
        Speed,
        Health,
        Damage,
        FireRate, 
        Projectile,
        
        MenuAddNewWeapon,
        MenuUpgradeWeapon,

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
        var bannerPanel = GetNodeOrNull<Control>("Panel") ?? GetNodeOrNull<Control>("../Hud/Panel");
        if (bannerPanel != null)
        {
            bannerPanel.Visible = false;
        }

        if (_upgradeAnimation != null && _upgradeAnimation.HasAnimation("upgrade_in"))
        {
            _upgradeAnimation.Play("upgrade_in");
        }
        else if (_upgradeMenu != null)
        {
            _upgradeMenu.Visible = true;
        }
        
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
                _leftUpgrade = UpgradeType.MenuAddNewWeapon;
                _midUpgrade = UpgradeType.MenuUpgradeWeapon;
                
                _leftButton.Text = FormatUpgradeName(_leftUpgrade);
                _midButton.Text = FormatUpgradeName(_midUpgrade);
                _rightButton.Visible = false;
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
        List<UpgradeType> possible = new List<UpgradeType> { 
            UpgradeType.WeaponAR, UpgradeType.WeaponMine, UpgradeType.WeaponRocket, 
            UpgradeType.WeaponForceField, UpgradeType.WeaponTesla 
        };
        
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

    private bool ApplyUpgrade(UpgradeType upgrade)
    {
        if (_player == null) return true;

        WeaponManager weapon = _player.GetNodeOrNull<WeaponManager>("WeaponManager");

        if (upgrade == UpgradeType.MenuAddNewWeapon)
        {
            OpenSubMenuAddWeapon();
            return false;
        }
        if (upgrade == UpgradeType.MenuUpgradeWeapon)
        {
            OpenSubMenuUpgradeWeapon();
            return false;
        }

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
            
            MinimumLevel += 4;
            return true; 
        }

        // In-run Level-up Stat Upgrades
        switch (upgrade)
        {
            case UpgradeType.Health:
                _player.UpgradeHealth(30);
                break;
            case UpgradeType.Speed:
                _player.UpgradeSpeed(25);
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
        }

        return true;
    }

    public void CloseUpgradeMenu()
    {
        if (_upgradeAnimation != null && _upgradeAnimation.HasAnimation("upgrade_out"))
        {
            _upgradeAnimation.Play("upgrade_out");
        }
        if (_upgradeMenu != null)
        {
            _upgradeMenu.Visible = false;
        }
        GetTree().Paused = false; 
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

    public void UpdateScrap(int currentScraps)
    {
        if (_scrapLabel != null)
        {
            _scrapLabel.Text = $"Scrap: {currentScraps}";
        }
        _localScrap = currentScraps;
        if (ScrapsPoints != null)
        {
            ScrapsPoints.Text = $"Scrap: {currentScraps}";
        }

        // Balanced Scraps-to-Coin conversion: 1 coin per 2 scraps picked up
        int diff = currentScraps - _lastProcessedScrap;
        if (diff > 0)
        {
            _scrapCoinCounter += diff;
            while (_scrapCoinCounter >= 2)
            {
                _scrapCoinCounter -= 2;
                coins += 1;
            }
            _lastProcessedScrap = currentScraps;
        }
    }

    public void SaveRunResults()
    {
        if (_runSaved) return;
        _runSaved = true;

        // // Check No Damage achievement on end of run if survived at least 3 minutes
        // if (_survivalTime >= 180.0f && _player != null && !_player.HasTakenDamageThisRun)
        // {
        //     CheckAndUnlockAchievement(2, "No Damage", "Survived run without taking damage!");
        // }

        if (_saveManager != null)
        {
            _saveManager.SaveGame(_localScrap, coins);
            GD.Print($"[HUD] Run results saved: +{_localScrap} scraps, +{coins} coins.");
        }
    }

    public void TriggerLoad()
    {
        if (_saveManager != null)
        {
            SaveData loadedData = _saveManager.LoadGame();
            _statsUpgradeTimes = loadedData._statsRepeatUpgrades.ToArray();
            _equippedPassive = loadedData._equippedPassive;
            _weaponPickInt = loadedData._weaponPick;
            _isInfiniteMode = loadedData._infiniteMode;
        }
    }
}
