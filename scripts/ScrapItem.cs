using Godot;

public partial class ScrapItem : Area2D
{
    [Export] public int ScrapValue { get; set; } = 1;
    public Node2D Target { get; set; }
    private float _speed = 400f;
    private bool _isCollected = false;

    public override void _PhysicsProcess(double delta)
    {
        if (_isCollected) return;

        if (Target != null && IsInstanceValid(Target))
        {
            Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += direction * _speed * (float)delta;

            // Collect when close to the player
            if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 25f)
            {
                Collect();
            }
        }
    }

    public void TriggerVacuum(Node2D player)
    {
        Target = player;
        _speed = Mathf.Max(_speed, 600f);
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

        recipient?.AddScrap(ScrapValue);

        // Play scrap pickup sound
        var customStream = GetNodeOrNull<AudioStreamPlayer>("Pickup")?.Stream;
        AudioManager.PlayScrapPickup(customStream);

        QueueFree();
    }
}