using Godot;
using System;

public partial class AreaChooser : Control
{
	private string _sceneString;
	Label _scrapsLabel;
	Label _coinsLabel;
	private SaveManager _saveManager;
	private int _currentScraps;
	private int _currentCoins;
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
		true,
		false,
		false,
		false,
		false, //6
		true	//stopper
	};
	private string[] _areaTitle = new string[] //22 letters per line max
	{
		"Urban Decayed City",
		"Primal Forest (Bukid)",
		"???",
		"???",
		"???",
		"???",
	};
	private string[] _areaDetails = new string[]
	{
		"\nThis city, a once-thriving\nmetropolis capital now\novergrown with mutated\nflora, populatedby a swarm\nof mice. The city was\ndesigned to be an\norchid-filled environmental\ncity.",
		"\nThe mountains and forests in\nthe outskirt, once lush\nand filled with serenity,\nnow harbors corrupted apex\npredators out to kill\nanyone. Inside the thick\nmist is filled with domestic\nberserk dogs.",
		"???",
		"???",
		"???",
		"???",
	};
	private string[] _areaScene = new string[]
	{
		"res://scene/levels/area.tscn",
		"res://scene/levels/area_2.tscn",
		"",
		"",
		"",
		""
	};
	private string[] _areaChooser = new string[]
	{
		"res://assets/main_menu/area/Stopper.jpg",//stopper
		"res://assets/main_menu/area/Area1.jpeg",//1
		"res://assets/main_menu/area/Area2.webp",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",
		"res://assets/Encylopedia/weapons.png",
		"res://assets/Encylopedia/buttons.png",//6
		"res://assets/main_menu/area/Stopper.jpg"	//stopper
	};
	public override void _Ready()
	{
		_scrapsLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Currency/Coins/MarginContainer/HBoxContainer/Label");
		_coinsLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Currency/Scraps/MarginContainer/HBoxContainer/Label");
		_saveManager = GetNode<SaveManager>("/root/SaveManager");

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
		_timer.Timeout += TextureChanged;

		_mainTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count]);
		_nextTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count + 1]);
		_prevTextureArea.Texture = GD.Load<Texture2D>(_areaChooser[count - 1]);
		TextureChanged();
		TriggerLoad();
	}


	private void StartGame()
	{
		_buttonSounds.Play();
		int index = Mathf.Clamp(count - 1, 0, _areaScene.Length - 1);
		_sceneString = _areaScene[index];
		if (string.IsNullOrEmpty(_sceneString))
		{
			_sceneString = "res://scene/levels/area.tscn";
		}
		TriggerSave(_sceneString);
		CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
	}

	private void BackToWeapons()
	{
		_buttonSounds.Play();
		_animPlayer.PlayBackwards("fromWeapons");
		_sceneString = "res://scene/main_menu/weapon_chooser.tscn";
		TriggerSave(_sceneString);
		_animPlayer.AnimationFinished += (Back) => CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
	}

	private void NextArea()
	{
		if (_timer.TimeLeft == 0 && count <= 5)
		{
			_animPlayer.Play("upping");
			_timer.Start();

			GD.Print("Next Anim Played");
			count += 1;
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

	public void TriggerSave(string scene)
	{
		if (_saveManager != null)
		{
			_saveManager.SaveGame(_currentScraps, _currentCoins);
			GD.Print("Scraps changed!");
			_saveManager.SavedArea(scene);
		}
	}
	public void TriggerLoad()
	{
		if (_saveManager != null)
		{
			SaveData loadedData = _saveManager.LoadGame();
			_currentScraps = loadedData._scrapsTotal;
			_currentCoins = loadedData._coinsTotal;
			_scrapsLabel.Text = $"{_currentScraps}";
			_coinsLabel.Text = $"{_currentCoins}";
		}
	}
	private void ChangeSceneSafe(string path)
    {
        var _scene = GD.Load<PackedScene>(path);
        if (_scene != null)
        {
            GetTree().ChangeSceneToPacked(_scene);
        }
        else
        {
            GD.PrintErr($"Failed to load: {path}");
        }
    }
}
