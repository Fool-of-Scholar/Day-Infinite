using Godot;
using System;
using System.Linq;

public partial class Upgrades : PanelContainer
{
	Button[] _buttons;
	Label _title;
	Label _details;
	AudioStreamPlayer _buttonSound;

	private string[] _DetailsFormat = new string[]
	{
		"Swarm fillers that rush the player in a steady, relentless line to force constant movement.",
		"Slow, bulky area-denial units that block paths and require area-of-effect weapons to scatter.",
		"High-speed dash units that sprint across the asphalt to disrupt player positioning and bypass primary firing lines.",
		"The stage milestone boss that commands the lower swarms and drops your rare Boss something upon defeat.",
		"As their packs grow larger, they become far more bloodthirsty. They strike in disciplined packs, prowling at speed and lethality.",
		"Durable monstrosities that encircle their foes. They act  in an uncanny hive synchronization, they emerge from the shadows.",
		"Wandering wraiths that materialize out of thin air. Their origins are unknown, but they possess vengefulness when someone intrudes the forest.",
		"They are born from decaying roots, a corrupted humus of the woods. They attempt to overwhelm their prey through sheer numbers.",
		"A towering, tree mountain-like juggernaut embodying no will. It is the root source for commanding every creature in its territory."
	};
	private string[] _TitleFormat = new string[]
	{
		"Basic Mice: Scrap-Tail Vermin",
		"Clustered Mice: The Wire-Ball Vermin",
		"Fast Mice: The Squeakers",
		"Boss Mice: Vermis Prime, The Core Queen",
		"Dread Wolf-Hound",
		"Sylvan Stalker (Entrapping Creature)",
		"Spirit Kappa",
		"The Blighted Crawler (Basic Creature)",
		"Primal Warden (Boss)"
	};
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_title = GetNode<Label>("VBoxContainer/HBoxContainer/Panel/VBoxContainer/Title");
		_details = GetNode<Label>("VBoxContainer/HBoxContainer/Panel/VBoxContainer/Details");
		_buttonSound = GetNode<AudioStreamPlayer>("../../../../ButtonSound");

		var container = GetNode<VBoxContainer>("VBoxContainer/HBoxContainer/ScrollContainer/VBoxContainer");

		_buttons = container.GetChildren().OfType<Button>().ToArray();
		
		if (_buttons == null) return;

		for (int i = 0; i <_buttons.Length; i++)
		{
			int index = i;
			_buttons[i].Pressed += () => OnButtonPressed(index);
			_buttons[i].Text = _TitleFormat[index];
		}

		DetailsFormatter(0);
	}

	private void OnButtonPressed(int buttonIndex)
	{
		_buttonSound.Play();
		GD.Print("hi");
		switch(buttonIndex)
		{
			case 0: DetailsFormatter(buttonIndex); break;
			case 1: DetailsFormatter(buttonIndex); break;
			case 2: DetailsFormatter(buttonIndex); break;
			case 3: DetailsFormatter(buttonIndex); break;
			case 4: DetailsFormatter(buttonIndex); break;
			case 5: DetailsFormatter(buttonIndex); break;
			case 6: DetailsFormatter(buttonIndex); break;
			case 7: DetailsFormatter(buttonIndex); break;
			case 8: DetailsFormatter(buttonIndex); break;
		}
	}

	private void DetailsFormatter(int index)
	{
		GD.Print("Changed");
		_details.Text = _DetailsFormat[index];
		_title.Text = _TitleFormat[index];
	}
}
