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
		var _ency = GD.Load<PackedScene>("res://scene/main_menu/encyclopedia.tscn");
		GD.Print("go to Encyclopedia");
		GetTree().ChangeSceneToPacked(_ency);
	}
}
