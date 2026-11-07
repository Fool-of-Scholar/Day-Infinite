// using Godot;
// using System;

// public partial class UpgradeMenu : Control
// {
// 	// 0 = Left Card, 1 = Middle Card, 2 = Right Card
// 	private int currentCardIndex = 0; 

// 	// References to local buttons
// 	private Button _btnLeft;
// 	private Button _btnMiddle;
// 	private Button _btnRight;

// 	// Reference to our parent HUD node
// 	private HUD _hudParent;

// 	// Called when the node enters the scene tree for the first time.
// 	public override void _Ready()
// 	{
// 		// 1. Fetching the parent HUD script reference safely
// 		_hudParent = GetParent<HUD>();

// 		// 2. Instantiating the 3 upgrade card buttons from your menu layout paths
// 		_btnLeft = GetNodeOrNull<Button>("PanelContainer/MarginContainer/HBoxContainer/BtnLeft");
// 		_btnMiddle = GetNodeOrNull<Button>("PanelContainer/MarginContainer/HBoxContainer/BtnMiddle");
// 		_btnRight = GetNodeOrNull<Button>("PanelContainer/MarginContainer/HBoxContainer/BtnRight");

// 		// FIX: Disables Godot's built-in automatic UI layout neighbor searching.
// 		// This forces controller inputs to follow your strict script constraints.
// 		NodePath selfPath = new NodePath(".");
// 		Button[] upgradeButtons = { _btnLeft, _btnMiddle, _btnRight };
// 		foreach (Button btn in upgradeButtons)
// 		{
// 			if (btn != null)
// 			{
// 				btn.FocusNeighborTop = selfPath;
// 				btn.FocusNeighborBottom = selfPath;
// 				btn.FocusNeighborLeft = selfPath;
// 				btn.FocusNeighborRight = selfPath;
// 			}
// 		}

// 		// Grabs the initial screen index focus safely when the level-up panel opens
// 		_btnLeft?.GrabFocus();
// 	}

// 	/* 
// 	 * Intercepts controller configurations cleanly.
// 	 * Utilizes modulo operations to map loop index ranges and prevent double registration skips.
// 	 */
// 	public override void _Input(InputEvent @event)
// 	{
// 		// Discards hold down frame echoes to completely clean up double hopping bugs
// 		if (@event.IsEcho()) return;

// 		if (@event.IsActionPressed("left"))
// 		{
// 			// Loop left across cards cleanly: 0 -> 2 -> 1 -> 0
// 			currentCardIndex = (currentCardIndex - 1 + 3) % 3;
// 			UpdateCardFocus();
// 			GetViewport().SetInputAsHandled();
// 		}
// 		else if (@event.IsActionPressed("right"))
// 		{
// 			// Loop right across cards cleanly: 0 -> 1 -> 2 -> 0
// 			currentCardIndex = (currentCardIndex + 1) % 3;
// 			UpdateCardFocus();
// 			GetViewport().SetInputAsHandled();
// 		}
// 		else if (@event.IsActionPressed("enter"))
// 		{
// 			// ONLY fires the parent HUD selection methods matching the exact active tracking index
// 			if (_hudParent != null && _hudParent.Visible == true)
// 			{
// 				switch (currentCardIndex)
// 				{
// 					case 0:
// 						GD.Print("[UpgradeMenu] Triggering Parent HUD: Left Selection");
// 						_hudParent.OnLeftSelected(); 
// 						break;
// 					case 1:
// 						GD.Print("[UpgradeMenu] Triggering Parent HUD: Middle Selection");
// 						_hudParent.OnMiddleSelected(); 
// 						break;
// 					case 2:
// 						GD.Print("[UpgradeMenu] Triggering Parent HUD: Right Selection");
// 						_hudParent.OnRightSelected(); 
// 						break;
// 				}
// 			}
// 			else
// 			{
// 				GD.PrintErr("[UpgradeMenu] Error: Parent HUD node reference could not be found!");
// 			}
// 			GetViewport().SetInputAsHandled();
// 		}
// 	}

// 	// Helper function that explicitly handles focusing the active horizontal card
// 	private void UpdateCardFocus()
// 	{
// 		switch (currentCardIndex)
// 		{
// 			case 0:
// 				_btnLeft?.GrabFocus();
// 				break;
// 			case 1:
// 				_btnMiddle?.GrabFocus();
// 				break;
// 			case 2:
// 				_btnRight?.GrabFocus();
// 				break;
// 		}
// 	}
// }
