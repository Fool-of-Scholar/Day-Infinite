using Godot;
using System;

public partial class WeaponChooser : Control
{
	AnimationPlayer _animPlayer;
	Button _next;
	Button _prev;
	Button _back;
	Button _proceedArea;
	TextureRect _weaponPrev;
	TextureRect _weaponMain;
	TextureRect _weaponNext;
	Label _weaponName;
	Label _weaponStats;
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

	//for null weapons
	private String _Weapon0 = "res://assets/Encylopedia/weapons.png";
	private String _Weapon6 = "res://assets/Encylopedia/weapons.png";

	private int count = 1;
	private String _weaponStringMain = "";
	private String _weaponStringPrev = "";
	private String _weaponStringNext = "";

	private string BackScene = "res://scene/main_menu/main_menu.tscn";
	private string AreaChooserScene = "res://scene/main_menu/area_chooser.tscn";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_next = GetNode<Button>("PanelContainer/HBoxContainer/Next");
		_prev = GetNode<Button>("PanelContainer/HBoxContainer/Prev");
		_back = GetNode<Button>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer2/Back");
		_proceedArea = GetNode<Button>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer2/PlayScene");

		_weaponName = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/WeaponName");
		_weaponStats = GetNode<Label>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/Details");

		_weaponMain = GetNode<TextureRect>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/MainWeapon");
		_weaponPrev = GetNode<TextureRect>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/PrevWeapon");
		_weaponNext = GetNode<TextureRect>("PanelContainer/HBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/NextWeapon");

		_next.Pressed += NextWeaponFunc;
		_prev.Pressed += PrevWeaponFunc;
		_back.Pressed += BackHome;
		_proceedArea.Pressed += ProceedAnim;
		NextWeapon();

		_animPlayer.AnimationFinished += BottomButtons;
	}

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Pressed && keyEvent.Keycode == Key.D || keyEvent.Keycode == Key.Right)
			{
				NextWeaponFunc();
			}
			if (keyEvent.Pressed && keyEvent.Keycode == Key.A || keyEvent.Keycode == Key.Left)
			{
				PrevWeaponFunc();
			}
		}
  }


	private void NextWeapon()
	{
		if (count >= 1 && count <= 5)
		{
			_weaponMain.Texture = GD.Load<Texture2D>(_weaponPaths[count]);
			_weaponNext.Texture = GD.Load<Texture2D>(_weaponPaths[count + 1]);
			_weaponPrev.Texture = GD.Load<Texture2D>(_weaponPaths[count - 1]);

			_weaponName.Text = _weaponStringModifierName[count - 1];
			_weaponStats.Text = _weaponStringModifierDesc[count - 1] + _weaponStringModifierStats[count - 1];
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
		GetTree().ChangeSceneToFile(BackScene);
		}
		else
		{
			
		GetTree().ChangeSceneToFile(AreaChooserScene);
		}
	}

	private void BackHome()
	{
		_animPlayer.Play("back");
	}

	private void ProceedAnim()
	{
		_animPlayer.Play("next");
	}
	


}
