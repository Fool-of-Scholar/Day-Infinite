using Godot;
using System;

public partial class Options : Button
{
	AnimationPlayer animPlayer;
	OptionButton languageOptions;
	int verify = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
<<<<<<< Updated upstream
		animPlayer = GetNode<AnimationPlayer>("../../../../AnimationPlayer");
=======
		animPlayer = GetNode<AnimationPlayer>("../../../../Options/AnimationPlayer2");
>>>>>>> Stashed changes
		languageOptions = GetNode<OptionButton>("../../../../Options/PanelContainer/MarginContainer/VBoxContainer/Language/OptionButton");
		this.Pressed += openOptions;
		
		languageOptions.AddItem("English", 0);
		languageOptions.AddItem("Jireh", 1);
		languageOptions.AddItem("Kalbo", 2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void openOptions()
	{
<<<<<<< Updated upstream
		if (verify == 0)
=======
		if (verify == 0 && !animPlayer.IsPlaying())
>>>>>>> Stashed changes
		{
			animPlayer.Play("options_in");
			verify = 1;
		}
		else
		{
			animPlayer.Play("options_out");
			verify = 0;
			
		}
	}
}
