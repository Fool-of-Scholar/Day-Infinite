using Godot;
using System;

public partial class MainMenu : Control
{
	Button _cheat;
	Label _scrapsLabel;
	Label _coinsLabel;
	private SaveManager _saveManager;
	private int _currentScraps;
	private int _currentCoins;
	AnimationPlayer _animPlayer;
	Button _btnPlay;
	private Label _totalScrapsLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_cheat = GetNode<Button>("Options/PanelContainer/MarginContainer/VBoxContainer/Button");
		_scrapsLabel = GetNode<Label>("Currency/MarginContainer/VBoxContainer/Coins/MarginContainer/HBoxContainer/Label");
		_coinsLabel = GetNode<Label>("Currency/MarginContainer/VBoxContainer/Scraps/MarginContainer/HBoxContainer/Label");
		_saveManager = GetNode<SaveManager>("/root/SaveManager");

		_cheat.Pressed += () =>
		{
			_currentScraps = 1000;
			_currentCoins = 100000;
			TriggerSave();
		};

		TriggerLoad();
	}


	public void TriggerSave()
	{
		if (_saveManager != null)
		{
			_saveManager.SaveGame(_currentScraps, _currentCoins);
			GD.Print("Scraps changed!");
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

}
