using Godot;
using System;

public partial class DiedInterface : Control
{
	Button _btnRestart;
	Button _mainMenu;
	private Button _btnGiveUp;
	AnimationPlayer _animPlayer;
	AudioStreamPlayer _buttonSounds;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");
		_btnRestart = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/Retry");
		_mainMenu = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/MainMenu");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_btnRestart.Pressed += OnRestartPressed;
		_mainMenu.Pressed += OnGiveUpPressed;
		if (_btnGiveUp != null) _btnGiveUp.Pressed += OnGiveUpPressed;
	}

	private void OnRestartPressed()
	{
		_buttonSounds.Play();
		GetTree().Paused = false; // Unpause the game before restarting
		string currentScenePath = GetTree().CurrentScene?.SceneFilePath;
		if (string.IsNullOrEmpty(currentScenePath))
		{
			currentScenePath = "res://scene/levels/area.tscn";
		}
		LoadingScreen.NextScenePath = currentScenePath;
		CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
	}

	private void ChangeSceneSafe(string path)
	{
		GetTree().ChangeSceneToFile(path);
	}

	private void OnMainMenuPressed(StringName backmain)
	{
		GD.Print("Animation Finished");
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scene/main_menu/main_menu.tscn");
	}
	private void OnGiveUpPressed()
	{
		_buttonSounds?.Play();
		GetTree().Paused = false;
		AnimationButton();
	}

	private void AnimationButton()
	{
        _animPlayer.Play("main");
				GD.Print("Animation PLaying");
				_animPlayer.AnimationFinished += OnMainMenuPressed;
		
	}
}
