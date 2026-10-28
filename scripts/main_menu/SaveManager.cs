using Godot;
using System;
using System.Linq;

public partial class SaveManager : Node
{
	private const string SavePath = "user://savegame.tres";
	private int _localScrapsAdd;

	public void SaveGame(int scraps, int coins)
	{

		SaveData currentData = LoadGame();

		currentData._scrapsTotal += scraps;
		currentData._coinsTotal += coins;
		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("Game saved!");
		}
	}
	public void SaveGameMenu(int scraps, Godot.Collections.Array<bool> _weaponUnlockedArray, int coins, Godot.Collections.Array<int> _upgradeTimes)
	{

		SaveData currentData = LoadGame();

		currentData._statsRepeatUpgrades = _upgradeTimes;
		currentData._weaponUnlocked = _weaponUnlockedArray;
		currentData._scrapsTotal = scraps;
		currentData._coinsTotal = coins;
		
		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("Game saved!");
		}
	}

	public void SavedWeapon(int weapon)
	{
		SaveData currentData = LoadGame();
		currentData._weaponPick = weapon;

		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("Game saved!");
		}
	}

	public void SavedArea(string area)
	{
		SaveData currentData = LoadGame();
		currentData._areaChoosed = area;

		Error error = ResourceSaver.Save(currentData, SavePath);
		if (error == Error.Ok)
		{
			GD.Print("Game saved!");
		}
	}

	public SaveData LoadGame()
	{
		SaveData data = null;
		if (FileAccess.FileExists(SavePath))
		{
			data = ResourceLoader.Load<SaveData>(SavePath);
			GD.Print("Game loaded!");
		}

		if (data == null)
		{
			GD.Print("No valid save file found. Initializing new SaveData.");
			data = new SaveData();
		}

		// Guarantee arrays and fields are never null or invalid
		if (data._weaponUnlocked == null || data._weaponUnlocked.Count < 5)
		{
			data._weaponUnlocked = new Godot.Collections.Array<bool> { true, false, false, false, false };
		}

		if (data._statsRepeatUpgrades == null || data._statsRepeatUpgrades.Count < 5)
		{
			data._statsRepeatUpgrades = new Godot.Collections.Array<int> { 0, 0, 0, 0, 0 };
		}

		if (data._weaponPick < 1 || data._weaponPick > 5)
		{
			data._weaponPick = 1;
		}

		if (string.IsNullOrEmpty(data._areaChoosed))
		{
			data._areaChoosed = "res://scene/levels/area.tscn";
		}

		return data;
	}
}
