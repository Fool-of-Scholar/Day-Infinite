using Godot;
using System;

public partial class HealthBattery : Area2D
{
    [Export] public int HealthRestoreAmount { get; set; } = 5;
    [Export] public float Speed { get; set; } = 350f;

    public Node2D Target { get; set; }

    private bool _isCollected = false;

    public override void _Ready()
    {
        AddToGroup("health_batteries");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isCollected) return;
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

    public void Collect(Movement player = null)
    {
        if (_isCollected) return; // Prevent double collection
        _isCollected = true;

        Movement recipient = player ?? Target as Movement;
        if (recipient == null && IsInsideTree())
        {
            recipient = GetTree().GetFirstNodeInGroup("player") as Movement;
        }

        recipient?.Heal(HealthRestoreAmount);

        // Play health pickup sound with uplifting distinct pitch
        var customStream = GetNodeOrNull<AudioStreamPlayer>("Pickup")?.Stream;
        AudioManager.PlayHealthPickup(customStream);

        QueueFree();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_isCollected) return;
        if (body is Movement player)
        {
            Target = player;
            Collect(player);
        }
    }
}