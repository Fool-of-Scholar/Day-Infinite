using Godot;
using System;
using System.Linq;

public partial class WeaponChooser : Control
{
<<<<<<< Updated upstream
	Button[] _statsButtons;
=======
	private int _currentStatsIndex = 0;
	private int downButton = 0;
>>>>>>> Stashed changes
	private string _sceneString;
	private int[] _upgradeTimesStats = new int[]
	{
		0,
		0,
		0,
		0,
		0
	};
	Label _scrapsLabel;
	Label _coinsLabel;
	Label _weaponPriceLabel;
	private SaveManager _saveManager;
	private int _currentScraps;
	private int _currentCoins;
	AnimationPlayer _animPlayer;
	Button _next;
	Button _prev;
	Button _back;
	Button _playerStats;
	Button _proceedArea;
	Button _buyButton;
	TextureRect _weaponPrev;
	TextureRect _weaponMain;
	TextureRect _weaponNext;
	Label _weaponName;
	Label _weaponStats;
	Panel _lockedManager;
	AudioStreamPlayer _buttonSounds;
	private bool verify = false;
	public string[] _weaponStringModifierDesc = new string[]
	{
		"Fast, accurate weapon that automatically tracks and fires toward enemies\n",
		"Heavy, high-impact weapon that destroy horde and tanky enemies\n",
		"Instant lightning discharge that penetrate toward enemies\n",
		"Ground Trap that release near Player, dealing most highest burst damage\n",
		"Protective Shield surrounding the player, knockblast enemies and protect you \n"
	};
	public string[] _weaponStringModifierName = new string[]
	{
		"Assault Rifle (AR)",
		"Rocket Launcher",
		"Tesla Burst",
		"Landmine",
		"ForceField"
	};
	public string[] _weaponStringModifierStats = new string[]
	{
		"Damage: 15 per bullet\nFire Rate: 0.5s\nProjectile Speed: 900\nMax Range: 800",
		"Damage: 90 explosion damage\nFire Rate: 2.0s\nProjectile Speed: 380\nMax Range: 900",
		"Damage: 50 per beam strike\nFire Rate: 1.2s\nChain Range: 380\nDeliver: Instantaneuos line of Ligthning",
		"Damage: 120 detonation burst\nCooldown: 3.5s\nTrap Lifetime: 12.0s\n",
		"Damage: 25 contact damage\nKnockback Force: 400 impulse pushing enemies away\nCooldown: 2.0s\n",
	};
	public string[] _weaponPaths = new string[]
	{
	"res://assets/PIXWEP/desert/body/body_laser_01.png",//stopper
	"res://assets/PIXWEP/body/body_laser_01.png",//1
	"res://assets/PIXWEP/body/body_laser_02.png",
	"res://assets/PIXWEP/body/body_plasma_01.png",
	"res://assets/PIXWEP/body/body_plasma_02.png",
	"res://assets/PIXWEP/body/body_projectile_01.png",//5
	"res://assets/PIXWEP/desert/body/body_laser_01.png"//stopper
	};

	public int[] _weaponPrices = new int[]
	{
		10, //sttaic
		110,
		4,
		6,
		6
	};

	public bool[] _lockManagerWeapon = new bool[]
	{
		true, //1
		false,
		false,
		false,
		false //5
	};

	public int[] _statsPrices = new int[]
	{
		3000, //sttaic
		3000,
		3000,
		3000,
		3000,

	};

	//for null weapons
	private String _Weapon0 = "res://assets/Encylopedia/weapons.png";
	private String _Weapon6 = "res://assets/Encylopedia/weapons.png";

	private int count = 1;
	private String _weaponStringMain = "";
	private String _weaponStringPrev = "";
	private String _weaponStringNext = "";

	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var container = GetNode<HBoxContainer>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer");
		_statsButtons = container.GetChildren().OfType<Button>().ToArray();

		for (int i = 0; i < _statsButtons.Length; i++)
		{
			int _index = i;
			_statsButtons[i].Pressed += () => BuyStats(_index);
			GD.Print(_index);
		}

		_weaponPriceLabel = GetNode<Label>("PanelContainer/Locked/PriceLabel");
		_buyButton = GetNode<Button>("PanelContainer/Locked/Buy");
		_playerStats = GetNode<Button>("VBoxContainer/HBoxContainer2/Player");

		_scrapsLabel = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/Cash/Coins/MarginContainer/HBoxContainer/Label");
		_coinsLabel = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/Cash/Scraps/MarginContainer/HBoxContainer/Label");
		_saveManager = GetNode<SaveManager>("/root/SaveManager");

		_buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_next = GetNode<Button>("PanelContainer/HBoxContainer/Next");
		_prev = GetNode<Button>("PanelContainer/HBoxContainer/Prev");
		_back = GetNode<Button>("VBoxContainer/HBoxContainer2/Back");
		_proceedArea = GetNode<Button>("VBoxContainer/HBoxContainer2/PlayScene");
		_lockedManager = GetNode<Panel>("PanelContainer/Locked");

		_weaponName = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/WeaponName");
		_weaponStats = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/Details");

		_weaponMain = GetNode<TextureRect>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/MainWeapon");
		_weaponPrev = GetNode<TextureRect>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/PrevWeapon");
		_weaponNext = GetNode<TextureRect>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/NextWeapon");

		_next.Pressed += NextWeaponFunc;
		_prev.Pressed += PrevWeaponFunc;
		_back.Pressed += BackHome;
		_proceedArea.Pressed += ProceedAnim;
		_buyButton.Pressed += BuyWeapon;
		_playerStats.Pressed += OpenStats;
		NextWeapon();

		_animPlayer.AnimationFinished += BottomButtons;
		_lockedManager.Visible = false;
		TriggerLoad();
	}

<<<<<<< Updated upstream
  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Pressed && keyEvent.Keycode == Key.D || keyEvent.Keycode == Key.Right)
=======
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("left"))
		{
			if(_isStatsPanelOpen == true)
>>>>>>> Stashed changes
			{
				_currentStatsIndex--;
				if (_currentStatsIndex <= 5 && _currentStatsIndex >= 0)
				{
					ToggleUpdateStats(_currentStatsIndex);
				}
			}
<<<<<<< Updated upstream
			if (keyEvent.Pressed && keyEvent.Keycode == Key.A || keyEvent.Keycode == Key.Left)
=======
			else
>>>>>>> Stashed changes
			{
			PrevWeaponFunc();
			}
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("right"))
		{
			
			if(_isStatsPanelOpen == true)
			{
				_currentStatsIndex++;
				if (_currentStatsIndex <= 5 && _currentStatsIndex >= 0)
				{
					ToggleUpdateStats(_currentStatsIndex);
				}
			}
			else
			{
			NextWeaponFunc();
			}
			GetViewport().SetInputAsHandled();
		}
			
		else if (@event.IsActionPressed("downward"))
		{
			if (downButton == 0)
			{
				_playerStats.GrabFocus();
				downButton = 1;
			}
			else if (downButton == 1)
			{
				_proceedArea.GrabFocus();
				downButton = 2;
			}
			else if (downButton == 2)
			{
				_back.GrabFocus();
				downButton = 0;
			}
			GetViewport().SetInputAsHandled();
;		}
		else if (@event.IsActionPressed("upward"))
		{
			if (downButton == 0)
			{
				_back.GrabFocus();
				downButton = 2;
			}
			else if (downButton == 1)
			{
				_playerStats.GrabFocus();
				downButton = 0;
				GD.Print("Upward pressed, downButton set to 1");
			}
			else
			{
				_proceedArea.GrabFocus();
				downButton = 1;
				GD.Print("Upward pressed, downButton reset to 0");
			}
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("enter"))
		{
			switch(downButton)
			{	
				default:
					BackHome();
					break;
				case 0:
					ToggleStatsPanel();
					break;
				case 1:
					ProceedAnim();
					break;
			}
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("back"))
		{
			if (_isStatsPanelOpen)
			{
				ToggleStatsPanel();
			}
			GetViewport().SetInputAsHandled();
		}
  }


	private void NextWeapon()
	{
		_buttonSounds.Play();
		if (count >= 1 && count <= 5)
		{
			_weaponMain.Texture = GD.Load<Texture2D>(_weaponPaths[count]);
			_weaponNext.Texture = GD.Load<Texture2D>(_weaponPaths[count + 1]);
			_weaponPrev.Texture = GD.Load<Texture2D>(_weaponPaths[count - 1]);

			_weaponName.Text = _weaponStringModifierName[count - 1];
			_weaponStats.Text = _weaponStringModifierDesc[count - 1] + _weaponStringModifierStats[count - 1];

			_weaponPriceLabel.Text = $"Price: {_weaponPrices[count - 1]}";

			if (_lockManagerWeapon[count - 1] == false)
			{
				_lockedManager.Visible = true;
				_proceedArea.Visible = false;
			}
			else
			{
				_proceedArea.Visible = true;
				_lockedManager.Visible = false;
			}
		}
	}

	private void NextWeaponFunc()
	{
		GD.Print("Next");
		if (count <= 4)
		{
		count += 1;
		}
		NextWeapon();
	}
	private void PrevWeaponFunc()
	{
		GD.Print("Prev");
		if (count >= 2)
		{
			count -= 1;
		}
	NextWeapon();
	}

	private void BottomButtons(StringName animName)
	{	
		if (animName == "back")
		{
			_sceneString = "res://scene/main_menu/main_menu.tscn";
			TriggerSave();
		CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
		}
		else if (animName == "next")
		{
			_sceneString = "res://scene/main_menu/area_chooser.tscn";
			TriggerSave();
		CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
		}
		else
		{
			GD.Print("Open Stats");
		}
		
	}

	private void BackHome()
	{
		_buttonSounds.Play();
		_animPlayer.Play("back");
	}

	private void ProceedAnim()
	{
		_saveManager.SavedWeapon(count);
		_buttonSounds.Play();
		_animPlayer.Play("next");
	}
	
	public void TriggerSave()
	{
		if (_saveManager != null)
		{
			_saveManager.SaveGameMenu(_currentScraps,new Godot.Collections.Array<bool>(_lockManagerWeapon), _currentCoins, new Godot.Collections.Array<int>(_upgradeTimesStats));
			GD.Print("Scraps changed!");
			_saveManager.SavedArea(_sceneString);
		}
	}
	public void TriggerLoad()
	{
		if (_saveManager != null)
		{
			SaveData loadedData = _saveManager.LoadGame();

			_upgradeTimesStats = loadedData._statsRepeatUpgrades.ToArray();
			_lockManagerWeapon = loadedData._weaponUnlocked.ToArray();
			_currentScraps = loadedData._scrapsTotal;
			_currentCoins = loadedData._coinsTotal;
			_scrapsLabel.Text = $"{_currentScraps}";
			_coinsLabel.Text = $"{_currentCoins}";
		}
	}

	private void BuyWeapon()
	{
		if (count >= 1 && count <=5)
		{
			if (_currentScraps >= _weaponPrices[count - 1] )
			{
				_currentScraps -= _weaponPrices[count - 1];
				GD.Print(_currentScraps);
				_lockManagerWeapon[count - 1] = true;
				TriggerSave();
				GD.Print(_currentScraps);
				_scrapsLabel.Text = $"{_currentScraps}";

				_proceedArea.Visible = true;
				_lockedManager.Visible = false;
			}
			else
			{
				_weaponPriceLabel.Text = "Not enough scraps!!";
			}
		}
	}

	private void OpenStats()
	{
		if (verify == false)
		{
			_animPlayer.Play("player_open");
			verify = true;
		}
		else
		{
			_animPlayer.Play("player_close");
			verify = false;
			
		}
	}

	private void BuyStats(int index)
	{
		_buttonSounds.Play();
		GD.Print("Analyzed!");
		switch(index)
		{
			case 0:
						StatsUpdated(index);
						GD.Print("Speed");
			break;
			case 1:
						StatsUpdated(index);
						GD.Print("FireRate");
			break;
			case 2:
						StatsUpdated(index);
						GD.Print("Health");
			break;
			case 3:
						StatsUpdated(index);
						GD.Print("Projectile");
			break;
			case 4:
						StatsUpdated(index);
						GD.Print("Damage");
			break;
		}

	}

	private void StatsUpdated(int index)
	{
		if(_statsPrices[index] <= _currentCoins)
				{
					_currentCoins -= _statsPrices[index];
					_statsButtons[index].Text = $"{_upgradeTimesStats[index]}+";
					_upgradeTimesStats[index] += 1;
					_coinsLabel.Text = $"{_currentCoins}";
					TriggerSave();
				}
	}

	private void ChangeSceneSafe(string path)
<<<<<<< Updated upstream
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
=======
	{
		var scene = GD.Load<PackedScene>(path);
		if (scene != null)
		{
			GetTree().ChangeSceneToPacked(scene);
		}
		else
		{
			GD.PrintErr($"Failed to load: {path}");
		}
	}
	private void ToggleUpdateStats(int index)
	{
		switch (index)
		{
			default:
				ToggleStatsPanel();
				break;
			case 0:
				_passiveButtons[index].GrabFocus();
				break;
			case 1:
				_passiveButtons[index].GrabFocus();
				break;
			case 2:
				_passiveButtons[index].GrabFocus();
				break;
			case 3:
				_passiveButtons[index].GrabFocus();
				break;
			case 4:
				_passiveButtons[index].GrabFocus();
				break;
			case 5:
				_passiveButtons[index].GrabFocus();
				break;

				
		}
	}
>>>>>>> Stashed changes
}
