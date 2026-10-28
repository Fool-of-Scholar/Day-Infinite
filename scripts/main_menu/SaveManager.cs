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
		if (!FileAccess.FileExists(SavePath))
		{
			GD.Print("No save file found.");
			return new SaveData();
		}

		SaveData data = ResourceLoader.Load<SaveData>(SavePath);
		GD.Print("Game loaded!");
		return data;
	}
}
