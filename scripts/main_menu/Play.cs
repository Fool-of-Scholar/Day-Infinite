using Godot;
using System;

public partial class Play : Button
{
	AnimationPlayer _animPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("../../../../Options/AnimationPlayer2");

		this.Pressed += openWorldAnim;
	}

	private void openWorldAnim()
	{
		_animPlayer.Play("Play");
		_animPlayer.AnimationFinished += openWorld;
	}

	private void openWorld(StringName Play)
	{
		string scene = "res://scene/main_menu/weapon_chooser.tscn";

		GetTree().ChangeSceneToFile(scene);
	}
}
