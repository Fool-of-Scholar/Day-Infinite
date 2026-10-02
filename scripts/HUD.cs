using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private ProgressBar _xpBar;
    private ProgressBar _healthBar;
    private Label _levelLabel;

    private Control _upgradeMenu;
    private Button _btnFireRate;
    private Button _btnProjectile;
    private Button _btnDamage;

    private Player _player;

    public override void _Ready()
    {
        _xpBar = GetNode<ProgressBar>("Control/XpBar");
        _healthBar = GetNode<ProgressBar>("Control/HealthBar");
        _levelLabel = GetNode<Label>("Control/LevelLabel");

        // Look for the newly renamed buttons
        _upgradeMenu = GetNode<Control>("UpgradeMenu");
        _btnFireRate = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnFireRate");
        _btnProjectile = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnProjectile");
        _btnDamage = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnDamage");

        _btnFireRate.Pressed += OnFireRateSelected;
        _btnProjectile.Pressed += OnProjectileSelected;
        _btnDamage.Pressed += OnDamageSelected;

        _upgradeMenu.Visible = false;
    }

    public void Initialize (Player player)
    {
        _player = player;
    }

    public void OpenUpgradeMenu()	
    {
        _upgradeMenu.Visible = true;
        GetTree().Paused = true; 
    }

    public void CloseUpgradeMenu ()
    {
        _upgradeMenu.Visible = false;
        GetTree().Paused = false; 
    }

    public void OnFireRateSelected ()
    {
        if (_player != null) {
            foreach (Node child in _player.GetChildren()) {
                if (child is WeaponManager weapon) weapon.UpgradeFireRate();
            }
        }
        CloseUpgradeMenu();
    }

    public void OnProjectileSelected ()
    {
        if (_player != null) {
            foreach (Node child in _player.GetChildren()) {
                if (child is WeaponManager weapon) weapon.UpgradeProjectile();
            }
        }
        CloseUpgradeMenu();
    }

    public void OnDamageSelected()
    {
        if (_player != null) {
            foreach (Node child in _player.GetChildren()) {
                if (child is WeaponManager weapon) weapon.UpgradeDamage();
            }
        }
        CloseUpgradeMenu();
    }

    public void UpdateHealth (int currentHealth, int maxHealth)
    {
        if (_healthBar == null) return;
        _healthBar.MaxValue = maxHealth;
        _healthBar.Value = currentHealth;
    }

    public void UpdateXp (int currentXp, int xpToNextLevel, int currentLevel)
    {
        if (_xpBar == null || _levelLabel == null) return;
        _xpBar.MaxValue = xpToNextLevel;
        _xpBar.Value = currentXp;
        _levelLabel.Text = $"LV. {currentLevel}";	
    }
}