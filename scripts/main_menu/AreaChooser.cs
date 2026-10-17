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

	private int count = 1;
	private string[] _areaChooser = new string[]
	{
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
	};
	public override void _Ready()
	{
		_prevArea = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/PrevArea");
		_nextArea = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/NextArea");
		_backWeapons = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/BackButton");
		_startGame = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/ProceedWorld");

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
	}


	private void StartGame()
	{
		string scene = "res://scene/levels/area.tscn";
		GetTree().ChangeSceneToFile(scene);
	}

	private void BackToWeapons()
	{
		_animPlayer.PlayBackwards("fromWeapons");

		_animPlayer.AnimationFinished += (Back) => GetTree().ChangeSceneToFile("res://scene/main_menu/weapon_chooser.tscn");
		
	}

	private void NextArea()
	{
		if (_timer.TimeLeft == 0 && count <= 6)
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
		if (_timer.TimeLeft == 0 && count >= 1)
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
		_mainTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count]);
		_nextTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count + 1]);
		_prevTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count - 1]);
	}
}
