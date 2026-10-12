using Godot;
using System;

public partial class WeaponChooser : Control
{
	Button _next;
	Button _prev;
	Button _back;
	Button _proceedArea;
	TextureRect _weaponPrev;
	TextureRect _weaponMain;
	TextureRect _weaponNext;

	public string[] _weaponPaths = new string[]
	{
	"res://assets/PIXWEP/body/body_laser_01.png",
	"res://assets/PIXWEP/body/body_laser_02.png",
	"res://assets/PIXWEP/body/body_plasma_01.png",
	"res://assets/PIXWEP/body/body_plasma_02.png",
	"res://assets/PIXWEP/body/body_projectile_01.png"
	};

	//for null weapons
	private String _Weapon0 = "res://assets/Encylopedia/weapons.png";
	private String _Weapon6 = "res://assets/Encylopedia/weapons.png";

	private int count = 2;
	private String _weaponStringMain = "";
	private String _weaponStringPrev = "";
	private String _weaponStringNext = "";

	private string BackScene = "res://scene/main_menu/main_menu.tscn";
	private string AreaChooserScene = "res://scene/main_menu/area_chooser.tscn";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_next = GetNode<Button>("PanelContainer/HBoxContainer/Next");
		_prev = GetNode<Button>("PanelContainer/HBoxContainer/Prev");
		_back = GetNode<Button>("PanelContainer/HBoxContainer/VBoxContainer/HBoxContainer2/Back");
		_proceedArea = GetNode<Button>("PanelContainer/HBoxContainer/VBoxContainer/HBoxContainer2/PlayScene");

		_weaponMain = GetNode<TextureRect>("PanelContainer/HBoxContainer/VBoxContainer/HBoxContainer/MainWeapon");
		_weaponPrev = GetNode<TextureRect>("PanelContainer/HBoxContainer/VBoxContainer/HBoxContainer/PrevWeapon");
		_weaponNext = GetNode<TextureRect>("PanelContainer/HBoxContainer/VBoxContainer/HBoxContainer/NextWeapon");

		_next.Pressed += NextWeaponFunc;
		_prev.Pressed += PrevWeaponFunc;
		_back.Pressed += BackHome;
		_proceedArea.Pressed += ProceedArea;
		NextWeapon();
	}

	private void NextWeapon()
	{
		if (count >= 1 && count <= 5)
		{
			_weaponMain.Texture = GD.Load<Texture2D>(_weaponPaths[count]);
			_weaponNext.Texture = GD.Load<Texture2D>(_weaponPaths[count + 1]);
			_weaponPrev.Texture = GD.Load<Texture2D>(_weaponPaths[count - 1]);
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

	private void BackHome()
	{
		GetTree().ChangeSceneToFile(BackScene);
	}

	private void ProceedArea()
	{
		GetTree().ChangeSceneToFile(AreaChooserScene);
	}
}
