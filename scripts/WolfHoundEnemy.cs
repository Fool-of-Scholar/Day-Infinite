using Godot;


public partial class WolfHoundEnemy : Enemy
{
    [ExportGroup("Wolf Pack")]
    /// Pack mates near by slightly accelerate this wolf — simulated pack aggression.
    [Export] public float PackBoostSpeed { get; set; } = 20f;
    [Export] public float PackSenseRadius { get; set; } = 180f;

    private float _packBoostTimer = 0f;
    private const float PackCheckInterval = 1.0f;  // Re-check pack count every second
    private int _nearbyPack = 0;

    public override void _Ready()
    {
        if (MaxHealth == 30) MaxHealth = 140;    
        if (MoveSpeed == 70.0f) MoveSpeed = 88f; 

        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Pack boost check
        _packBoostTimer -= (float)delta;
        if (_packBoostTimer <= 0f)
        {
            _nearbyPack = CountNearbyWolves();
            _packBoostTimer = PackCheckInterval;
        }

        if (TargetPlayer == null || !IsInstanceValid(TargetPlayer))
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        float effectiveSpeed = MoveSpeed + (_nearbyPack >= 2 ? PackBoostSpeed : 0f);

        Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * effectiveSpeed;
        UpdateFacing(Velocity.X);
        MoveAndSlide();
    }

    private int CountNearbyWolves()
    {
        int count = 0;
        var allEnemies = GetTree().GetNodesInGroup("enemies");
        foreach (Node n in allEnemies)
        {
            if (n == this) continue;
            if (n is WolfHoundEnemy wolf)
            {
                if (GlobalPosition.DistanceTo(wolf.GlobalPosition) < PackSenseRadius)
                    count++;
            }
        }
        return count;
    }
}
