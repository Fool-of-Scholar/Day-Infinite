using Godot;
using System;

public partial class SaveData : Resource
{
	[Export] public int _scrapsTotal { get; set; } = 0;
	[Export] public Godot.Collections.Array<bool> _weaponUnlocked { get; set; } = new Godot.Collections.Array<bool> { true, false, false, false, false };
	// 0: Speed, 1: FireRate, 2: Health, 3: Projectile, 4: Damage, 5: Dash (levels 0 to 5)
	[Export] public Godot.Collections.Array<int> _statsRepeatUpgrades { get; set; } = new Godot.Collections.Array<int> { 0, 0, 0, 0, 0, 0 };
	// Index of the single passive equipped for gameplay (0 to 5)
	[Export] public int _equippedPassive { get; set; } = 0;
	[Export] public int _coinsTotal { get; set; } = 0;
	[Export] public int _weaponPick { get; set; } = 1;
	[Export] public string _areaChoosed { get; set; } = "res://scene/levels/area.tscn";
	// 0: Escaped (10 min in Area 1), 1: Lost in Wilderness (20 min in Area 2), 2: No Damage, 3: Mania (4 weapons)
	[Export] public Godot.Collections.Array<bool> _achievements { get; set; } = new Godot.Collections.Array<bool> { false, false, false, false };
	// 0: Null Beta (default), 1: DMO
	[Export] public int _selectedCharacter { get; set; } = 0;
	// Infinite mode: timer replaced by ∞
	[Export] public bool _infiniteMode { get; set; } = false;
}
