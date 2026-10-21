using Godot;
using System;

public partial class Exit : Button
{
	AudioStreamPlayer _buttonSounds;
	public override void _Ready()
	{
		_buttonSounds = GetNode<AudioStreamPlayer>("../../../../ButtonSound");

		this.Pressed += exitGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void exitGame()
	{
		_buttonSounds.Play();

		GetTree().Quit();
	}
}
