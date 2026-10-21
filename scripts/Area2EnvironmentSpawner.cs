using Godot;
using System.Collections.Generic;


/// - Equips trees with trunk-level collision and proper Y-sorting depth.

public partial class Area2EnvironmentSpawner : Node2D
{
    [ExportGroup("Textures")]
    [Export] public Texture2D TreeTexture1 { get; set; }
    [Export] public Texture2D TreeTexture2 { get; set; }
    [Export] public Texture2D WiltedTreeTexture { get; set; }
    [Export] public Texture2D BushTexture { get; set; }
    [Export] public Texture2D GrassTexture { get; set; }
    [Export] public Texture2D DirtTexture { get; set; }

    [ExportGroup("Spawn Counts")]
    [Export] public int TreeCount { get; set; } = 35;
    [Export] public int WiltedCount { get; set; } = 12;
    [Export] public int BushCount { get; set; } = 25;
    [Export] public int DecorCount { get; set; } = 25;

    [ExportGroup("Spawn Area Bounds")]
    [Export] public float MapRadius { get; set; } = 2200f;
    [Export] public float SafeRadius { get; set; } = 160f; // No collision props directly on player spawn
    [Export] public float MinPropDistance { get; set; } = 85f; // Keep trees from overlapping

    private readonly List<Vector2> _placedPositions = new();
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();

        // Default to the new Arboles tree textures if not explicitly assigned in inspector
        TreeTexture1 ??= GD.Load<Texture2D>("res://assets/Environment/Area 2 Environment/Tree.png");
        TreeTexture2 ??= GD.Load<Texture2D>("res://assets/Environment/Area 2 Environment/Tree 2.png");

        // Clear existing static placeholder children from the scene
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        GenerateEnvironment();
    }

    private void GenerateEnvironment()
    {
        // 1. Ground decors (dirt & grass - non-blocking, can spawn anywhere)
        SpawnDecors(DecorCount);

        // 2. Bushes (decorative foliage)
        SpawnBushes(BushCount);

        // 3. Normal Trees (with trunk collision)
        SpawnTrees(TreeCount, isWilted: false);

        // 4. Wilted Trees
        SpawnTrees(WiltedCount, isWilted: true);
    }

    private void SpawnTrees(int count, bool isWilted)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomValidPosition(checkCollisionDistance: true);
            if (pos == Vector2.Zero) continue;

            Texture2D tex;
            if (isWilted)
            {
                tex = WiltedTreeTexture;
            }
            else
            {
                tex = (_rng.Randf() > 0.5f && TreeTexture2 != null) ? TreeTexture2 : TreeTexture1;
            }

            if (tex == null) continue;

            Sprite2D sprite = new Sprite2D
            {
                Texture = tex,
                Position = pos,
                TextureFilter = TextureFilterEnum.Nearest,
                FlipH = _rng.Randf() > 0.5f
            };

            // Add trunk collision so player and enemies walk behind foliage but bump into trunk
            StaticBody2D body = new StaticBody2D();
            CollisionShape2D col = new CollisionShape2D();
            CapsuleShape2D rect = new CapsuleShape2D
            {
                Height = isWilted ? 60f : 80f,
                Radius = 8f
            };

            col.Shape = rect;
            col.Position = isWilted
                ? new Vector2(0, 24)
                : new Vector2(sprite.FlipH ? -2 : 2, 40);

            body.AddChild(col);
            sprite.AddChild(body);
            AddChild(sprite);

            _placedPositions.Add(pos);
        }
    }

    private void SpawnBushes(int count)
    {
        if (BushTexture == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomValidPosition(checkCollisionDistance: false);
            if (pos == Vector2.Zero) continue;

            Sprite2D sprite = new Sprite2D
            {
                Texture = BushTexture,
                Position = pos,
                TextureFilter = TextureFilterEnum.Nearest,
                FlipH = _rng.Randf() > 0.5f
            };

            AddChild(sprite);
            _placedPositions.Add(pos);
        }
    }

    private void SpawnDecors(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Texture2D tex = (_rng.Randf() > 0.5f && GrassTexture != null) ? GrassTexture : DirtTexture;
            if (tex == null) continue;

            Vector2 pos = new Vector2(
                _rng.RandfRange(-MapRadius, MapRadius),
                _rng.RandfRange(-MapRadius, MapRadius)
            );

            Sprite2D sprite = new Sprite2D
            {
                Texture = tex,
                Position = pos,
                TextureFilter = TextureFilterEnum.Nearest,
                FlipH = _rng.Randf() > 0.5f,
                ZIndex = -1 // Render below players and trees
            };

            AddChild(sprite);
        }
    }

    private Vector2 GetRandomValidPosition(bool checkCollisionDistance)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 candidate = new Vector2(
                _rng.RandfRange(-MapRadius, MapRadius),
                _rng.RandfRange(-MapRadius, MapRadius)
            );

            // Never spawn directly on player's start location
            if (candidate.Length() < SafeRadius)
                continue;

            // Check distance to other placed trees
            if (checkCollisionDistance)
            {
                bool tooClose = false;
                foreach (Vector2 p in _placedPositions)
                {
                    if (candidate.DistanceTo(p) < MinPropDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;
            }

            return candidate;
        }

        return Vector2.Zero;
    }
}
