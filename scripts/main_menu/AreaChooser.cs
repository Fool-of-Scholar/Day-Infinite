using Godot;
using System;

public partial class AreaChooser : Control
{
	Button _startGame;
	Button _backWeapons;
	Button _nextArea;
	Button _prevArea;
	AnimationPlayer _animPlayer;
	Timer _timer;

	TextureRect _mainTextureArea;
	TextureRect _prevTextureArea;
	TextureRect _nextTextureArea;
	AudioStreamPlayer _buttonSounds;
	Panel _lockedManager;
	Label _areaDetailsLabel;
	Label _areaTitleLabel;

	private int count = 1;
	private bool[] _lockedManagerArea = new bool[]
	{
		true, //stopper
		true, //1
		false,
		false,
		false,
		false,
		false, //6
		true	//stopper
	};
	private string[] _areaTitle = new string[] //22 letters per line max
	{
		"Urban Decayed City",
		"???",
		"???",
		"???",
		"???",
		"???",
	};
	private string[] _areaDetails = new string[]
	{
		"\nThis city, a once-thriving\nmetropolis capital now\novergrown with mutated\nflora, populatedby a swarm\nof mice. The city was\ndesigned to be an\norchid-filled environmental\ncity",
		"???",
		"???",
		"???",
		"???",
		"???",
	};
	private string[] _areaChooser = new string[]
	{
		"res://assets/Encylopedia/buttons.png",//stopper
		"res://assets/main_menu/area/received_1996949624323731.jpeg",//1
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",//6
		"res://assets/Encylopedia/weapons.png"	//stopper
	};
	public override void _Ready()
	{
		_areaTitleLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/PanelContainer/VBoxContainer/AreaTitle");
		_areaDetailsLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/PanelContainer/VBoxContainer/AreaDetails");
		_lockedManager = GetNode<Panel>("Locked");
		_prevArea = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/PrevArea");
		_nextArea = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/NextArea");
		_backWeapons = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/BackButton");
		_startGame = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/ProceedWorld");
		_buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");

		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_timer = GetNode<Timer>("Timer");

		_mainTextureArea = GetNode<TextureRect>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/Main");
		_prevTextureArea = GetNode<TextureRect>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/Prev");
		_nextTextureArea = GetNode<TextureRect>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/Next");

		_startGame.Pressed += StartGame;
		_backWeapons.Pressed += BackToWeapons;
		_nextArea.Pressed += NextArea;
		_prevArea.Pressed += PrevArea;

		_mainTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count]);
		_nextTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count + 1]);
		_prevTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count - 1]);
		TextureChanged();
	}


	private void StartGame()
	{
		_buttonSounds.Play();
		string scene = "res://scene/levels/area.tscn";
		GetTree().ChangeSceneToFile(scene);
	}

	private void BackToWeapons()
	{
		_buttonSounds.Play();
		_animPlayer.PlayBackwards("fromWeapons");

		_animPlayer.AnimationFinished += (Back) => GetTree().ChangeSceneToFile("res://scene/main_menu/weapon_chooser.tscn");
		
	}

	private void NextArea()
	{
		if (_timer.TimeLeft == 0 && count <= 5)
		{
			_animPlayer.Play("upping");
			_timer.Start();

			GD.Print("Next Anim Played");
			count += 1;
			_timer.Timeout += TextureChanged;
			
		}
		
	}

	private void PrevArea()
	{
		if (_timer.TimeLeft == 0 && count >= 2)
		{
			_animPlayer.PlayBackwards("upping");
			_timer.Start();

			GD.Print("Prev Anim Played");

			count -= 1;
			_timer.Timeout += TextureChanged;
			
		}
		
		
	}

	private void TextureChanged()
	{
		_buttonSounds.Play();
		_mainTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count]);
		_nextTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count + 1]);
		_prevTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count - 1]);

		_areaDetailsLabel.Text = _areaDetails[count-1];
		_areaTitleLabel.Text = _areaTitle[count-1];

		GD.Print(count);
		if (_lockedManagerArea[count] == true)
		{
			_lockedManager.Visible = false;
			_startGame.Visible = true;
		}
		else
		{
			_lockedManager.Visible = true;
			_startGame.Visible = false;
		}
	}
}
