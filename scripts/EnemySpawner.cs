using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene BasicEnemyScene { get; set; }
    [Export] public float SpawnInterval { get; set; } = 0.1f;

    private PathFollow2D _spawnPath;
    private Timer _spawnTimer;
    private Node2D _player;

    public override void _Ready()
    {
        _spawnPath = GetNodeOrNull<PathFollow2D>("Path2D/PathFollow2D");
        _spawnTimer = GetNodeOrNull<Timer>("SpawnTimer");

        // Automatically locate the player in the current world scene
        _player = GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");

        // Configure and start timer
        if (_spawnTimer != null)
        {
            _spawnTimer.WaitTime = SpawnInterval;
            _spawnTimer.OneShot = false;
            _spawnTimer.Timeout += SpawnEnemy;
            _spawnTimer.Start();
        }
    }

    public override void _Process(double delta)
    {
        // Re-locate player if reference was lost
        if (_player == null)
        {
            _player = GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");
            return;
        }

        // Lock the spawner perimeter to follow the player
        GlobalPosition = _player.GlobalPosition;
    }

    private void SpawnEnemy()
    {
        if (BasicEnemyScene == null || _spawnPath == null) return;

        // Pick a random progress point (0.0 to 1.0) along the perimeter curve
        _spawnPath.ProgressRatio = GD.Randf();

        Node2D enemy = BasicEnemyScene.Instantiate<Node2D>();
        
        // Spawn into the root world, NOT as a child of the spawner
        GetTree().CurrentScene.AddChild(enemy);
        enemy.GlobalPosition = _spawnPath.GlobalPosition;
    }
}