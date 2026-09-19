using Godot;
using System;

public partial class OpeningScreen : Control
{

	Timer timer;
	Label label;
<<<<<<< Updated upstream
	[Export] public string nextPath = "res://scene/main_menu/main_menu.tscn";
=======
	[Export] public string nextPath = "res://scene/main_menu/another_main_menu.tscn";
>>>>>>> Stashed changes
	private bool _canClick = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");
		label = GetNode<Label>("PanelContainer/MarginContainer/Label");
		timer.OneShot = true;

		timer.Timeout += OnTimerTimeout;
		
		timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

  public override void _Input(InputEvent @event)
  {
    if (_canClick && @event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				TransitionToNextScene();
			}
		}
<<<<<<< Updated upstream
=======
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Pressed && keyEvent.Keycode == Key.Space)
			{
				TransitionToNextScene();
			}
		}
>>>>>>> Stashed changes
  }

	private void OnTimerTimeout()
	{
		_canClick = true;
		label.Text = "You can now proceed. jireh kalbo";
	}

	private void TransitionToNextScene()
	{
		GetTree().ChangeSceneToFile(nextPath);
	}
}
