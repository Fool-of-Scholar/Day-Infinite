using Godot;
using System;

public partial class MainMenu : Control
{
	AnimationPlayer _animPlayer;
	Button _btnPlay;
	private Label _totalScrapsLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_btnPlay = GetNode<Button>("VBoxContainer/BtnPlay");
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

	private void OnPlayPressed()
	{
		QueueFree(); // Remove the current scene from the tree
	}
	private void OnStoredScrapsChanged(int newScrapValue)
	{
		_totalScrapsLabel.Text = $"Total Scraps: {newScrapValue}";
	}

  public override void _ExitTree()
  {
    if (GameManager.Instance != null)
		{
			GameManager.Instance.StoredScrapsChanged -= OnStoredScrapsChanged;
		}
  }

}
