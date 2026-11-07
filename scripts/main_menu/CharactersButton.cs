using Godot;

public partial class CharactersButton : Button
{
	public override void _Ready()
	{
		Pressed += OnPressed;
	}

	public void OnPressed()
	{
		var scene = GD.Load<PackedScene>("res://scene/main_menu/character_chooser.tscn");
		if (scene != null)
			GetTree().ChangeSceneToPacked(scene);
		else
			GD.PrintErr("[CharactersButton] Failed to load character_chooser.tscn");
	}
}

