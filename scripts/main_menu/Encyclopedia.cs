using Godot;
using System;

public partial class Encyclopedia : Control
{
	Button _back;
	Button _weapons;
	Button _upgrades;
	Button _lores;
	Button _achievementsBtn;
	AnimationPlayer _animPlayer;
	PanelContainer _loreBox;
	PanelContainer _upgradesBox;
	PanelContainer _weaponsBox;
	PanelContainer _achievementsBox;

	AudioStreamPlayer _buttonSounds;
	
	public override void _Ready()
	{
		_buttonSounds = GetNode<AudioStreamPlayer>("ButtonSound");
		GD.Print("Encyclopedia Initialized");
		_back = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Back");
		_weapons = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Weapon");
		_upgrades = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Upgrades");
		_lores = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Lores");
		_achievementsBtn = GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/Achievements");
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_loreBox = GetNode<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Lore");
		_upgradesBox = GetNode<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Upgrades");
		_weaponsBox = GetNode<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Weapons");
		_achievementsBox = GetNodeOrNull<PanelContainer>("PanelContainer/MarginContainer/VBoxContainer/Achievements");

		_back.Pressed += BackButton;
		_weapons.Pressed += Weapons;
		_upgrades.Pressed += Upgrades;
		_lores.Pressed += Lores; 
		if (_achievementsBtn != null)
		{
			_achievementsBtn.Pressed += OnAchievementsPressed;
		}

		_weaponsBox.Visible = true;
		_loreBox.Visible = false;
		_upgradesBox.Visible = false;
		if (_achievementsBox != null)
		{
			_achievementsBox.Visible = false;
		}

		// RefreshAchievements();
		GD.Print("Encyclopedia Initialized");
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
		if (_achievementsBox != null) _achievementsBox.Visible = false;
	}

	private void Upgrades()
	{
		_buttonSounds.Play();
		_animPlayer.Play("next");
		_weaponsBox.Visible = false;
		_loreBox.Visible = false;
		_upgradesBox.Visible = true;
		if (_achievementsBox != null) _achievementsBox.Visible = false;
	}

	private void Lores()
	{
		_buttonSounds.Play();
		_animPlayer.Play("next");
		_weaponsBox.Visible = false;
		_loreBox.Visible = true;
		_upgradesBox.Visible = false;
		if (_achievementsBox != null) _achievementsBox.Visible = false;
	}

	private void OnAchievementsPressed()
	{
		_buttonSounds.Play();
		_animPlayer.Play("next");
		_weaponsBox.Visible = false;
		_loreBox.Visible = false;
		_upgradesBox.Visible = false;
		if (_achievementsBox != null)
		{
			_achievementsBox.Visible = true;
			// RefreshAchievements();
		}
	}

	// private void RefreshAchievements()
	// {
	// 	if (_achievementsBox == null) return;

	// 	SaveManager sm = new SaveManager();
	// 	SaveData data = sm.LoadGame();

	// 	for (int i = 0; i < 4; i++)
	// 	{
	// 		var card = _achievementsBox.GetNodeOrNull<PanelContainer>($"ScrollContainer/FirstSet/Card{i}") 
	// 			?? _achievementsBox.GetNodeOrNull<PanelContainer>($"MarginContainer/GridContainer/Card{i}");
	// 		if (card == null) continue;

	// 		var icon = card.GetNodeOrNull<TextureRect>("Margin/VBox/Icon");
	// 		var titleLabel = card.GetNodeOrNull<Label>("Margin/VBox/Title");
	// 		var statusLabel = card.GetNodeOrNull<Label>("Margin/VBox/Status");

	// 		bool isUnlocked = data._achievements != null && data._achievements.Count > i && data._achievements[i];

	// 		if (isUnlocked)
	// 		{
	// 			// Glowing radiant style
	// 			StyleBoxFlat glow = new StyleBoxFlat
	// 			{
	// 				BgColor = new Color(0.10f, 0.16f, 0.25f, 0.95f),
	// 				BorderWidthLeft = 4,
	// 				BorderWidthTop = 4,
	// 				BorderWidthRight = 4,
	// 				BorderWidthBottom = 4,
	// 				BorderColor = new Color(1.0f, 0.85f, 0.25f, 1.0f), // Golden radiant border
	// 				CornerRadiusTopLeft = 14,
	// 				CornerRadiusTopRight = 14,
	// 				CornerRadiusBottomRight = 14,
	// 				CornerRadiusBottomLeft = 14,
	// 				ShadowColor = new Color(1.0f, 0.82f, 0.15f, 0.85f), // Golden glowing aura
	// 				ShadowSize = 16,
	// 				AntiAliasing = true
	// 			};

	// 			card.AddThemeStyleboxOverride("panel", glow);

	// 			if (icon != null)
	// 			{
	// 				icon.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	// 			}
	// 			if (titleLabel != null)
	// 			{
	// 				titleLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.95f, 0.45f));
	// 			}
	// 			if (statusLabel != null)
	// 			{
	// 				statusLabel.Text = "★ ACHIEVED ★";
	// 				statusLabel.AddThemeColorOverride("font_color", new Color(0.35f, 1.0f, 0.55f));
	// 			}

	// 			// Pulsing subtle glow animation
	// 			card.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	// 			Tween tween = card.CreateTween().SetLoops();
	// 			tween.TweenProperty(card, "modulate", new Color(1.15f, 1.15f, 1.05f), 1.2f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	// 			tween.TweenProperty(card, "modulate", new Color(1.0f, 1.0f, 1.0f), 1.2f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	// 		}
	// 		else
	// 		{
	// 			// Locked style
	// 			StyleBoxFlat locked = new StyleBoxFlat
	// 			{
	// 				BgColor = new Color(0.07f, 0.08f, 0.11f, 0.70f),
	// 				BorderWidthLeft = 2,
	// 				BorderWidthTop = 2,
	// 				BorderWidthRight = 2,
	// 				BorderWidthBottom = 2,
	// 				BorderColor = new Color(0.28f, 0.32f, 0.40f, 0.50f),
	// 				CornerRadiusTopLeft = 14,
	// 				CornerRadiusTopRight = 14,
	// 				CornerRadiusBottomRight = 14,
	// 				CornerRadiusBottomLeft = 14,
	// 				ShadowSize = 0,
	// 				AntiAliasing = true
	// 			};

	// 			card.AddThemeStyleboxOverride("panel", locked);

	// 			if (icon != null)
	// 			{
	// 				icon.Modulate = new Color(0.35f, 0.35f, 0.40f, 0.60f);
	// 			}
	// 			if (titleLabel != null)
	// 			{
	// 				titleLabel.AddThemeColorOverride("font_color", new Color(0.70f, 0.72f, 0.78f));
	// 			}
	// 			if (statusLabel != null)
	// 			{
	// 				statusLabel.Text = "🔒 LOCKED";
	// 				statusLabel.AddThemeColorOverride("font_color", new Color(0.50f, 0.52f, 0.58f));
	// 			}

	// 			card.Modulate = new Color(0.85f, 0.85f, 0.85f, 0.85f);
	// 		}
	// 	}
	// }
}
