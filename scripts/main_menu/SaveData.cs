using Godot;
using System;

public partial class SaveData : Resource
{
	[Export] public int _scrapsTotal {get; set;} = 0;
	[Export] public Godot.Collections.Array<bool> _weaponUnlocked;
	[Export] public Godot.Collections.Array<int> _statsRepeatUpgrades;
	[Export] public int _coinsTotal;
	[Export] public int _weaponPick;
	[Export] public string _areaChoosed;
}
