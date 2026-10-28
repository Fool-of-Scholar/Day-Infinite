using Godot;
using System;

/// <summary>
/// Area 2 Enemy Spawner — 20-minute run, Boss at 10:00.
///
/// Archetype roster:
///   0:00  — Creature (basic, frequent, persistent)
///   3:00  — Spirit Kappa (ranged, cross-shot, stationary)
///   5:00  — "Some Creature" Entrapping ring (every 5 min wave)
///   7:00  — Wolf Hound packs
///  10:00  — Boss 2 (mist + root attacks, no world cleanse)
///
/// After Boss 2 spawns, regular enemies continue (Boss is a field boss, not a finale).
/// Game timer runs to 20 minutes — surviving all 20 min is the win condition.
/// </summary>
public partial class EnemySpawner2 : Node2D
{
    [Signal] public delegate void TimeUpdatedEventHandler(int minutes, int seconds);
    [Signal] public delegate void Boss2SpawnedEventHandler();

    [ExportGroup("Testing & Debug")]
    [Export] public float StartTimeOffset { get; set; } = 0f;

    // ── Basic Creature ─────────────────────────────────────────────────────────
    [ExportGroup("Basic Creature")]
    [Export] public PackedScene BasicCreatureScene { get; set; }
    [Export] public float BasicUnlockTime   { get; set; } = 0f;
    [Export] public bool  BasicFacesLeft    { get; set; } = true;

    // ── Spirit Kappa ───────────────────────────────────────────────────────────
    [ExportGroup("Spirit Kappa (Ranged)")]
    [Export] public PackedScene KappaScene      { get; set; }
    [Export] public float       KappaUnlockTime { get; set; } = 180f;  // 3:00
    [Export] public bool        KappaFacesLeft  { get; set; } = true;

    // ── Entrapping Ring (Some Creature) ────────────────────────────────────────
    [ExportGroup("Entrapping Ring")]
    [Export] public PackedScene EntrappingScene       { get; set; }
    [Export] public float       EntrappingFirstTime   { get; set; } = 300f;  // 5:00
    [Export] public float       EntrappingRepeatEvery { get; set; } = 300f;  // every 5 min
    [Export] public int         EntrappingRingCount   { get; set; } = 12;    // enemies in the ring
    [Export] public bool        EntrappingFacesLeft   { get; set; } = true;

    // ── Wolf Hound Pack ────────────────────────────────────────────────────────
    [ExportGroup("Wolf Hound Pack")]
    [Export] public PackedScene WolfScene       { get; set; }
    [Export] public float       WolfUnlockTime  { get; set; } = 420f;  // 7:00
    [Export] public int         WolfPackSize    { get; set; } = 4;
    [Export] public bool        WolfFacesLeft   { get; set; } = true;

    // ── Boss 2 ────────────────────────────────────────────────────────────────
    [ExportGroup("Boss 2")]
    [Export] public PackedScene Boss2Scene        { get; set; }
    [Export] public float       Boss2TriggerTime  { get; set; } = 600f;  // 10:00
    [Export] public bool        Boss2FacesLeft    { get; set; } = false;

    // ── Internals ─────────────────────────────────────────────────────────────
    private PathFollow2D _spawnPath;
    private Node2D _player;
    private Node2D _enemyContainer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private float _elapsedTime = 0f;
    private bool  _boss2Spawned = false;

    private float _basicCooldown    = 0f;
    private float _kappaCooldown    = 0f;
    private float _wolfCooldown     = 0f;
    private float _nextEntrappingAt = 0f; // set on _Ready to EntrappingFirstTime

    public override void _Ready()
    {
        _rng.Randomize();
        _elapsedTime = StartTimeOffset;
        _nextEntrappingAt = EntrappingFirstTime;

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
        if (_player == null) { LocatePlayer(); return; }

        // Spawner follows the player so the spawn ring is always centred on them
        GlobalPosition = _player.GlobalPosition;

        _elapsedTime += (float)delta;

        // Broadcast time for HUD
        int totalSeconds = (int)_elapsedTime;
        EmitSignal(SignalName.TimeUpdated, totalSeconds / 60, totalSeconds % 60);

        // ── Boss 2 at 10:00 ────────────────────────────────────────────────────
        if (!_boss2Spawned && _elapsedTime >= Boss2TriggerTime)
        {
            SpawnBoss2();
            // NOTE: regular spawns continue after Boss 2 — he's a field boss not a finale
        }

        HandleRegularSpawns((float)delta);

        // ── Entrapping ring wave (every 5 minutes starting at 5:00) ────────────
        if (_elapsedTime >= _nextEntrappingAt && EntrappingScene != null)
        {
            SpawnEntrappingRing();
            _nextEntrappingAt += EntrappingRepeatEvery;
        }
    }

    // ── Regular independent spawn timers ─────────────────────────────────────

    private void HandleRegularSpawns(float dt)
    {
        if (_spawnPath == null) return;

        // Basic Creature — high-frequency, ramps from 1.2s to 0.5s by minute 9
        if (_elapsedTime >= BasicUnlockTime && BasicCreatureScene != null)
        {
            _basicCooldown -= dt;
            if (_basicCooldown <= 0f)
            {
                float interval = GetRampedInterval(1.2f, 0.45f, BasicUnlockTime, 540f);
                SpawnAtRing(BasicCreatureScene, 1, BasicFacesLeft);
                _basicCooldown = GetJittered(interval);
            }
        }

        // Spirit Kappa — moderate frequency, 1 at a time
        if (_elapsedTime >= KappaUnlockTime && KappaScene != null)
        {
            _kappaCooldown -= dt;
            if (_kappaCooldown <= 0f)
            {
                float interval = GetRampedInterval(6.0f, 3.0f, KappaUnlockTime, 900f);
                SpawnAtRing(KappaScene, 1, KappaFacesLeft);
                _kappaCooldown = GetJittered(interval);
            }
        }

        // Wolf Hound packs
        if (_elapsedTime >= WolfUnlockTime && WolfScene != null)
        {
            _wolfCooldown -= dt;
            if (_wolfCooldown <= 0f)
            {
                float interval = GetRampedInterval(5.0f, 2.5f, WolfUnlockTime, 900f);
                // Pack size grows after boss spawn
                int packSize = _boss2Spawned ? WolfPackSize + 2 : WolfPackSize;
                SpawnAtRing(WolfScene, packSize, WolfFacesLeft);
                _wolfCooldown = GetJittered(interval);
            }
        }
    }

    // ── Entrapping Ring ───────────────────────────────────────────────────────

    /// <summary>
    /// Spawns EntrappingRingCount enemies evenly distributed in a full circle
    /// around the player at a fixed radius, then they slowly walk inward.
    /// </summary>
    private void SpawnEntrappingRing()
    {
        if (_player == null || EntrappingScene == null) return;

        GD.Print($"[Spawner2] >> ENTRAPPING RING WAVE at {_elapsedTime / 60f:F1} min <<");

        const float RingRadius = 540f;
        float angleStep = Mathf.Tau / EntrappingRingCount;

        for (int i = 0; i < EntrappingRingCount; i++)
        {
            float angle = angleStep * i;
            Vector2 spawnPos = _player.GlobalPosition + new Vector2(
                Mathf.Cos(angle) * RingRadius,
                Mathf.Sin(angle) * RingRadius
            );

            Node2D enemy = EntrappingScene.Instantiate<Node2D>();
            enemy.GlobalPosition = spawnPos;
            enemy.Set("SpriteFacesLeftByDefault", EntrappingFacesLeft);
            enemy.Set("RingAngleDeg", Mathf.RadToDeg(angle));

            ApplyTimeScaling(enemy);

            if (_enemyContainer != null)
                _enemyContainer.AddChild(enemy);
            else
                GetTree().CurrentScene.AddChild(enemy);
        }
    }

    // ── Boss 2 spawn ──────────────────────────────────────────────────────────

    private void SpawnBoss2()
    {
        _boss2Spawned = true;
        EmitSignal(SignalName.Boss2Spawned);
        GD.Print(">>> 10:00 — BOSS 2 ARRIVES! Regular spawns continue. <<<");

        if (Boss2Scene == null || _spawnPath == null) return;

        _spawnPath.ProgressRatio = _rng.Randf();
        Node2D boss = Boss2Scene.Instantiate<Node2D>();
        boss.GlobalPosition = _spawnPath.GlobalPosition;
        boss.Set("SpriteFacesLeftByDefault", Boss2FacesLeft);

        if (_enemyContainer != null)
            _enemyContainer.AddChild(boss);
        else
            GetTree().CurrentScene.AddChild(boss);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SpawnAtRing(PackedScene scene, int count, bool facesLeft)
    {
        int currentCount = GetTree().GetNodesInGroup("enemies").Count;
        if (currentCount >= 90) return;

        for (int i = 0; i < count; i++)
        {
            if (currentCount + i >= 90) break;
            _spawnPath.ProgressRatio = _rng.Randf();
            Node2D enemy = scene.Instantiate<Node2D>();
            enemy.GlobalPosition = _spawnPath.GlobalPosition;
            enemy.Set("SpriteFacesLeftByDefault", facesLeft);
            ApplyTimeScaling(enemy);

            if (_enemyContainer != null)
                _enemyContainer.AddChild(enemy);
            else
                GetTree().CurrentScene.AddChild(enemy);
        }
    }

    /// <summary>
    /// Time-scaled difficulty — same algorithm as Area 1 spawner.
    /// HP: +10%/min capped at +80% (9 min). Speed: +5%/min capped at +45%.
    /// Area 2 base stats are already higher so scaling stays proportional.
    /// </summary>
    private void ApplyTimeScaling(Node2D enemy)
    {
        if (enemy is Enemy e)
        {
            float minutes = _elapsedTime / 60f;
            float hpMult    = 1f + Mathf.Min(minutes * 0.10f, 0.80f);
            float speedMult = 1f + Mathf.Min(minutes * 0.05f, 0.45f);
            e.MaxHealth = Mathf.RoundToInt(e.MaxHealth * hpMult);
            e.MoveSpeed *= speedMult;
        }
    }

    private float GetRampedInterval(float start, float end, float winStart, float winEnd)
    {
        float t = Mathf.Clamp((_elapsedTime - winStart) / (winEnd - winStart), 0f, 1f);
        return Mathf.Lerp(start, end, t);
    }

    private float GetJittered(float baseInterval)
    {
        float variance = baseInterval * 0.20f;
        return _rng.RandfRange(Mathf.Max(0.2f, baseInterval - variance), baseInterval + variance);
    }

    private void LocatePlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (_player == null)
            _player = GetParent()?.GetNodeOrNull<Node2D>("Player");
    }
}
