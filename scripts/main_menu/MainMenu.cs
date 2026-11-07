using Godot;
using System;

public partial class MainMenu : Control
{
	Play _play;
	CharactersButton _charactersButton;
	EncyclopediaMainMenu _encyclopediaButton;
	Options _optionsButton;
	Exit _exitButton;
	private int downButton = 0;
	Button _cheat;
	Button _btnChar;
	Button _btnEncy;
	Button _btnOptions;
	Button _btnExit;
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
		_btnPlay = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/Play");
		_btnChar = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/Character");
		_btnEncy = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/Encyclopedia");
		_btnOptions = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/Options");
		_btnExit = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/Exit");

		_play = GetNode<Play>("PanelContainer/MarginContainer/VBoxContainer/Play");
		_charactersButton = GetNode<CharactersButton>("PanelContainer/MarginContainer/VBoxContainer/Character");
		_encyclopediaButton = GetNode<EncyclopediaMainMenu>("PanelContainer/MarginContainer/VBoxContainer/Encyclopedia");
		_optionsButton = GetNode<Options>("PanelContainer/MarginContainer/VBoxContainer/Options");
		_exitButton = GetNode<Exit>("PanelContainer/MarginContainer/VBoxContainer/Exit");

		NodePath emptyPath = new NodePath(".");
	
	Button[] allButtons = { _btnPlay, _btnChar, _btnEncy, _btnOptions, _btnExit };
	foreach (Button btn in allButtons)
	{
		btn.FocusNeighborTop = emptyPath;
		btn.FocusNeighborBottom = emptyPath;
		btn.FocusNeighborLeft = emptyPath;
		btn.FocusNeighborRight = emptyPath;
	}

	// ... rest of your existing _Ready() setup code ...
	_play = GetNode<Play>("PanelContainer/MarginContainer/VBoxContainer/Play");

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

	/* 
	 * Handles manual custom controller/keyboard navigation cycling.
	 * Consuming inputs via SetInputAsHandled() prevents Godot's built-in focus rules from overriding GrabFocus().
	 * Indexes updated to cycle evenly through all 5 main buttons:
	 * 0 = Play, 1 = Character, 2 = Encyclopedia, 3 = Options, 4 = Exit
	 */
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("downward"))
		{
			if (downButton == 0)
			{
				_btnChar.GrabFocus();
				downButton = 1;
			}
			else if (downButton == 1)
			{
				_btnEncy.GrabFocus();
				downButton = 2;
			}
			else if (downButton == 2)
			{
				_btnOptions.GrabFocus();
				downButton = 3;
			}
			else if (downButton == 3)
			{
				_btnExit.GrabFocus();
				downButton = 4;
			}
			else if (downButton == 4)
			{
				_btnPlay.GrabFocus();
				downButton = 0;
			}

			// Tells Godot that this controller movement action has been handled, 
			// stopping the engine's underlying automatic UI navigation from causing unwanted jumping.
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("upward"))
		{
			if (downButton == 0)
			{
				_btnExit.GrabFocus();
				downButton = 4;
			}
			else if (downButton == 1)
			{
				_btnPlay.GrabFocus();
				downButton = 0;
			}
			else if (downButton == 2)
			{
				_btnChar.GrabFocus();
				downButton = 1;
			}
			else if (downButton == 3)
			{
				_btnEncy.GrabFocus();
				downButton = 2;
			}
			else if (downButton == 4)
			{
				_btnOptions.GrabFocus();
				downButton = 3;
			}

			// Stops default engine focus from overriding upward selection.
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("enter"))
		{
			switch (downButton)
			{	
				case 0:
					_play.openWorldAnim();
					break;
				default:
					break;
			}
			
			// Suppresses unintended double trigger registrations on the UI layer.
			GetViewport().SetInputAsHandled();
		}
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
