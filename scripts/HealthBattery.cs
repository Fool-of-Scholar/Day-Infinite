using Godot;
using System;

public partial class HealthBattery : Area2D
{
    [Export] public int HealthRestoreAmount { get; set; } = 5;
    [Export] public float Speed { get; set; } = 350f;

    public Node2D Target { get; set; }

    public override void _Ready()
    {
        AddToGroup("health_batteries");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Target == null || !IsInstanceValid(Target)) return;

        // Move toward the player and accelerate
        GlobalPosition = GlobalPosition.MoveToward(Target.GlobalPosition, Speed * (float)delta);
        Speed += 600f * (float)delta;

        // Collect when reaching the player
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 25f)
        {
            Collect();
        }
    }

    public void TriggerVacuum(Node2D player)
    {
        Target = player;
        Speed = Mathf.Max(Speed, 600f);
    }

    // This is the method Movement.cs was looking for!
    public void Collect()
    {
        if (Target is Movement player)
        {
            player.Heal(HealthRestoreAmount);
        }
        else
        {
            // Fallback if collected via body collision area instead of magnet target
            Node2D playerNode = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (playerNode is Movement p)
            {
                p.Heal(HealthRestoreAmount);
            }
        }
        QueueFree();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Movement player)
        {
            Target = player;
            Collect();
        }
    }
}