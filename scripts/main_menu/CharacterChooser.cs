using Godot;
using System;

public partial class CharacterChooser : Control
{
	private SaveManager _saveManager;
	private int _currentIndex = 0; // 0 = Null Beta, 1 = DMO
	private const int TotalCharacters = 2;

	// Node references (matched in character_chooser.tscn)
	private Label _nameLabel;
	private Label _descLabel;
	private TextureRect _previewRect;
	private Button _prevBtn;
	private Button _nextBtn;
	private Button _backBtn;
	private Button _selectBtn;

	private Texture2D[] _previews = new Texture2D[2];
	private string[] _names = new string[]
	{
		"Null Beta",
		"DMO"
	};
	private string[] _descriptions = new string[]
	{
		"A broken wanderer who ventures for a lost cause of his master's will.",
		"A wanderer who awakened in the post-apocalyptic world ages and searching for the clue of why this world is destroyed and find a way to fix and bring humanity back."
	};

	public override void _Ready()
	{
		_saveManager = GetNode<SaveManager>("/root/SaveManager");

		_nameLabel   = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/CharName");
		_descLabel   = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/CharDesc");
		_previewRect = GetNode<TextureRect>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/Preview");
		_prevBtn     = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/PrevBtn");
		_nextBtn     = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/NextBtn");
		_backBtn     = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/BackBtn");
		_selectBtn   = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/SelectBtn");

		// Sliced character preview frames (using AtlasTexture so only 1 frame shows instead of the whole spritesheet)
		var nullBetaTexture = GD.Load<Texture2D>("res://assets/Movement/Idle All SPriteSheet.png");
		_previews[0] = new AtlasTexture
		{
			Atlas = nullBetaTexture,
			Region = new Rect2(0, 32, 32, 32) // Frame 6: front-facing idle (32x32)
		};

		var dmoTexture = GD.Load<Texture2D>("res://assets/new/south-east idle (1).png");
		_previews[1] = new AtlasTexture
		{
			Atlas = dmoTexture,
			Region = new Rect2(0, 0, 64, 64) // Frame 0: idle stance (64x64)
		};

		_prevBtn.Pressed   += OnPrev;
		_nextBtn.Pressed   += OnNext;
		_backBtn.Pressed   += OnBack;
		_selectBtn.Pressed += OnSelect;

		// Load saved selection
		if (_saveManager != null)
		{
			_currentIndex = _saveManager.LoadGame()._selectedCharacter;
		}

		Refresh();
	}

	private void Refresh()
	{
		_nameLabel.Text  = _names[_currentIndex];
		_descLabel.Text  = _descriptions[_currentIndex];
		_previewRect.Texture = _previews[_currentIndex];
		_prevBtn.Disabled = _currentIndex == 0;
		_nextBtn.Disabled = _currentIndex == TotalCharacters - 1;
	}

	private void OnPrev()
	{
		if (_currentIndex > 0)
		{
			_currentIndex--;
			Refresh();
		}
	}

	private void OnNext()
	{
		if (_currentIndex < TotalCharacters - 1)
		{
			_currentIndex++;
			Refresh();
		}
	}

	private void OnSelect()
	{
		_saveManager?.SaveCharacter(_currentIndex);
		GD.Print($"[CharacterChooser] Selected character: {_names[_currentIndex]}");
		OnBack();
	}

	private void OnBack()
	{
		CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/main_menu.tscn");
	}

	private void ChangeSceneSafe(string path)
	{
		var scene = GD.Load<PackedScene>(path);
		if (scene != null)
			GetTree().ChangeSceneToPacked(scene);
		else
			GD.PrintErr($"[CharacterChooser] Failed to load: {path}");
	}
}

