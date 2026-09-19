using Godot;
using System;

public partial class World : Node2D
{
	// References to all 4 of your individual layer nodes
	private TileMapLayer _waterLayer;
	private TileMapLayer _groundLayer;
	private TileMapLayer _environmentLayer;
	private TileMapLayer _cliffsLayer;

	//  Change 'int' to 'string' since "can_place_item" is text text, not a number
	private string _canPlaceDataKey = "can_place_item";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_waterLayer = GetNode<TileMapLayer>("water");
		_groundLayer = GetNode<TileMapLayer>("ground");
		_environmentLayer = GetNode<TileMapLayer>("environment");
		_cliffsLayer = GetNode<TileMapLayer>("cliffs");

<<<<<<< Updated upstream
		GD.Print("Hello World! Map systems initialized.");
=======
		Movement player = GetNodeOrNull<Movement>("Player");
		HUD hud = GetNodeOrNull<HUD>("Hud");

		if (player != null && hud != null)
		{
			player.HealthChanged += hud.UpdateHealth;
			player.XpChanged += hud.UpdateXp;
			player.LeveledUp += hud.OpenUpgradeMenu;

			hud.UpdateHealth(player.CurrentHealth, player.MaxHealth);
			hud.UpdateXp(player.CurrentXp, player.XpToNextLevel, player.CurrentLevel);

		}

		GD.Print("Hello World! Map systems initialized.");


>>>>>>> Stashed changes
	}



	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("click"))
		{
			// 1. Fetch the absolute world pixel position matching your Camera2D perspective
			Vector2 mousePosition = GetGlobalMousePosition();
			
			// 2. Map the pixel coordinates to your grid cell coordinates
			Vector2I cellPosition = _groundLayer.LocalToMap(mousePosition);

			// 3. Define the item tile you want to place from your Tileset asset
			int sourceId = 1; 
			Vector2I itemAtlasCoords = new Vector2I(35, 3); 

			
			TileData tileData = _groundLayer.GetCellTileData(cellPosition);

			
			if (tileData != null)
			{
				
				bool canPlace = tileData.GetCustomData(_canPlaceDataKey).AsBool();

				if (canPlace)
				{
					// TO paint the item tile directly onto the environment layer
					_environmentLayer.SetCell(cellPosition, sourceId, itemAtlasCoords);
					GD.Print($"Placed an item at cell: {cellPosition} using atlas coords: {itemAtlasCoords}");
				}
				else
				{
					GD.Print("Cannot place item here. The tile does not allow placement.");
				}
			}
			else
			{
				GD.Print("No tile data found at this position .");
			}
		}	
	}

		public void PlayerDied()
	{
		PackedScene gameOverScene = GD.Load<PackedScene>("res://scene/died_interface.tscn");


		if (gameOverScene != null)
		{
			Control gameOverInterface = gameOverScene.Instantiate<Control>();
			GetTree().Root.AddChild(gameOverInterface);
			GetTree().Paused = true; // Pause the game when the player dies
		}
		else
		{
			GD.PrintErr("Failed to load the game over scene.");
		}
	}
	
	public void OnPlayerDied()
    {
        PackedScene gameOverScene = GD.Load<PackedScene>("res://scene/died_interface.tscn");

        if (gameOverScene != null)
        {
            Control gameOverInterface = gameOverScene.Instantiate<Control>();
            
            // Add it directly to the world scene
            AddChild(gameOverInterface);
            
            // Pause the rest of the game
            GetTree().Paused = true; 
        }
        else
        {
            GD.PrintErr("Failed to load the game over scene.");
        }
    }
}
