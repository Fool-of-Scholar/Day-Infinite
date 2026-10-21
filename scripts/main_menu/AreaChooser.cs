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

	// Index 0 and last are sentinels so Prev/Next can peek safely.
	// Index 1 = Area 1, Index 2 = Area 2.
	private string[] _areaChooser = new string[]
	{
		"res://assets/Tilemap/Area 1.png",  // [0] left-peek sentinel
		"res://assets/Tilemap/Area 1.png",  // [1] Area 1
		"res://assets/Tilemap/Area 2.png",  // [2] Area 2
		"res://assets/Tilemap/Area 2.png",  // [3] right-peek sentinel
	};

	// Maps chooser index → scene path
	private string[] _areaScenes = new string[]
	{
		"res://scene/levels/area.tscn",    // index 1 → Area 1
		"res://scene/levels/area_2.tscn",  // index 2 → Area 2
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
		// count 1 = Area 1, count 2 = Area 2 (maps to _areaScenes index 0, 1)
		int sceneIndex = Mathf.Clamp(count - 1, 0, _areaScenes.Length - 1);
		string scene = _areaScenes[sceneIndex];
		GetTree().ChangeSceneToFile(scene);
	}

	private void BackToWeapons()
	{
		_animPlayer.PlayBackwards("fromWeapons");

		_animPlayer.AnimationFinished += (Back) => GetTree().ChangeSceneToFile("res://scene/main_menu/weapon_chooser.tscn");
		
	}

	private void NextArea()
	{
		// Cap at index 2 (Area 2), which is _areaChooser.Length - 2
		if (_timer.TimeLeft == 0 && count < _areaChooser.Length - 2)
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
		// Floor at index 1 (Area 1)
		if (_timer.TimeLeft == 0 && count > 1)
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
