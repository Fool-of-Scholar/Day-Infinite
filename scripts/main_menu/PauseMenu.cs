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
        Button btnRetry = GetNodeOrNull<Button>("TextureRect/Restart") ?? GetNodeOrNull<Button>("Restart");
        Button btnResume = GetNodeOrNull<Button>("TextureRect/Resume") ?? GetNodeOrNull<Button>("Resume");
        Button btnExit = GetNodeOrNull<Button>("TextureRect/Exit") ?? GetNodeOrNull<Button>("Exit");

        // 3. Connect listeners ONLY if the buttons are successfully found to prevent crashes
        if (btnRetry != null) btnRetry.Pressed += OnRetryPressed;
        if (btnResume != null) btnResume.Pressed += OnResumePressed;
        if (btnExit != null) btnExit.Pressed += OnExitPressed;

        if (btnRetry == null || btnResume == null || btnExit == null)
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

    private void OnExitPressed()
    {
        GetTree().Paused = false; 
        _pauseOpenAnimation?.Play("pause_in");
        GetTree().ChangeSceneToFile("res://scene/main_menu/another_main_menu.tscn");
    }
}
