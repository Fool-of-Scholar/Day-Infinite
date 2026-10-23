using Godot;
using System.Collections.Generic;

public partial class Area1EnvironmentSpawner : Node2D
{
    [ExportGroup("Textures")]
    [Export] public Texture2D StreetLightTexture { get; set; }
    [Export] public Texture2D TrashCanTexture { get; set; }
    [Export] public Texture2D BrokenBenchTexture { get; set; }

    [ExportGroup("Spawn Counts")]
    [Export] public int StreetLightCount { get; set; } = 30;
    [Export] public int TrashCanCount { get; set; } = 25;
    [Export] public int BrokenBenchCount { get; set; } = 20;

    [ExportGroup("Spawn Area Bounds")]
    [Export] public float MapRadius { get; set; } = 2200f;
    [Export] public float SafeRadius { get; set; } = 160f; // No collision props directly on player spawn
    [Export] public float MinPropDistance { get; set; } = 80f; // Keep collision props from overlapping

    private readonly List<Vector2> _placedPositions = new();
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();

        // Default textures if not explicitly assigned in inspector
        StreetLightTexture ??= GD.Load<Texture2D>("res://assets/Environment/Area 1 Environment/StreetLight.png");
        TrashCanTexture ??= GD.Load<Texture2D>("res://assets/Environment/Area 1 Environment/TrashCan.png");
        BrokenBenchTexture ??= GD.Load<Texture2D>("res://assets/Environment/Area 1 Environment/BrokenBench.png");

        // Clear existing static placeholder children from the scene
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        GenerateEnvironment();
    }

    private void GenerateEnvironment()
    {
        // 1. Broken Street Lights (with base collision)
        SpawnStreetLights(StreetLightCount);

        // 2. Trash Cans (with collision)
        SpawnTrashCans(TrashCanCount);

        // 3. Broken Benches (decorative, no collision)
        SpawnBenches(BrokenBenchCount);
    }

    private void SpawnStreetLights(int count)
    {
        if (StreetLightTexture == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomValidPosition(checkCollisionDistance: true);
            if (pos == Vector2.Zero) continue;

            Sprite2D sprite = new Sprite2D
            {
                Texture = StreetLightTexture,
                Position = pos,
                TextureFilter = TextureFilterEnum.Nearest,
                FlipH = _rng.Randf() > 0.5f
            };

            // sprite size is 29x150
            StaticBody2D body = new StaticBody2D();
            CollisionShape2D col = new CollisionShape2D();
            RectangleShape2D rect = new RectangleShape2D { 
                Size = new Vector2(12, 37)
        
                 };

            col.Shape = rect;
            col.Position = new Vector2(sprite.FlipH ? -4 : 4, 65);

            body.AddChild(col);
            sprite.AddChild(body);
            AddChild(sprite);

            _placedPositions.Add(pos);
        }
    }

    private void SpawnTrashCans(int count)
    {
        if (TrashCanTexture == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomValidPosition(checkCollisionDistance: true);
            if (pos == Vector2.Zero) continue;

            Sprite2D sprite = new Sprite2D
            {
                Texture = TrashCanTexture,
                Position = pos,
                TextureFilter = TextureFilterEnum.Nearest,
                FlipH = _rng.Randf() > 0.5f
            };

            // sprite size is 33x41
            StaticBody2D body = new StaticBody2D();
            CollisionShape2D col = new CollisionShape2D();
            RectangleShape2D rect = new RectangleShape2D { Size = new Vector2(17, 21) };
    
            col.Shape = rect;
            col.Position = new Vector2(0, 10);

            body.AddChild(col);
            sprite.AddChild(body);
            AddChild(sprite);

            _placedPositions.Add(pos);
        }
    }

    private void SpawnBenches(int count)
    {
        if (BrokenBenchTexture == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomValidPosition(checkCollisionDistance: false);
            if (pos == Vector2.Zero) continue;

            Sprite2D sprite = new Sprite2D
            {
                Texture = BrokenBenchTexture,
                Position = pos,
                TextureFilter = TextureFilterEnum.Nearest,
                FlipH = _rng.Randf() > 0.5f
            };

            AddChild(sprite);
            _placedPositions.Add(pos);
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

            // Check distance to other placed collision props
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
