using Godot;
using System;

public partial class PauseMenu : Control
{
    private AnimationPlayer _pauseOpenAnimation;

    public override void _Ready()
    {
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

        // 3. Connect listeners ONLY if the buttons are successfully found to prevent crashes
        if (btnRetry != null) btnRetry.Pressed += OnRetryPressed;
        if (btnResume != null) btnResume.Pressed += OnResumePressed;
        if (btnQuit != null) btnQuit.Pressed += OnQuitPressed;

        if (btnRetry == null || btnResume == null || btnQuit == null)
        {
            GD.PrintErr("HUD Pause Menu Warning: One or more button paths inside the script do not match the editor names!");
        }
    }

    private void OnRetryPressed()
    {
				GD.Print("Retry button pressed. Reloading current scene...");
        GetTree().Paused = false; 
        _pauseOpenAnimation?.Play("pause_in");
        GetTree().ReloadCurrentScene();
    }

    private void OnResumePressed()
    {
        GetTree().Paused = false;
        _pauseOpenAnimation?.Play("pause_out");
    }

    private void OnQuitPressed()
    {
        GetTree().Paused = false; 
        _pauseOpenAnimation?.Play("pause_in");
        GetTree().ChangeSceneToFile("res://scene/main_menu/another_main_menu.tscn");
    }
    
    private void OnSettingsPressed()
    {
        GD.Print("Settings button pressed. Opening settings menu...");
        // Implement your settings menu logic here
    }

    private void OnStatsPressed()
    {
        GD.Print("Stats button pressed. Opening stats menu...");
        // Implement your stats menu logic here
    }
}
