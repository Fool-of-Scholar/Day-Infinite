using Godot;
using System;

public partial class BackButton : TextureButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Pressed += BackPressed;
	}

	private void BackPressed ()
	{
		string scene = "res://scene/main_menu/main_menu.tscn";

		GetTree().ChangeSceneToFile(scene);
	}
}
