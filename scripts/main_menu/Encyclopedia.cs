using Godot;
using System;

public partial class Encyclopedia : Control
{
	Button _back;
	Button _weapons;
	Button _upgrades;
	Button _lores;
	AnimationPlayer _animPlayer;
	PanelContainer _loreBox;
	PanelContainer _upgradesBox;
	PanelContainer _weaponsBox;

	AudioStreamPlayer _buttonSounds;
	
	public override void _Ready()
	{
		_buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");
		GD.Print("Emcyclopedia Initialized");
		_back = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Back");
		_weapons = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Weapon");
		_upgrades = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Upgrades");
		_lores = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Lores");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_loreBox = GetNode<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Lore");
		_upgradesBox = GetNode<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Upgrades");
		_weaponsBox = GetNode<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Weapons");

		_back.Pressed += BackButton;
		_weapons.Pressed += Weapons;
		_upgrades.Pressed += Upgrades;
		_lores.Pressed += Lores; 

		_weaponsBox.Visible = true;
		_loreBox.Visible = false;
		_upgradesBox.Visible = false;

		GD.Print("Emcyclopedia Initialized");
	}


	private void BackButton()
	{
		_buttonSounds.Play();
		_animPlayer.Play("back");
		_animPlayer.AnimationFinished += Back;
	}

	private void Back(StringName back)
	{
		var _main = GD.Load<PackedScene>("res://scene/main_menu/main_menu.tscn");

		GetTree().ChangeSceneToPacked(_main);
		
	}

	private void Weapons()
	{
		_buttonSounds.Play();
		_animPlayer.Play("next");
		_weaponsBox.Visible = true;
		_loreBox.Visible = false;
		_upgradesBox.Visible = false;
	}

	private void Upgrades()
	{
		_buttonSounds.Play();
		_animPlayer.Play("next");
		_weaponsBox.Visible = false;
		_loreBox.Visible = false;
		_upgradesBox.Visible = true;
	}

	private void Lores()
	{
		_buttonSounds.Play();
		_animPlayer.Play("next");
		_weaponsBox.Visible = false;
		_loreBox.Visible = true;
		_upgradesBox.Visible = false;
	}
}
