using Godot;
using System;

public partial class DiedInterface : Control
{
	Button _btnRestart;
	Button _mainMenu;
	private Button _btnGiveUp;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_btnRestart = GetNodeOrNull<Button>("TextureRect/Retry");
		_mainMenu = GetNodeOrNull<Button>("TextureRect/MainMenu");
		_btnGiveUp = GetNodeOrNull<Button>("TextureRect/GiveUp");

		if (_btnRestart != null) _btnRestart.Pressed += OnRestartPressed;
		if (_mainMenu != null) _mainMenu.Pressed += OnMainMenuPressed;
		if (_btnGiveUp != null) _btnGiveUp.Pressed += OnGiveUpPressed;
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
	private async void OnGiveUpPressed()
    {
        // 1. Hide the DiedInterface so the screen and camera are clear
        Visible = false;
        // 2. IMPORTANT: Unpause the tree, otherwise AnimatedSprite2D cannot advance frames!
        GetTree().Paused = false;
        // 3. Find the player
        Movement player = (GetTree().GetFirstNodeInGroup("player") as Movement)
                       ?? GetTree().CurrentScene.GetNodeOrNull<Movement>("Player");
        if (player != null)
        {
            // Trigger the death VFX on the player
            player.PlayDeathVfxOnly();
        }
        // 4. Wait for the death animation to complete (18 frames at 22 fps ~= 0.85s)
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
        // 5. Game is over -> send to Main Menu
        GetTree().ChangeSceneToFile("res://scene/main_menu/another_main_menu.tscn");
    }
}
