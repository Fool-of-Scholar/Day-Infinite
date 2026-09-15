using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private ProgressBar _xpBar;
	private ProgressBar _healthBar;
	private Label _levelLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_xpBar = GetNode<ProgressBar>("Control/XpBar");
		_healthBar = GetNode<ProgressBar>("Control/HealthBar");
		_levelLabel = GetNode<Label>("Control/LevelLabel");
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
