using Godot;
using System;

public partial class AreaChooser : Control
{
	Button _startGame;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_startGame = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/Button");

		_startGame.Pressed += StartGame;
	}


	private void StartGame()
	{
		GetTree().ChangeSceneToFile("res://scene/world.tscn");
	}
}
