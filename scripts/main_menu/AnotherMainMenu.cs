using Godot;
using System;

public partial class AnotherMainMenu : Control
{
	private readonly string _nextScenePath = "res://scene/main_menu/main_menu.tscn";
	AnimationPlayer _animPlayer;
	Timer _timer;
	Timer start;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_timer = GetNode<Timer>("LoadingScene");
		start = GetNode<Timer>("Start");
		start.Timeout += () => {
			TransitionToNextScene();
			var audioManager = GetNode<AudioManager>("/root/AudioManager");
      audioManager.PlayMusic();
		};
	}



	private void TransitionToNextScene()
	{

		_animPlayer.Play("mode");
		_animPlayer.AnimationFinished += (StringName animName) => {
			if (animName == "mode")
			{
				start.Start();
			}
		};

		_timer.Start();
		_timer.Timeout += () => {
			GetTree().Root.AddChild(GD.Load<PackedScene>(_nextScenePath).Instantiate());
			QueueFree(); // Remove the current scene from the tree
		};
	}
}

