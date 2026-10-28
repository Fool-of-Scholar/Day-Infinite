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

		"This city, a once-thriving metropolis capital now overgrown with mutated flora, populated by a swarm of mice. The city was designed to be a an orchid-filled environmental city",
		"The mountains and forests in the outskirt, once lush and filled with serenity, now harbors corrupted apex predators out to kill anyone. Inside the thick mist is filled with domestic berserk dogs",
		"???",
		"???",
		"???",
		"???",
		"???",
		"???",
		"???"
	};
	private string[] _TitleFormat = new string[]
	{
		"Urban Decayed City",
		"Primal Forest (Bukid)",
		"???",
		"???",
		"???",
		"???",
		"???",
		"???",
		"???",
		"???"
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
