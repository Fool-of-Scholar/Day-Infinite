using Godot;
using System;

public partial class Play : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		this.Pressed += openWorld;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void openWorld()
	{
		string scene = "res://scene/world.tscn";

		GetTree().ChangeSceneToFile(scene);
	}
}
