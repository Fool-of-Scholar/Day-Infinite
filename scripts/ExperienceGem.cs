using Godot;
using System;

public partial class ExperienceGem : Area2D
{
    [Export] public int ExperienceValue { get; set; } = 10;
    [Export] public float Speed { get; set; } = 350f;

    public Node2D Target { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        if (Target == null) return;

        // 1. Move toward the player
        GlobalPosition = GlobalPosition.MoveToward(Target.GlobalPosition, Speed * (float)delta);
        Speed += 600f * (float)delta;

        // 2. Distance fallback: If the gem reaches the player's body, collect it immediately
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 20f)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (Target is Movement player)
        {
            player.AddExperience(ExperienceValue);
        }
        QueueFree(); // Instantly destroy the gem
    }
}