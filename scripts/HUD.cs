using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private ProgressBar _xpBar;
    private ProgressBar _healthBar;
    private Label _levelLabel;
    private Label _timerLabel;

    private Control _upgradeMenu;
    private Button _btnSpeed;
    private Button _btnHealth;
    private Button _btnDamage;

    private Label _scrapCollected;

    private Movement _player;

    public override void _Ready()
    {
        // Keeps UI responsive and clickable when GetTree().Paused = true
        ProcessMode = ProcessModeEnum.Always;

        _xpBar = GetNodeOrNull<ProgressBar>("Control/XpBar");
        _healthBar = GetNodeOrNull<ProgressBar>("Control/HealthBar");
        _levelLabel = GetNodeOrNull<Label>("Control/LevelLabel");
        _timerLabel = GetNodeOrNull<Label>("Control/TimerLabel");

        // Bind the scrap label (adjust node path if your Label is located elsewhere under Control)
        _scrapCollected = GetNodeOrNull<Label>("Control/ScrapCollected");

        // Upgrade Menu bindings
        _upgradeMenu = GetNode<Control>("UpgradeMenu");
        _btnSpeed = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnSpeed");
        _btnHealth = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnHealth");
        _btnDamage = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnDamage");

        if (_btnSpeed != null) _btnSpeed.Pressed += OnSpeedSelected;
        if (_btnHealth != null) _btnHealth.Pressed += OnHealthSelected;
        if (_btnDamage != null) _btnDamage.Pressed += OnDamageSelected;

        if (_upgradeMenu != null) _upgradeMenu.Visible = false;
    }

    public void Initialize(Movement player)
    {
        _player = player;
        if (_player != null)
        {
            // Subscribe to the player's scrap collection signal
            _player.ScrapChanged += OnScrapChanged;
            
            // Set initial display value
            OnScrapChanged(_player.CurrentScraps);
        }
    }

    // --- SCRAP HUD UPDATE ---
    private void OnScrapChanged(int currentScraps)
    {
        if (_scrapCollected != null)
        {
            _scrapCollected.Text = $"Scraps: {currentScraps}";
        }
    }

    // --- GAME TIMER ---
    public void UpdateTimer(int minutes, int seconds)
    {
        if (_timerLabel != null)
        {
            _timerLabel.Text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    // --- UPGRADE MENU ---
    public void OpenUpgradeMenu()   
    {
        if (_upgradeMenu != null) _upgradeMenu.Visible = true;
        GetTree().Paused = true; // Freeze game world
    }

    public void CloseUpgradeMenu()
    {
        if (_upgradeMenu != null) _upgradeMenu.Visible = false;
        GetTree().Paused = false; // Unfreeze game world
    }

    public void OnSpeedSelected()
    {
        _player?.UpgradeSpeed(35);
        CloseUpgradeMenu();
    }

    public void OnHealthSelected()
    {
        _player?.UpgradeHealth(25);
        CloseUpgradeMenu();
    }

    public void OnDamageSelected()
    {
        CloseUpgradeMenu();
    }

    // --- STAT BARS ---
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
}