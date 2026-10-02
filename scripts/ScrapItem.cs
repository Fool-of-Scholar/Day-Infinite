using Godot;

public partial class ScrapItem : Area2D
{
    [Export] public int ScrapValue { get; set; } = 1;
    public Node2D Target { get; set; }
    private float _speed = 400f;

    public override void _PhysicsProcess(double delta)
    {
        if (Target != null && IsInstanceValid(Target))
        {
            Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += direction * _speed * (float)delta;

            // Collect when close to the player
            if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 25f)
            {
                if (Target is Movement player)
                {
                    player.AddScrap(ScrapValue);
                }
                QueueFree();
            }
        }
    }
}