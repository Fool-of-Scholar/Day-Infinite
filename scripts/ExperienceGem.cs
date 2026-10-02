using Godot;
using System;

public partial class ExperienceGem : Area2D
{
    [Export] public int ExperienceValue { get; set; } = 10;
    [Export] public float Speed { get; set; } = 350f;

    public Node2D Target { get; set; }

    public override void _Ready()
    {
        // 1. Register to the "gems" group so the Boss Cleanse can find all gems
        AddToGroup("gems");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Safety check: ensure Target exists and hasn't been freed
        if (Target == null || !IsInstanceValid(Target)) return;

        // 2. Move toward the player and accelerate
        GlobalPosition = GlobalPosition.MoveToward(Target.GlobalPosition, Speed * (float)delta);
        Speed += 600f * (float)delta;

        // 3. Distance fallback: Collect when reaching the player
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 25f)
        {
            Collect();
        }
    }

    // Called by the Boss on death (or a Magnet power-up)
    public void TriggerVacuum(Node2D player)
    {
        Target = player;
        Speed = Mathf.Max(Speed, 600f); // High starting speed to cross the entire screen
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