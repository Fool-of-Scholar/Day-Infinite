using Godot;
using System;

public partial class PauseMenu : Control
{  
    HUD _hud;
    SaveManager _saveManager;
    private string _sceneString;
    private AnimationPlayer _pauseOpenAnimation;
    AnimationPlayer _animPlayer;
    OptionButton _languageOptions;
	HSlider _musicSlider;
	HSlider _sfxSlider;
    AudioStreamPlayer _buttonSounds;
    private bool verify = true;
    private int _musicBusIndex;
    private int _sfxBusIndex;
    public override void _Ready()
    {
        _hud = GetNode<HUD>("../../Hud");
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        // 1. Safe Parent Reference Check
        // This looks upward to find your Hud node, then down into its AnimationPlayer
        var hudNode = GetParent();
        if (hudNode != null)
        {
            // Update "AnimationPlayer" to match the EXACT name of the node in your HUD tree!
            // (e.g., "PauseOpenAnimation" or "AnimationPlayer")
            _pauseOpenAnimation = hudNode.GetNodeOrNull<AnimationPlayer>("../AnimationPlayer");
        }

        // 2. Safe Button Assignments (Removing the accidental '%' characters from strings)
        // If your buttons are named exactly "Retry", "Resume", and "Exit", this works perfectly.
        // If they are nested, update paths to "TextureRect/Retry" etc.
        Button btnRetry = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/Retry") ?? GetNodeOrNull<Button>("%Retry");
        Button btnResume = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/Resume") ?? GetNodeOrNull<Button>("%Resume");
        Button btnQuit = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/Quit") ?? GetNodeOrNull<Button>("%Quit");
        Button btnSettings = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/Settings") ?? GetNodeOrNull<Button>("%Settings");
        Button btnStats = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/Stats") ?? GetNodeOrNull<Button>("%Stats");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");

        

        // 3. Connect listeners ONLY if the buttons are successfully found to prevent crashes
        if (btnRetry != null) btnRetry.Pressed += OnRetryPressed;
        if (btnResume != null) btnResume.Pressed += OnResumePressed;
        if (btnQuit != null) btnQuit.Pressed += OnQuitPressed;
        if (btnSettings != null) btnSettings.Pressed += OnSettingsPressed;
        if (btnStats != null) btnStats.Pressed += OnStatsPressed;

        if (btnRetry == null || btnResume == null || btnQuit == null || btnSettings == null ||  btnStats == null)
        {
            GD.PrintErr("HUD Pause Menu Warning: One or more button paths inside the script do not match the editor names!");
        }

        _languageOptions = GetNode<OptionButton>("Options/PanelContainer/MarginContainer/VBoxContainer/Language/OptionButton");
        _musicSlider = GetNode<HSlider>("Options/PanelContainer/MarginContainer/VBoxContainer/Music/MusicSlider");
        _sfxSlider = GetNode<HSlider>("Options/PanelContainer/MarginContainer/VBoxContainer/SFX/SFXSlider");

        _languageOptions.AddItem("English", 0);
		_languageOptions.AddItem("Jireh", 1);
		_languageOptions.AddItem("Kalbo", 2);

        _musicBusIndex = AudioServer.GetBusIndex("Master");
		_musicSlider.Value = Mathf.DbToLinear(_musicBusIndex);
		_musicSlider.ValueChanged += SetMusicSlider;

        _sfxBusIndex = AudioServer.GetBusIndex("SFX");
        _sfxSlider.Value = Mathf.DbToLinear(_sfxBusIndex);
        _sfxSlider.ValueChanged += SetSFXSlider;
    }

    private void OnRetryPressed()
    {
        _buttonSounds.Play();
		GD.Print("Retry button pressed. Reloading current scene...");
        GetTree().Paused = false; 
        _pauseOpenAnimation?.Play("pause_in");
        GetTree().ReloadCurrentScene();
    }

    private void OnResumePressed()
    {
        _buttonSounds.Play();
        GetTree().Paused = false;
        _pauseOpenAnimation?.Play("pause_out");
        _hud._pauseButton.Visible = true;
        _hud._joystick.Visible = true;
    }

    private void OnQuitPressed()
    {
        _buttonSounds.Play();
        GetTree().Paused = false; 
        _pauseOpenAnimation?.Play("pause_in");
        _sceneString = "res://scene/main_menu/main_menu.tscn";
        TriggerSave(_sceneString);
        CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
    }
    
    private void OnSettingsPressed()
    {
        _buttonSounds.Play();
        GD.Print("options");
        if (verify == true)
        {
            _animPlayer.Play("options");
            GD.Print("Settings button pressed. Opening settings menu...");
            // Implement your settings menu logic here
            verify = false;
        }
    }

    private void OnStatsPressed()
    {
        _buttonSounds.Play();
        GD.Print("stats");
        if (verify == false)
        {
            _animPlayer.Play("stats");
            GD.Print("Stats button pressed. Opening stats menu...");
            // Implement your stats menu logic here
            verify = true;
        }
    }

    //Music
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

    public void SetSFXSlider(double value)
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


	//remove function
    protected override void Dispose(bool disposing)
    {
        if (disposing && _musicSlider != null)
        {
            _musicSlider.ValueChanged -= SetMusicSlider;
        }
        base.Dispose(disposing);
    }
    
}
