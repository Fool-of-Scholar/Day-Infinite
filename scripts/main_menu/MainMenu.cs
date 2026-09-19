using Godot;
using System;

public partial class MainMenu : Control
{
	AnimationPlayer _animPlayer;
	Button _btnPlay;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_btnPlay = GetNode<Button>("VBoxContainer/BtnPlay");
		_btnPlay.Pressed += OnPlayPressed;
	}

	private void OnPlayPressed()
	{
		QueueFree(); // Remove the current scene from the tree
	}
}
