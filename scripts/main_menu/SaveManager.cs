using Godot;
using System;
using System.Linq;

public partial class SaveManager : Node
{
	private const string SavePath = "user://savegame.tres";

	public void SaveGame(int scraps, int coins)
	{
		SaveData currentData = LoadGame();

		currentData._scrapsTotal += scraps;
		currentData._coinsTotal += coins;
		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print($"[SaveManager] Run saved: +{scraps} scraps, +{coins} coins. Total: {currentData._scrapsTotal} scraps, {currentData._coinsTotal} coins.");
		}
	}

	public void SaveGameMenu(int scraps, Godot.Collections.Array<bool> _weaponUnlockedArray, int coins, Godot.Collections.Array<int> _upgradeTimes, int equippedPassive = 0)
	{
		SaveData currentData = LoadGame();

		currentData._statsRepeatUpgrades = _upgradeTimes;
		currentData._weaponUnlocked = _weaponUnlockedArray;
		currentData._scrapsTotal = scraps;
		currentData._coinsTotal = coins;
		currentData._equippedPassive = equippedPassive;
		
		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("[SaveManager] Menu data saved!");
		}
	}

	public void SavedWeapon(int weapon)
	{
		SaveData currentData = LoadGame();
		currentData._weaponPick = weapon;

		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("[SaveManager] Equipped weapon saved!");
		}
	}

	public void SavedArea(string area)
	{
		SaveData currentData = LoadGame();
		currentData._areaChoosed = area;

		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("[SaveManager] Area selection saved!");
		}
	}

	public void SaveEquippedPassive(int passiveIndex)
	{
		SaveData currentData = LoadGame();
		currentData._equippedPassive = passiveIndex;
		ResourceSaver.Save(currentData, SavePath);
		GD.Print($"[SaveManager] Equipped passive saved: index {passiveIndex}");
	}

	public void SaveCharacter(int characterIndex)
	{
		SaveData currentData = LoadGame();
		currentData._selectedCharacter = characterIndex;
		ResourceSaver.Save(currentData, SavePath);
		GD.Print($"[SaveManager] Character saved: index {characterIndex}");
	}

	public void SaveInfiniteMode(bool enabled)
	{
		SaveData currentData = LoadGame();
		currentData._infiniteMode = enabled;
		ResourceSaver.Save(currentData, SavePath);
		GD.Print($"[SaveManager] Infinite mode saved: {enabled}");
	}

	// public bool UnlockAchievement(int index)
	// {
	// 	if (index < 0 || index >= 4) return false;
	// 	SaveData currentData = LoadGame();
	// 	if (currentData._achievements != null && currentData._achievements.Count > index && currentData._achievements[index])
	// 	{
	// 		return false; // Already unlocked
	// 	}

	// 	while (currentData._achievements == null || currentData._achievements.Count < 4)
	// 	{
	// 		if (currentData._achievements == null) currentData._achievements = new Godot.Collections.Array<bool>();
	// 		currentData._achievements.Add(false);
	// 	}

	// 	currentData._achievements[index] = true;
	// 	ResourceSaver.Save(currentData, SavePath);
	// 	GD.Print($"[SaveManager] Achievement #{index} unlocked and saved!");
	// 	return true;
	// }

	public SaveData LoadGame()
	{
		SaveData data = null;
		if (FileAccess.FileExists(SavePath))
		{
			data = ResourceLoader.Load<SaveData>(SavePath);
			GD.Print("[SaveManager] Game loaded!");
		}

		if (data == null)
		{
			GD.Print("[SaveManager] No valid save file found. Initializing new SaveData.");
			data = new SaveData();
		}

		// Guarantee arrays and fields are never null or invalid
		if (data._weaponUnlocked == null || data._weaponUnlocked.Count < 5)
		{
			data._weaponUnlocked = new Godot.Collections.Array<bool> { true, false, false, false, false };
		}

		// 6 passive upgrades: 0:Speed, 1:FireRate, 2:Health, 3:Projectile, 4:Damage, 5:Dash
		if (data._statsRepeatUpgrades == null)
		{
			data._statsRepeatUpgrades = new Godot.Collections.Array<int> { 0, 0, 0, 0, 0, 0 };
		}
		else
		{
			while (data._statsRepeatUpgrades.Count < 6)
			{
				data._statsRepeatUpgrades.Add(0);
			}
		}

		if (data._achievements == null)
		{
			data._achievements = new Godot.Collections.Array<bool> { false, false, false, false };
		}
		else
		{
			while (data._achievements.Count < 4)
			{
				data._achievements.Add(false);
			}
		}

		if (data._equippedPassive < 0 || data._equippedPassive > 5)
		{
			data._equippedPassive = 0;
		}

		if (data._weaponPick < 1 || data._weaponPick > 5)
		{
			data._weaponPick = 1;
		}

		if (string.IsNullOrEmpty(data._areaChoosed) || !data._areaChoosed.StartsWith("res://scene/levels/"))
		{
			data._areaChoosed = "res://scene/levels/area.tscn";
		}

		if (data._selectedCharacter < 0 || data._selectedCharacter > 1)
		{
			data._selectedCharacter = 0;
		}

		return data;
	}
}
