using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    [Signal] public delegate void TimeUpdatedEventHandler(int minutes, int seconds);
    [Signal] public delegate void BossFightStartedEventHandler();

    [ExportGroup("Testing & Debug")]
    // Set this to 595 or 600 in the Inspector to test the boss instantly
    [Export] public float StartTimeOffset { get; set; } = 600f;

    // --- BASIC ENEMY ---
    [ExportGroup("Basic Enemy")]
    [Export] public PackedScene BasicEnemyScene { get; set; }
    [Export] public float BasicInterval { get; set; } = 1.2f;
    [Export] public float BasicUnlockTime { get; set; } = 0f;       // Spawns from 0:00
    [Export] public bool BasicFacesLeft { get; set; } = true;

    // --- FAST ENEMY ---
    [ExportGroup("Fast Enemy")]
    [Export] public PackedScene FastEnemyScene { get; set; }
    [Export] public float FastInterval { get; set; } = 2.2f;
    [Export] public float FastUnlockTime { get; set; } = 120f;     // Spawns from 2:00
    [Export] public bool FastFacesLeft { get; set; } = true;

    // --- SWARM ENEMY ---
    [ExportGroup("Swarm Enemy")]
    [Export] public PackedScene SwarmEnemyScene { get; set; }
    [Export] public float SwarmInterval { get; set; } = 4.5f;
    [Export] public float SwarmUnlockTime { get; set; } = 240f;    // Spawns from 4:00
    [Export] public int SwarmBatchCount { get; set; } = 4;
    [Export] public bool SwarmFacesLeft { get; set; } = false;

    // --- TANK ENEMY ---
    [ExportGroup("Tank Enemy")]
    [Export] public PackedScene TankEnemyScene { get; set; }
    [Export] public float TankInterval { get; set; } = 8.0f;
    [Export] public float TankUnlockTime { get; set; } = 420f;     // Spawns from 7:00
    [Export] public bool TankFacesLeft { get; set; } = false;

    // --- BOSS ENCOUNTER ---
    [ExportGroup("Boss Encounter")]
    [Export] public PackedScene BossScene { get; set; }
    [Export] public float BossTriggerTime { get; set; } = 600f;    // 10:00
    [Export] public bool BossFacesLeft { get; set; } = false;

    private PathFollow2D _spawnPath;
    private Node2D _player;
    private Node2D _enemyContainer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private float _elapsedTime = 0f;
    private bool _bossSpawned = false;
    private bool _timeFrozen = false;

    // Independent timers per enemy archetype
    private float _basicCooldown = 0f;
    private float _fastCooldown = 0f;
    private float _swarmCooldown = 0f;
    private float _tankCooldown = 0f;

    public override void _Ready()
    {
        _rng.Randomize();
        _elapsedTime = StartTimeOffset;

        _spawnPath = GetNodeOrNull<PathFollow2D>("Path2D/PathFollow2D");

        // Locate or automatically generate the EnemyContainer
        _enemyContainer = GetTree().CurrentScene?.GetNodeOrNull<Node2D>("EnemyContainer");
        if (_enemyContainer == null && GetTree().CurrentScene != null)
        {
            _enemyContainer = new Node2D { Name = "EnemyContainer" };
            GetTree().CurrentScene.AddChild(_enemyContainer);
        }

        LocatePlayer();
    }

    public override void _Process(double delta)
    {
        if (_player == null)
        {
            LocatePlayer();
            return;
        }

        GlobalPosition = _player.GlobalPosition;

        if (_timeFrozen) return;

        _elapsedTime += (float)delta;

        // Broadcast game clock to HUD
        int totalSeconds = (int)_elapsedTime;
        EmitSignal(SignalName.TimeUpdated, totalSeconds / 60, totalSeconds % 60);

        // 10-Minute Trigger
        if (_elapsedTime >= BossTriggerTime && !_bossSpawned)
        {
            TriggerBossEncounter();
            return;
        }

        // Process independent spawn cycles
        HandleIndependentSpawns((float)delta);
    }

    private void HandleIndependentSpawns(float dt)
    {
    if (_spawnPath == null) return;

    _basicCooldown -= dt;
    if (_basicCooldown <= 0f)
    {
        PackedScene sceneToSpawn = null;
        int spawnCount = 1;
        bool facesLeft = true;
        float baseInterval = 1.0f;

        // Strict time brackets (Mutually Exclusive)
        if (_elapsedTime >= 420f && TankEnemyScene != null)
        {
            sceneToSpawn = TankEnemyScene;
            facesLeft = TankFacesLeft;
            baseInterval = 1.6f;
        }
        else if (_elapsedTime >= 240f && SwarmEnemyScene != null)
        {
            sceneToSpawn = SwarmEnemyScene;
            spawnCount = SwarmBatchCount; // Spawns in clusters of 4
            facesLeft = SwarmFacesLeft;
            baseInterval = 1.5f;
        }
        else if (_elapsedTime >= 120f && FastEnemyScene != null)
        {
            sceneToSpawn = FastEnemyScene;
            facesLeft = FastFacesLeft;
            baseInterval = 0.9f;
        }
        else if (BasicEnemyScene != null)
        {
            sceneToSpawn = BasicEnemyScene;
            facesLeft = BasicFacesLeft;
            baseInterval = 1.2f;
        }

        if (sceneToSpawn != null)
        {
            SpawnGroup(sceneToSpawn, spawnCount, facesLeft);
        }

        // Reset cooldown with jitter for the active tier
        _basicCooldown = GetJitteredInterval(baseInterval);
        }
    }

    // Applies a ±20% organic variance so enemies don't spawn like a metronome
    private float GetJitteredInterval(float baseInterval)
    {
        float variance = baseInterval * 0.20f;
        return _rng.RandfRange(Mathf.Max(0.2f, baseInterval - variance), baseInterval + variance);
    }

    private void SpawnGroup(PackedScene scene, int count, bool facesLeft)
    {
        for (int i = 0; i < count; i++)
        {
            _spawnPath.ProgressRatio = _rng.Randf();
            Node2D enemy = scene.Instantiate<Node2D>();
            enemy.GlobalPosition = _spawnPath.GlobalPosition;

            // Injects orientation into Enemy or Boss1Enemy
            enemy.Set("SpriteFacesLeftByDefault", facesLeft);

            if (_enemyContainer != null)
            {
                _enemyContainer.AddChild(enemy);
            }
            else
            {
                GetTree().CurrentScene.AddChild(enemy);
            }
        }
    }

    private void TriggerBossEncounter()
    {
        _bossSpawned = true;
        _timeFrozen = true;

        EmitSignal(SignalName.BossFightStarted);
        GD.Print(">>> 10:00 REACHED! REGULAR SPAWNS HALTED. BOSS ARRIVES! <<<");

        if (BossScene != null && _spawnPath != null)
        {
            _spawnPath.ProgressRatio = _rng.Randf();
            Node2D boss = BossScene.Instantiate<Node2D>();
            boss.GlobalPosition = _spawnPath.GlobalPosition;
            boss.Set("SpriteFacesLeftByDefault", BossFacesLeft);

            if (_enemyContainer != null)
            {
                _enemyContainer.AddChild(boss);
            }
            else
            {
                GetTree().CurrentScene.AddChild(boss);
            }
        }
    }

    private void LocatePlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (_player == null && GetParent() != null)
        {
            _player = GetParent().GetNodeOrNull<Node2D>("Player");
        }
    }
}