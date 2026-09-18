using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class HUD : CanvasLayer
{
	private ProgressBar _xpBar;
	private ProgressBar _healthBar;
	private Label _levelLabel;

	private Control _upgradeMenu;
	private Button _btnSpeed;
	private Button _btnHealth;
	private Button _btnDamage;

	private Movement _player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_xpBar = GetNode<ProgressBar>("Control/XpBar");
		_healthBar = GetNode<ProgressBar>("Control/HealthBar");
		_levelLabel = GetNode<Label>("Control/LevelLabel");

		// Upgrade Var
		_upgradeMenu = GetNode<Control>("UpgradeMenu");
		_btnSpeed = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnSpeed");
		_btnHealth = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnHealth");
		_btnDamage = GetNode<Button>("UpgradeMenu/VBoxContainer/BtnDamage");

		_btnSpeed.Pressed += OnSpeedSelected;
		_btnHealth.Pressed += OnHealthSelected;
		_btnDamage.Pressed += OnDamageSelected;

		_upgradeMenu.Visible = false;

	}
	public void Initialize (Movement player)
	{
		_player = player;
	}

	public void OpenUpgradeMenu()	
	{
		_upgradeMenu.Visible = true;
		GetTree().Paused = true; // Freeze the world
	}

	public void CloseUpgradeMenu ()
	{
		_upgradeMenu.Visible = false;
		GetTree().Paused = false; // Resume game loop
	}

	public void OnSpeedSelected ()
	{
		_player?.UpgradeSpeed(35);
		CloseUpgradeMenu();
	}

	public void OnHealthSelected ()
	{
		_player?.UpgradeHealth(25);
		CloseUpgradeMenu();
	}

	public void OnDamageSelected()
	{
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
