using Godot;
using System;

public partial class OpeningScreen : Control
{
	private Timer _timer;
	[Export] public string NextPath = "res://scene/main_menu/another_main_menu.tscn";
	private bool _canClick = false;

	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		_timer.OneShot = true;
		_timer.Timeout += OnTimerTimeout;
		_timer.Start();

		// Start background preloading of Area 1 and Area 2 immediately while opening animation plays
		ScenePreloader.Instance?.StartPreloading();
	}

	public override void _Input(InputEvent @event)
	{
		if (!_canClick) return;

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			TransitionToNextScene();
		}
		else if (@event is InputEventScreenTouch touch && touch.Pressed)
		{
			TransitionToNextScene();
		}
		else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
		{
			TransitionToNextScene();
		}
	}

	private void OnTimerTimeout()
	{
		_canClick = true;
	}

	private void TransitionToNextScene()
	{
		_canClick = false;
		GetTree().ChangeSceneToFile(NextPath);
	}
}
