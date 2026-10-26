using Godot;
using System;
using System.Collections.Generic;

public partial class TeslaBurst : Area2D
{
    [Export] public float FireRate { get; set; } = 1.2f;   // Balanced: was 1.0 — slightly slower than rocket
    [Export] public int Damage { get; set; } = 50;           // Balanced: was 40 — chain value justifies cost

    [Export] public Texture2D[] LightningSprites { get; set; }
    [Export] public PackedScene ExplosionScene { get; set; }

    // --- STAGE PROPERTIES (set by WeaponManager) ---
    // Stage 2: chain to nearest enemy up to this many hops (0 = no chain)
    public int ChainCount { get; set; } = 0;
    public float ChainRange { get; set; } = 300f;
    // Stage 3: shoot this many side-by-side beams (1 = default, 3 = triple beam)
    // NOTE: WeaponManager spawns extra TeslaBurst instances offset sideways for Stage 3.
    // This flag is set to true on the extra beams so they don't chain further.
    public bool IsChainedBeam { get; set; } = false;

    public override async void _Ready()
    {
        GetNodeOrNull<AudioStreamPlayer>("ShotAudio")?.Play();

        Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");


        if (LightningSprites != null && LightningSprites.Length > 0)
        {
            int randomIndex = (int)GD.RandRange(0, LightningSprites.Length - 1);
            sprite.Texture = LightningSprites[randomIndex];
            sprite.FlipV = GD.Randf() > 0.5f;
        }

        // Wait two physics frames so collision circle is ready
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Collect all enemies hit by this beam
        List<Enemy> hitEnemies = new List<Enemy>();

        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is Enemy enemy)
            {
                hitEnemies.Add(enemy);
                enemy.TakeDamage(Damage);
                SpawnEffect(enemy.GlobalPosition);
            }
        }

        // Stage 2: chain from the first hit enemy to nearby enemies
        if (ChainCount > 0 && !IsChainedBeam && hitEnemies.Count > 0)
        {
            ChainLightning(hitEnemies[0], ChainCount, hitEnemies);
        }

        // Fade out
        Tween fadeTween = CreateTween();
        fadeTween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
        await ToSignal(fadeTween, Tween.SignalName.Finished);
        QueueFree();
    }

    private void ChainLightning(Enemy from, int remainingHops, List<Enemy> alreadyHit)
    {
        if (remainingHops <= 0) return;

        Enemy nearest = FindNearestEnemy(from.GlobalPosition, alreadyHit);
        if (nearest == null) return;

        alreadyHit.Add(nearest);
        nearest.TakeDamage(Damage);
        SpawnEffect(nearest.GlobalPosition);

        // Draw a brief visual line/arc between from and nearest
        SpawnChainEffect(from.GlobalPosition, nearest.GlobalPosition);

        ChainLightning(nearest, remainingHops - 1, alreadyHit);
    }

    private Enemy FindNearestEnemy(Vector2 origin, List<Enemy> exclude)
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Enemy nearest = null;
        float best = ChainRange;

        foreach (Node node in enemies)
        {
            if (node is Enemy e && !exclude.Contains(e))
            {
                float dist = origin.DistanceTo(e.GlobalPosition);
                if (dist < best)
                {
                    best = dist;
                    nearest = e;
                }
            }
        }
        return nearest;
    }

    private void SpawnEffect(Vector2 pos)
    {
        if (ExplosionScene != null)
        {
            GpuParticles2D explosion = ExplosionScene.Instantiate<GpuParticles2D>();
            explosion.GlobalPosition = pos;
            GetTree().CurrentScene.AddChild(explosion);
        }
    }

    private async void SpawnChainEffect(Vector2 from, Vector2 to)
    {
        // 1. Create a line dynamically
        Line2D arc = new Line2D();
        
        // 2. Add start point
        arc.AddPoint(from);
        
        // 3. Add a jittery middle point to make it look like a lightning zigzag
        Vector2 mid = (from + to) / 2.0f;
        float jitter = from.DistanceTo(to) * 0.2f; // Scale the zigzag by how far the jump is
        mid += new Vector2((float)GD.RandRange(-jitter, jitter), (float)GD.RandRange(-jitter, jitter));
        arc.AddPoint(mid);
        
        // 4. Add end point
        arc.AddPoint(to);
        
        // 5. Make it look like electricity
        arc.DefaultColor = new Color(0.4f, 0.9f, 1.0f, 1.0f); // Bright electric cyan
        arc.Width = 3.0f;
        GetTree().CurrentScene.AddChild(arc);

        // 6. Fade it out quickly in 0.2 seconds
        Tween fade = CreateTween();
        fade.TweenProperty(arc, "modulate:a", 0.0f, 0.2f);
        await ToSignal(fade, Tween.SignalName.Finished);
        arc.QueueFree();
    }
}