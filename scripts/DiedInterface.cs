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
		_btnRestart = GetNode<Button>("PanelContainer/HBoxContainer/Retry");
		_mainMenu = GetNode<Button>("PanelContainer/HBoxContainer/MainMenu");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_btnRestart.Pressed += OnRestartPressed;
		_mainMenu.Pressed += OnGiveUpPressed;
		if (_btnGiveUp != null) _btnGiveUp.Pressed += OnGiveUpPressed;
	}

	private void OnRestartPressed()
	{
		_buttonSounds.Play();
		GetTree().Paused = false; // Unpause the game before restarting
		GetTree().ReloadCurrentScene();
	}
	private void OnMainMenuPressed(StringName backmain)
	{
		GD.Print("Animation Finished");
		GetTree().ChangeSceneToFile("res://scene/main_menu/main_menu.tscn");
	}
	private async void OnGiveUpPressed()
    {
			_buttonSounds.Play();
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
				AnimationButton();
    }

	private void AnimationButton()
	{
        _animPlayer.Play("main");
				GD.Print("Animation PLaying");
				_animPlayer.AnimationFinished += OnMainMenuPressed;
		
	}
}
