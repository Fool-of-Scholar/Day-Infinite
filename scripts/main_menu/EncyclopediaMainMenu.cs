using Godot;
using System;

public partial class EncyclopediaMainMenu : Button
{
	AnimationPlayer _animPlayer;
	AudioStreamPlayer _buttonSounds;
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("../../../../Options/AnimationPlayer2");
		_buttonSounds = GetNode<AudioStreamPlayer>("../../../../ButtonSound");


		this.Pressed += EnterAnim;
	}

	private void EnterAnim()
	{
		_buttonSounds.Play();

		_animPlayer.Play("Play");
		_animPlayer.AnimationFinished += EnterEncyclopedia;
	}

	private void EnterEncyclopedia(StringName Play)
	{
		GD.Print("go to Encyclopedia");
		GetTree().ChangeSceneToFile("res://scene/main_menu/encyclopedia.tscn");
	}
}
