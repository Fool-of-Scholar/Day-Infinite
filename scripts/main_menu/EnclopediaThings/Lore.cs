using Godot;
using System;
using System.Linq;

public partial class Lore : PanelContainer
{
	Button[] _buttons;
	Label _title;
	Label _details;

	private string[] _DetailsFormat = new string[]
	{
		"The most common survivors of the city's collapse by these mass producing vermin. These low-tier rats are driven to produce more of their kind.",
		"They are formed when a dozen or more scuttling rodents devours each other for supremacy. A twitching mass of flesh and bones.",
		" Mutated vermin that nested too close, born early that supercharged their nervous systems into a fast, speedster that devours their pray.",
		"The original one, her eggs began the infestation of the mice in the city. A hulking mountain-like rat that crowned itself ruler of the undercity ruins.",
		"???",
		"???",
		"???",
		"???",
		"???"
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
