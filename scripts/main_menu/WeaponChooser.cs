using Godot;
using System;
using System.Linq;

public partial class WeaponChooser : Control
{
	private string _sceneString;
	private SaveManager _saveManager;
	private int _currentScraps;
	private int _currentCoins;
	private AnimationPlayer _animPlayer;

	// Top UI Labels & Controls
	private Label _scrapsLabel;
	private Label _coinsLabel;
	private Label _weaponPriceLabel;
	private Button _next;
	private Button _prev;
	private Button _back;
	private Button _playerStats;
	private Button _proceedArea;
	private Button _buyButton;
	private TextureRect _weaponPrev;
	private TextureRect _weaponMain;
	private TextureRect _weaponNext;
	private Label _weaponName;
	private Label _weaponStats;
	private Panel _lockedManager;
	private AudioStreamPlayer _buttonSounds;
	private bool _isStatsPanelOpen = false;

	// Passive Upgrades System (6 passives: Speed, FireRate, Health, Projectile, Damage, Dash)
	private Button[] _passiveButtons;
	private Label _passiveTitle;
	private Label _passiveLevel;
	private Label _passiveDesc;
	private Button _passiveEquipBtn;
	private Button _passiveUpgradeBtn;

	private int[] _upgradeTimesStats = new int[6] { 0, 0, 0, 0, 0, 0 };
	private int _equippedPassive = 0;
	private int _selectedPassiveIndex = 0;

	public readonly int[] PassiveUpgradeCosts = new int[]
	{
		40,   // Level 1
		80,   // Level 2
		130,  // Level 3
		190,  // Level 4
		260   // Level 5
	};

	public readonly string[] PassiveNames = new string[]
	{
		"Speed Boost",
		"Rapid Fire",
		"Max Health",
		"Extra Projectiles",
		"Raw Damage",
		"Dash Cooldown"
	};

	public readonly string[] PassiveShortNames = new string[]
	{
		"Speed",
		"Fire Rate",
		"Health",
		"Projectile",
		"Damage",
		"Dash"
	};

	public readonly string[] _weaponStringModifierDesc = new string[]
	{
		"Fast, accurate weapon that automatically tracks and fires toward enemies\n",
		"Heavy, high-impact weapon that destroy horde and tanky enemies\n",
		"Instant lightning discharge that penetrate toward enemies\n",
		"Ground Trap that release near Player, dealing most highest burst damage\n",
		"Protective Shield surrounding the player, knockblast enemies and protect you \n"
	};
	public readonly string[] _weaponStringModifierName = new string[]
	{
		"Assault Rifle (AR)",
		"Rocket Launcher",
		"Tesla Burst",
		"Landmine",
		"ForceField"
	};
	public readonly string[] _weaponStringModifierStats = new string[]
	{
		"Damage: 15 per bullet\nFire Rate: 0.5s\nProjectile Speed: 900\nMax Range: 800",
		"Damage: 90 explosion damage\nFire Rate: 2.0s\nProjectile Speed: 380\nMax Range: 900",
		"Damage: 50 per beam strike\nFire Rate: 1.2s\nChain Range: 380\nDeliver: Instantaneous line of Lightning",
		"Damage: 120 detonation burst\nCooldown: 3.5s\nTrap Lifetime: 12.0s\n",
		"Damage: 25 contact damage\nKnockback Force: 400 impulse pushing enemies away\nCooldown: 2.0s\n",
	};
	public readonly string[] _weaponPaths = new string[]
	{
		"res://assets/PIXWEP/desert/body/body_laser_01.png",//stopper
		"res://assets/PIXWEP/body/body_laser_01.png",//1
		"res://assets/PIXWEP/body/body_laser_02.png",
		"res://assets/PIXWEP/body/body_plasma_01.png",
		"res://assets/Landmines/mine (157).png",
		"res://assets/forcefieldblue/forcefield.png",//5
		"res://assets/PIXWEP/desert/body/body_laser_01.png"//stopper
	};

	public readonly int[] _weaponPrices = new int[]
	{
		10,
		50,
		60,
		70,
		80
	};

	public bool[] _lockManagerWeapon = new bool[]
	{
		true,
		false,
		false,
		false,
		false
	};

	private int count = 1;

	public override void _Ready()
	{
		_saveManager = GetNodeOrNull<SaveManager>("/root/SaveManager");
		_buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_coinsLabel = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/Cash/Coins/MarginContainer/HBoxContainer/Label");
		_scrapsLabel = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/Cash/Scraps/MarginContainer/HBoxContainer/Label");

		_weaponPriceLabel = GetNode<Label>("PanelContainer/Locked/PriceLabel");
		_buyButton = GetNode<Button>("PanelContainer/Locked/Buy");
		_playerStats = GetNode<Button>("VBoxContainer/HBoxContainer2/Player");
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
		_playerStats.Pressed += ToggleStatsPanel;

		// --- Setup Passive Upgrades UI ---
		var grid = GetNodeOrNull<GridContainer>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer/OptionsGrid");
		if (grid != null)
		{
			_passiveButtons = grid.GetChildren().OfType<Button>().ToArray();
			for (int i = 0; i < _passiveButtons.Length; i++)
			{
				int index = i;
				_passiveButtons[i].Pressed += () => SelectPassive(index);
			}
		}

		_passiveTitle = GetNodeOrNull<Label>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer/RightInfo/PassiveTitle");
		_passiveLevel = GetNodeOrNull<Label>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer/RightInfo/PassiveLevel");
		_passiveDesc = GetNodeOrNull<Label>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer/RightInfo/PassiveDesc");
		_passiveEquipBtn = GetNodeOrNull<Button>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer/RightInfo/ActionButtons/EquipButton");
		_passiveUpgradeBtn = GetNodeOrNull<Button>("PlayerPassive/PanelContainer/MarginContainer/HBoxContainer/RightInfo/ActionButtons/UpgradeButton");

		if (_passiveEquipBtn != null) _passiveEquipBtn.Pressed += OnEquipPassivePressed;
		if (_passiveUpgradeBtn != null) _passiveUpgradeBtn.Pressed += OnUpgradePassivePressed;

		_animPlayer.AnimationFinished += BottomButtons;
		_lockedManager.Visible = false;

		TriggerLoad();
		NextWeapon();
		RefreshPassiveUI();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.D || keyEvent.Keycode == Key.Right)
			{
				NextWeaponFunc();
			}
			else if (keyEvent.Keycode == Key.A || keyEvent.Keycode == Key.Left)
			{
				PrevWeaponFunc();
			}
		}
	}

	private void NextWeapon()
	{
		_buttonSounds?.Play();
		if (count >= 1 && count <= 5)
		{
			_weaponMain.Texture = GD.Load<Texture2D>(_weaponPaths[count]);
			_weaponNext.Texture = GD.Load<Texture2D>(_weaponPaths[count + 1]);
			_weaponPrev.Texture = GD.Load<Texture2D>(_weaponPaths[count - 1]);

			_weaponName.Text = _weaponStringModifierName[count - 1];
			_weaponStats.Text = _weaponStringModifierDesc[count - 1] + _weaponStringModifierStats[count - 1];

			_weaponPriceLabel.Text = $"Price: {_weaponPrices[count - 1]} Scraps";

			if (!_lockManagerWeapon[count - 1])
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
		if (count <= 4)
		{
			count += 1;
			NextWeapon();
		}
	}

	private void PrevWeaponFunc()
	{
		if (count >= 2)
		{
			count -= 1;
			NextWeapon();
		}
	}

	private void BottomButtons(StringName animName)
	{	
		if (animName == "back")
		{
			_sceneString = "res://scene/main_menu/main_menu.tscn";
			LoadingScreen.NextScenePath = _sceneString;
			TriggerSave();
			CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
		}
		else if (animName == "next")
		{
			_sceneString = "res://scene/main_menu/area_chooser.tscn";
			LoadingScreen.NextScenePath = _sceneString;
			TriggerSave();
			CallDeferred(nameof(ChangeSceneSafe), "res://scene/main_menu/loading_screen.tscn");
		}
	}

	private void BackHome()
	{
		_buttonSounds?.Play();
		_animPlayer.Play("back");
	}

	private void ProceedAnim()
	{
		if (_proceedArea != null && _proceedArea.Disabled) return;
		if (_proceedArea != null) _proceedArea.Disabled = true;
		_saveManager?.SavedWeapon(count);
		_buttonSounds?.Play();
		_animPlayer.Play("next");
	}

	public void TriggerSave()
	{
		if (_saveManager != null)
		{
			_saveManager.SaveGameMenu(
				_currentScraps,
				new Godot.Collections.Array<bool>(_lockManagerWeapon),
				_currentCoins,
				new Godot.Collections.Array<int>(_upgradeTimesStats),
				_equippedPassive
			);
		}
	}

	public void TriggerLoad()
	{
		if (_saveManager != null)
		{
			SaveData loadedData = _saveManager.LoadGame();

			var rawStats = loadedData._statsRepeatUpgrades.ToArray();
			for (int i = 0; i < 6; i++)
			{
				_upgradeTimesStats[i] = i < rawStats.Length ? Mathf.Clamp(rawStats[i], 0, 5) : 0;
			}

			_equippedPassive = Mathf.Clamp(loadedData._equippedPassive, 0, 5);
			_lockManagerWeapon = loadedData._weaponUnlocked.ToArray();
			_currentScraps = loadedData._scrapsTotal;
			_currentCoins = loadedData._coinsTotal;

			if (_scrapsLabel != null) _scrapsLabel.Text = $"{_currentScraps}";
			if (_coinsLabel != null) _coinsLabel.Text = $"{_currentCoins}";
		}
	}

	private void BuyWeapon()
	{
		if (count >= 1 && count <= 5)
		{
			int price = _weaponPrices[count - 1];
			if (_currentScraps >= price)
			{
				_currentScraps -= price;
				_lockManagerWeapon[count - 1] = true;
				TriggerSave();
				if (_scrapsLabel != null) _scrapsLabel.Text = $"{_currentScraps}";

				_proceedArea.Visible = true;
				_lockedManager.Visible = false;
				_buttonSounds?.Play();
			}
			else
			{
				_weaponPriceLabel.Text = "Not enough scraps!!";
			}
		}
	}

	// --- Passive Upgrade Logic ---

	private void ToggleStatsPanel()
	{
		_buttonSounds?.Play();
		if (!_isStatsPanelOpen)
		{
			_animPlayer.Play("player_open");
			_isStatsPanelOpen = true;
			RefreshPassiveUI();
		}
		else
		{
			_animPlayer.Play("player_close");
			_isStatsPanelOpen = false;
		}
	}

	private void SelectPassive(int index)
	{
		if (index < 0 || index >= 6) return;
		_selectedPassiveIndex = index;
		_buttonSounds?.Play();
		RefreshPassiveUI();
	}

	private void OnEquipPassivePressed()
	{
		_equippedPassive = _selectedPassiveIndex;
		_buttonSounds?.Play();
		TriggerSave();
		RefreshPassiveUI();
		GD.Print($"[WeaponChooser] Equipped passive: {PassiveNames[_equippedPassive]}");
	}

	private void OnUpgradePassivePressed()
	{
		int curLvl = _upgradeTimesStats[_selectedPassiveIndex];
		if (curLvl >= 5) return;

		int cost = PassiveUpgradeCosts[curLvl];
		if (_currentCoins >= cost)
		{
			_currentCoins -= cost;
			_upgradeTimesStats[_selectedPassiveIndex] = curLvl + 1;

			// If nothing was equipped or this is currently equipped, ensure it stays equipped
			if (_equippedPassive == _selectedPassiveIndex || _upgradeTimesStats[_equippedPassive] == 0)
			{
				_equippedPassive = _selectedPassiveIndex;
			}

			if (_coinsLabel != null) _coinsLabel.Text = $"{_currentCoins}";
			_buttonSounds?.Play();
			TriggerSave();
			RefreshPassiveUI();
			GD.Print($"[WeaponChooser] Upgraded {PassiveNames[_selectedPassiveIndex]} to Level {_upgradeTimesStats[_selectedPassiveIndex]}/5");
		}
	}

	private void RefreshPassiveUI()
	{
		if (_passiveButtons != null)
		{
			for (int i = 0; i < _passiveButtons.Length && i < 6; i++)
			{
				int lvl = _upgradeTimesStats[i];
				bool isEquipped = (i == _equippedPassive);
				string tag = isEquipped ? "[E] " : "";
				string lvlText = (lvl >= 5) ? "MAX" : $"{lvl}/5";
				_passiveButtons[i].Text = $"{tag}{PassiveShortNames[i]}\n{lvlText}";

				if (isEquipped)
				{
					_passiveButtons[i].Modulate = new Color(0.4f, 1.0f, 0.6f, 1f); // Highlight green
				}
				else if (i == _selectedPassiveIndex)
				{
					_passiveButtons[i].Modulate = new Color(0.5f, 0.85f, 1.0f, 1f); // Selected blue
				}
				else
				{
					_passiveButtons[i].Modulate = new Color(1f, 1f, 1f, 1f);
				}
			}
		}

		int selectedLvl = _upgradeTimesStats[_selectedPassiveIndex];
		bool isSelectedEquipped = (_selectedPassiveIndex == _equippedPassive);

		if (_passiveTitle != null)
		{
			_passiveTitle.Text = PassiveNames[_selectedPassiveIndex];
		}

		if (_passiveLevel != null)
		{
			string equipStatus = isSelectedEquipped ? "★ [E] EQUIPPED FOR RUN ★" : "[NOT EQUIPPED]";
			string maxTag = selectedLvl >= 5 ? " (MAX)" : "";
			_passiveLevel.Text = $"Level: {selectedLvl}/5{maxTag}  {equipStatus}";
			_passiveLevel.Modulate = isSelectedEquipped ? new Color(0.3f, 1f, 0.5f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
		}

		if (_passiveDesc != null)
		{
			_passiveDesc.Text = GetPassiveDescription(_selectedPassiveIndex, selectedLvl);
		}

		if (_passiveEquipBtn != null)
		{
			if (isSelectedEquipped)
			{
				_passiveEquipBtn.Text = "[E] EQUIPPED";
				_passiveEquipBtn.Disabled = true;
			}
			else
			{
				_passiveEquipBtn.Text = "EQUIP [E]";
				_passiveEquipBtn.Disabled = false;
			}
		}

		if (_passiveUpgradeBtn != null)
		{
			if (selectedLvl >= 5)
			{
				_passiveUpgradeBtn.Text = "MAX LEVEL (5/5)";
				_passiveUpgradeBtn.Disabled = true;
			}
			else
			{
				int cost = PassiveUpgradeCosts[selectedLvl];
				_passiveUpgradeBtn.Text = $"UPGRADE ({cost} Coins)";
				_passiveUpgradeBtn.Disabled = (_currentCoins < cost);
			}
		}
	}

	private string GetPassiveDescription(int index, int level)
	{
		switch (index)
		{
			case 0:
				return $"Increases base player movement speed.\n\n" +
				       $"• Current: +{level * 15} Speed\n" +
				       (level < 5 ? $"• Next Level (Lvl {level + 1}): +{(level + 1) * 15} Speed" : "• MAX LEVEL REACHED!");
			case 1:
				return $"Increases firing speed across all weapons.\n\n" +
				       $"• Current: +{level * 10}% Fire Rate\n" +
				       (level < 5 ? $"• Next Level (Lvl {level + 1}): +{(level + 1) * 10}% Fire Rate" : "• MAX LEVEL REACHED!");
			case 2:
				return $"Increases maximum character health.\n\n" +
				       $"• Current: +{level * 25} Max HP\n" +
				       (level < 5 ? $"• Next Level (Lvl {level + 1}): +{(level + 1) * 25} Max HP" : "• MAX LEVEL REACHED!");
			case 3:
				int curProj = (level + 1) / 2;
				int nextProj = (level + 2) / 2;
				return $"Fires additional projectiles with each shot.\n\n" +
				       $"• Current: +{curProj} Extra Projectile{(curProj == 1 ? "" : "s")}\n" +
				       (level < 5 ? $"• Next Level (Lvl {level + 1}): +{nextProj} Extra Projectile{(nextProj == 1 ? "" : "s")}" : "• MAX LEVEL REACHED!");
			case 4:
				return $"Increases projectile and explosion impact damage.\n\n" +
				       $"• Current: +{level * 8} Bonus Damage\n" +
				       (level < 5 ? $"• Next Level (Lvl {level + 1}): +{(level + 1) * 8} Bonus Damage" : "• MAX LEVEL REACHED!");
			case 5:
				float curCooldown = Mathf.Max(2.0f, 5.0f - (level * 0.6f));
				float nextCooldown = Mathf.Max(2.0f, 5.0f - ((level + 1) * 0.6f));
				return $"Reduces Dash ability cooldown and grants bonus sprint speed.\n\n" +
				       $"• Current: {curCooldown:F1}s Cooldown, +{level * 25} Dash Speed\n" +
				       (level < 5 ? $"• Next Level (Lvl {level + 1}): {nextCooldown:F1}s Cooldown, +{(level + 1) * 25} Dash Speed" : "• MAX LEVEL REACHED!");
			default:
				return "";
		}
	}

	private void ChangeSceneSafe(string path)
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
}
