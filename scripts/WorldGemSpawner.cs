using Godot;
using System;

public partial class WorldGemSpawner : Node
{
    [Export] public PackedScene GemScene { get; set; }
    
    [ExportGroup("Spawn Frequency Scaling")]
    [Export] public float BaseSpawnInterval { get; set; } = 0.45f; // Balanced for mobile & PC
    [Export] public float MinSpawnInterval { get; set; } = 0.35f;

    [ExportGroup("Spawn Distances")]
    [Export] public float OnScreenMinRadius { get; set; } = 350.0f;  // Outside player's 150px magnet radius to avoid instant vacuum
    [Export] public float OnScreenMaxRadius { get; set; } = 600.0f;  // Visible distance

    [Export] public float OffScreenMinRadius { get; set; } = 600.0f; // Ongoing off-screen minimum distance
    [Export] public float OffScreenMaxRadius { get; set; } = 1300.0f;// Ongoing off-screen maximum distance

    [Export] public int MaxActiveGems { get; set; } = 90; // Mobile-safe budget

    private float _timer = 0.0f;
    private float _cullTimer = 0.0f;
    private float _gemCountTimer = 0.0f;
    private int _cachedGemCount = 0;
    private float _elapsedTime = 0.0f;
    private Node2D _player;

    public override void _Ready()
    {
        FindPlayer();

        // Wait 2.5 seconds so the Area Title banner animation finishes cleanly before any gems appear
        GetTree().CreateTimer(2.5f).Timeout += () =>
        {
            if (IsInstanceValid(this) && _player != null && IsInstanceValid(_player))
            {
                SpawnInitialOnScreenCluster();
            }
        };
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

        // Spawn 8 gems at safe distance from player so they are not immediately vacuumed
        for (int i = 0; i < 8; i++)
        {
            SpawnGemAtRange(OnScreenMinRadius, OnScreenMaxRadius);
        }
        _cachedGemCount += 8;
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
        _gemCountTimer += (float)delta;

        // Periodically refresh active gem count to avoid per-frame heap allocations
        if (_gemCountTimer >= 1.0f)
        {
            _gemCountTimer = 0.0f;
            _cachedGemCount = GetTree().GetNodesInGroup("gems").Count;
        }

        // Clean up gems left behind far away (> 1800px) when exceeding cap
        if (_cullTimer >= 2.0f)
        {
            _cullTimer = 0.0f;
            var gems = GetTree().GetNodesInGroup("gems");
            _cachedGemCount = gems.Count;
            if (_cachedGemCount > MaxActiveGems)
            {
                foreach (Node node in gems)
                {
                    if (node is Node2D gemNode && IsInstanceValid(gemNode))
                    {
                        if (gemNode.GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) > 1800f * 1800f)
                        {
                            gemNode.QueueFree();
                            _cachedGemCount--;
                            if (_cachedGemCount <= MaxActiveGems) break;
                        }
                    }
                }
            }
        }

        // --- FREQUENCY SCALING ---
        float scalingFactor = _elapsedTime * 0.015f; 
        float currentInterval = Mathf.Max(MinSpawnInterval, BaseSpawnInterval - scalingFactor);

        if (_timer >= currentInterval)
        {
            _timer = 0.0f;

            // Only spawn if under the active gem budget
            if (_cachedGemCount < MaxActiveGems)
            {
                SpawnGemAtRange(OffScreenMinRadius, OffScreenMaxRadius);
                _cachedGemCount++;
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