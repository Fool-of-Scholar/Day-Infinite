using Godot;
using System;

public partial class MainMenu : Control
{
	AnimationPlayer _animPlayer;

	private Label _totalScrapsLabel;
	Button _btnPlay;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_btnPlay = GetNode<Button>("MainMenu/PanelContainer/MarginContainer/VBoxContainer/Play");
		_btnPlay.Pressed += OnPlayPressed;
		_totalScrapsLabel = GetNode<Label>("MainMenu/PanelContainer/MarginContainer/VBoxContainer/TotalScrapsLabel");
		// 1. Display current stored scraps immediately on load
        if (GameManager.Instance != null)
        {
            _totalScrapsLabel.Text = $"Total Scraps: {GameManager.Instance.StoredScraps}";

            // 2. Listen to changes if scraps are updated globally
            GameManager.Instance.StoredScrapsChanged += OnStoredScrapsChanged;
        }
	}

	private void OnStoredScrapsChanged(int newTotal)
	{
		_totalScrapsLabel.Text = $"Total Scraps: {newTotal}";
	}

	private void OnPlayPressed()
	{
		QueueFree(); // Remove the current scene from the tree
	}
}
