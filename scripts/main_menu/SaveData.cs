using Godot;
using System;

public partial class SaveData : Resource
{
	[Export] public int _scrapsTotal { get; set; } = 0;
	[Export] public Godot.Collections.Array<bool> _weaponUnlocked { get; set; } = new Godot.Collections.Array<bool> { true, false, false, false, false };
	[Export] public Godot.Collections.Array<int> _statsRepeatUpgrades { get; set; } = new Godot.Collections.Array<int> { 0, 0, 0, 0, 0 };
	[Export] public int _coinsTotal { get; set; } = 0;
	[Export] public int _weaponPick { get; set; } = 1;
	[Export] public string _areaChoosed { get; set; } = "res://scene/levels/area.tscn";
}
