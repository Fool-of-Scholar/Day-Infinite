using Godot;
using System;

public partial class WorldGemSpawner : Node
{
    [Export] public PackedScene GemScene { get; set; }
    
    [ExportGroup("Spawn Frequency Scaling")]
    [Export] public float BaseSpawnInterval { get; set; } = 0.25f; // Starting speed (seconds between spawns)
    [Export] public float MinSpawnInterval { get; set; } = 0.25f; // Fast late-game speed
    
    [ExportGroup("Spawn Distances")]
    [Export] public float OnScreenMinRadius { get; set; } = 50.0f;   // Near player for the start
    [Export] public float OnScreenMaxRadius { get; set; } = 250.0f;  // Inside camera view for start
    
    [Export] public float OffScreenMinRadius { get; set; } = 600.0f; // Ongoing off-screen minimum distance
    [Export] public float OffScreenMaxRadius { get; set; } = 1300.0f;// Ongoing off-screen maximum distance

    [Export] public int MaxActiveGems { get; set; } = 150;

    private float _timer = 0.0f;
    private float _cullTimer = 0.0f;
    private float _elapsedTime = 0.0f;
    private Node2D _player;

    public override void _Ready()
    {
        FindPlayer();
        
        // Step 1: Spawn gems right on-screen around the player at game start
        CallDeferred(nameof(SpawnInitialOnScreenCluster));
    }

    private void FindPlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (_player == null)
        {
            _player = GetNodeOrNull<Node2D>("../Player");
        }
    }

    private void SpawnInitialOnScreenCluster()
    {
        if (_player == null) return;

        // Spawn 8 gems close to the player right when the game boots up
        for (int i = 0; i < 8; i++)
        {
            SpawnGemAtRange(OnScreenMinRadius, OnScreenMaxRadius);
        }
        GD.Print("[Spawner] Initial on-screen gem cluster generated.");
    }

    public override void _Process(double delta)
    {
        if (GemScene == null) return;

        if (_player == null || !IsInstanceValid(_player))
        {
            FindPlayer();
            if (_player == null) return;
        }

        _elapsedTime += (float)delta;
        _timer += (float)delta;
        _cullTimer += (float)delta;

        // Clean up gems left behind far away (> 2000px) when exceeding cap
        if (_cullTimer >= 2.0f)
        {
            _cullTimer = 0.0f;
            var gems = GetTree().GetNodesInGroup("gems");
            if (gems.Count > MaxActiveGems)
            {
                foreach (Node node in gems)
                {
                    if (node is Node2D gemNode && IsInstanceValid(gemNode))
                    {
                        if (gemNode.GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) > 2000f * 2000f)
                        {
                            gemNode.QueueFree();
                        }
                    }
                }
            }
        }

        // --- FASTER FREQUENCY SCALING ---
        float scalingFactor = _elapsedTime * 0.02f; 
        float currentInterval = Mathf.Max(MinSpawnInterval, BaseSpawnInterval - scalingFactor);

        if (_timer >= currentInterval)
        {
            _timer = 0.0f;
            
            // Only spawn if under the active gem budget
            if (GetTree().GetNodesInGroup("gems").Count < MaxActiveGems)
            {
                SpawnGemAtRange(OffScreenMinRadius, OffScreenMaxRadius);
            }
        }
    }

    private void SpawnGemAtRange(float minRadius, float maxRadius)
    {
        if (_player == null) return;

        ExperienceGem gem = GemScene.Instantiate<ExperienceGem>();

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        
        float angle = rng.Randf() * Mathf.Tau;
        float distance = rng.RandfRange(minRadius, maxRadius);

        Vector2 spawnOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        gem.GlobalPosition = _player.GlobalPosition + spawnOffset;

        // Add safely to the current scene tree
        GetTree().CurrentScene.CallDeferred("add_child", gem);
    }
}