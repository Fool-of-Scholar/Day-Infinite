using Godot;
using System;

public partial class Play : Button
{
	private string _sceneString;
	SaveManager _saveManager;
	AnimationPlayer _animPlayer;
	AudioStreamPlayer _buttonSounds;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_saveManager = GetNode<SaveManager>("/root/SaveManager");
		_animPlayer = GetNode<AnimationPlayer>("../../../../Options/AnimationPlayer2");
		_buttonSounds = GetNode<AudioStreamPlayer>("../../../../ButtonSound");

		this.Pressed += openWorldAnim;
	}

	private void openWorldAnim()
	{
		_buttonSounds.Play();
		_animPlayer.Play("Play");
		_animPlayer.AnimationFinished += openWorld;
	}

	private void openWorld(StringName Play)
	{
		_sceneString = "res://scene/main_menu/weapon_chooser.tscn";
		TriggerSave(_sceneString);
		CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
	}

	private void ChangeSceneSafe(string path)
    {
        var _scene = GD.Load<PackedScene>(path);
        if (_scene != null)
        {
            GetTree().ChangeSceneToPacked(_scene);
        }
        else
        {
            GD.PrintErr($"Failed to load: {path}");
        }
    }
	
	public void TriggerSave(string scene)
	{
		if (_saveManager != null)
		{
			GD.Print("Scraps changed!");
			_saveManager.SavedArea(scene);
		}
	}

}
