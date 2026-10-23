using Godot;
using System;

public partial class Options : Button
{
	AnimationPlayer animPlayer;
	OptionButton languageOptions;
	HSlider _musicSlider;
	HSlider _sfxSlider;
	AudioStreamPlayer _buttonSounds;
	int verify = 0;
	private int _musicBusIndex;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animPlayer = GetNode<AnimationPlayer>("../../../../Options/AnimationPlayer2");
		_buttonSounds = GetNode<AudioStreamPlayer>("../../../../ButtonSound");
		languageOptions = GetNode<OptionButton>("../../../../Options/PanelContainer/MarginContainer/VBoxContainer/Language/OptionButton");
		_musicSlider = GetNode<HSlider>("../../../../Options/PanelContainer/MarginContainer/VBoxContainer/Music/MusicSlider");
		_sfxSlider = GetNode<HSlider>("../../../../Options/PanelContainer/MarginContainer/VBoxContainer/SFX/SFXSlider");
		this.Pressed += openOptions;
		
		languageOptions.AddItem("English", 0);
		languageOptions.AddItem("Jireh", 1);
		languageOptions.AddItem("Kalbo", 2);

		_musicBusIndex = AudioServer.GetBusIndex("Master");
		_musicSlider.Value = Mathf.DbToLinear(_musicBusIndex);
		_musicSlider.ValueChanged += SetMusicSlider;

		
	}

	
	public void SetMusicSlider(double value)
	{
		GD.Print(value, "Minecrafe");
		if (value <= 0.0)
		{
		AudioServer.SetBusMute(_musicBusIndex, true);
		}
		else
		{
		AudioServer.SetBusMute(_musicBusIndex, false);
		float dbVolume = Mathf.LinearToDb((float)value);
		AudioServer.SetBusVolumeDb(_musicBusIndex, dbVolume);
		}
	}

	//remove function
  protected override void Dispose(bool disposing)
  {
		if (disposing && _musicSlider != null)
		{
			_musicSlider.ValueChanged -= SetMusicSlider;
		}
    base.Dispose(disposing);
  }

	private void openOptions()
	{
		_buttonSounds.Play();

		if (verify == 0 && !animPlayer.IsPlaying())
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
