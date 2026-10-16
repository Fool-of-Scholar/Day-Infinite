using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    [Signal] public delegate void TimeUpdatedEventHandler(int minutes, int seconds);
    [Signal] public delegate void BossFightStartedEventHandler();
    [Signal] public delegate void EliteWarningEventHandler(); // Fires at 5:30 for a mini-boss pulse

    [ExportGroup("Testing & Debug")]
    // Set this to 595 or 600 in the Inspector to test the boss instantly
    [Export] public float StartTimeOffset { get; set; } = 0f;

    // --- BASIC ENEMY ---
    [ExportGroup("Basic Enemy")]
    [Export] public PackedScene BasicEnemyScene { get; set; }
    [Export] public float BasicUnlockTime { get; set; } = 0f;       // Spawns from 0:00
    [Export] public bool BasicFacesLeft { get; set; } = true;

    // --- FAST ENEMY ---
    [ExportGroup("Fast Enemy")]
    [Export] public PackedScene FastEnemyScene { get; set; }
    [Export] public float FastUnlockTime { get; set; } = 120f;     // Spawns from 2:00
    [Export] public bool FastFacesLeft { get; set; } = true;

    // --- SWARM ENEMY ---
    [ExportGroup("Swarm Enemy")]
    [Export] public PackedScene SwarmEnemyScene { get; set; }
    [Export] public float SwarmUnlockTime { get; set; } = 240f;    // Spawns from 4:00
    [Export] public int SwarmBatchCount { get; set; } = 4;
    [Export] public bool SwarmFacesLeft { get; set; } = false;

    // --- TANK ENEMY ---
    [ExportGroup("Tank Enemy")]
    [Export] public PackedScene TankEnemyScene { get; set; }
    [Export] public float TankUnlockTime { get; set; } = 360f;     // Spawns from 6:00 (was 7:00)
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
    private bool _eliteWarningFired = false; // 5:30 mid-game surge warning

    // --- INDEPENDENT TIMERS PER ARCHETYPE (all active simultaneously once unlocked) ---
    private float _basicCooldown = 0f;
    private float _fastCooldown = 0f;
    private float _swarmCooldown = 0f;
    private float _tankCooldown = 0f;

    public override void _Ready()
    {
        _rng.Randomize();
        _elapsedTime = StartTimeOffset;

        _spawnPath = GetNodeOrNull<PathFollow2D>("Path2D/PathFollow2D");

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

        if (!_timeFrozen)
        {
            _elapsedTime += (float)delta;

            // Broadcast game clock to HUD
            int totalSeconds = (int)_elapsedTime;
            EmitSignal(SignalName.TimeUpdated, totalSeconds / 60, totalSeconds % 60);

            // 5:30 mid-game elite warning surge
            if (_elapsedTime >= 330f && !_eliteWarningFired)
            {
                _eliteWarningFired = true;
                EmitSignal(SignalName.EliteWarning);
                TriggerEliteSurge();
                GD.Print(">>> 5:30 ELITE SURGE! Spawning extra wave! <<<");
            }
        }

        // 10-Minute Boss Trigger
        if (_elapsedTime >= BossTriggerTime && !_bossSpawned)
        {
            TriggerBossEncounter();
            return;
        }

        // All active archetypes spawn simultaneously via independent timers
        HandleIndependentSpawns((float)delta);
    }

    private void HandleIndependentSpawns(float dt)
    {
        if (_spawnPath == null) return;

        // --- BASIC ENEMY (always active after unlock, never replaced) ---
        if (_elapsedTime >= BasicUnlockTime && BasicEnemyScene != null)
        {
            _basicCooldown -= dt;
            if (_basicCooldown <= 0f)
            {
                // Ramp: starts at 1.4s, drops to 0.6s by minute 9
                float baseInterval = GetRampedInterval(1.4f, 0.6f, BasicUnlockTime, 540f);
                SpawnGroup(BasicEnemyScene, 1, BasicFacesLeft);
                _basicCooldown = GetJitteredInterval(baseInterval);
            }
        }

        // --- FAST ENEMY (joins from 2:00, gets faster over time) ---
        if (_elapsedTime >= FastUnlockTime && FastEnemyScene != null)
        {
            _fastCooldown -= dt;
            if (_fastCooldown <= 0f)
            {
                float baseInterval = GetRampedInterval(1.8f, 0.7f, FastUnlockTime, 540f);
                SpawnGroup(FastEnemyScene, 1, FastFacesLeft);
                _fastCooldown = GetJitteredInterval(baseInterval);
            }
        }

        // --- SWARM ENEMY (joins from 4:00 in growing clusters) ---
        if (_elapsedTime >= SwarmUnlockTime && SwarmEnemyScene != null)
        {
            _swarmCooldown -= dt;
            if (_swarmCooldown <= 0f)
            {
                float baseInterval = GetRampedInterval(3.5f, 1.5f, SwarmUnlockTime, 540f);
                // Batch grows from 3 → 6 over time
                int batch = (_elapsedTime >= 420f) ? 6 : SwarmBatchCount;
                SpawnGroup(SwarmEnemyScene, batch, SwarmFacesLeft);
                _swarmCooldown = GetJitteredInterval(baseInterval);
            }
        }

        // --- TANK ENEMY (joins from 6:00, very slow cadence) ---
        if (_elapsedTime >= TankUnlockTime && TankEnemyScene != null)
        {
            _tankCooldown -= dt;
            if (_tankCooldown <= 0f)
            {
                float baseInterval = GetRampedInterval(8.0f, 4.0f, TankUnlockTime, 540f);
                SpawnGroup(TankEnemyScene, 1, TankFacesLeft);
                _tankCooldown = GetJitteredInterval(baseInterval);
            }
        }
    }

    // Lerp a spawn interval from startInterval down to endInterval across a time window
    private float GetRampedInterval(float startInterval, float endInterval, float windowStart, float windowEnd)
    {
        float t = Mathf.Clamp((_elapsedTime - windowStart) / (windowEnd - windowStart), 0f, 1f);
        return Mathf.Lerp(startInterval, endInterval, t);
    }

    // Applies ±20% organic variance so enemies don't spawn like a metronome
    private float GetJitteredInterval(float baseInterval)
    {
        float variance = baseInterval * 0.20f;
        return _rng.RandfRange(Mathf.Max(0.2f, baseInterval - variance), baseInterval + variance);
    }

    // Triggers a burst of mixed enemies at the 5:30 elite moment
    private void TriggerEliteSurge()
    {
        if (_spawnPath == null) return;

        // Spawn a fast wave + swarm cluster together as an elite pressure event
        if (FastEnemyScene != null) SpawnGroup(FastEnemyScene, 4, FastFacesLeft);
        if (SwarmEnemyScene != null) SpawnGroup(SwarmEnemyScene, 8, SwarmFacesLeft);
        if (BasicEnemyScene != null) SpawnGroup(BasicEnemyScene, 5, BasicFacesLeft);
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

            // Scale enemy stats by current time bracket for progressive difficulty
            ApplyTimeScaling(enemy);

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

    // Scales enemy HP and speed based on current elapsed time — enemy pressure grows organically
    private void ApplyTimeScaling(Node2D enemy)
    {
        if (enemy is Enemy e)
        {
            // HP scales: +10% per minute elapsed (capped at +80% max = 9 min)
            float minutesElapsed = _elapsedTime / 60f;
            float hpMultiplier = 1f + Mathf.Min(minutesElapsed * 0.10f, 0.80f);
            e.MaxHealth = Mathf.RoundToInt(e.MaxHealth * hpMultiplier);
            // Reset CurrentHealth to match new max (node just spawned, so base = export value)
            // NOTE: Enemy._Ready() sets CurrentHealth = MaxHealth, so we must set MaxHealth BEFORE _Ready runs.
            // Since we set this right after Instantiate (before AddChild), it applies correctly.

            // Speed scales: +5% per minute elapsed (capped at +45% max = 9 min)
            float speedMultiplier = 1f + Mathf.Min(minutesElapsed * 0.05f, 0.45f);
            e.MoveSpeed = e.MoveSpeed * speedMultiplier;
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