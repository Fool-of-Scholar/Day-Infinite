using Godot;
using System;

public partial class InputManager : Node
{
    private const string SavePath = "user://keybinds.cfg";
    private readonly string[] _actionsToSave = {"left", "right", "upward", "downward"};

    public override void _Ready()
    {
        LoadKeybinds();
    }

    public void SaveKeybind(string actionName, InputEventKey newEvent)
    {
        var config = new ConfigFile();
        // Load existing config if it exists so we don't overwrite other data
        config.Load(SavePath); 

        // Store physical keycode as it is safer across different keyboard layouts
        config.SetValue("Keybinds", actionName, (long)newEvent.PhysicalKeycode);
        config.Save(SavePath);

        // Apply immediately to the live InputMap
        UpdateInputMap(actionName, newEvent);
    }

    public void LoadKeybinds()
    {
        var config = new ConfigFile();
        if (config.Load(SavePath) != Error.Ok) return; // No file yet, use project defaults

        foreach (string action in _actionsToSave)
        {
            if (config.HasSectionKey("Keybinds", action))
            {
                long rawKeycode = (long)config.GetValue("Keybinds", action);
                
                var newEvent = new InputEventKey();
                newEvent.PhysicalKeycode = (Key)rawKeycode;

                UpdateInputMap(action, newEvent);
            }
        }
    }

    private void UpdateInputMap(string actionName, InputEventKey newEvent)
    {
        if (InputMap.HasAction(actionName))
        {
            InputMap.ActionEraseEvents(actionName);
            InputMap.ActionAddEvent(actionName, newEvent);
        }
    }
}
