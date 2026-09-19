using Godot;
using System;

public partial class DiedInterface : Control
{
	Button _btnRestart;
	Button _mainMenu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_btnRestart = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/Retry");
		_mainMenu = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/MainMenu");

		_btnRestart.Pressed += OnRestartPressed;
		_mainMenu.Pressed += OnMainMenuPressed;
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false; // Unpause the game before restarting
		GetTree().ReloadCurrentScene();
	}
	private void OnMainMenuPressed()
	{
		GetTree().Paused = false; // Unpause the game before going to main menu
		GetTree().ChangeSceneToFile("res://scene/main_menu/another_main_menu.tscn");
	}
}
