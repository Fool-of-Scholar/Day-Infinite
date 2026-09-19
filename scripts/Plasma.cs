using Godot;
using System;

public partial class Plasma : Node2D
{
    [Export] public PackedScene BulletScene {get; set;}
    [Export] public PackedScene UpgradeBulletScene {get; set;}

	[Export] public PackedScene ExplosiveBulletScene {get; set;}
	
	[Export] public Movement PlayerRef {get; set;}
    [Export] public float AttackInterval {get; set;} = 0.6f;
    [Export] public float Range {get; set;} = 600f;
    
    private Timer _timer;
    private Movement _player; 

	private AudioStreamPlayer2D _shootSound;

    public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		_timer.WaitTime = AttackInterval;
		_timer.Timeout += Fire;

		_shootSound = GetNode<AudioStreamPlayer2D>("ShootSound");

		// Tell the weapon to listen for the LeveledUp signal using the Inspector reference
		if (PlayerRef != null)
		{
			PlayerRef.LeveledUp += OnPlayerLeveledUp; 
			GD.Print("SUCCESS: Weapon successfully found the Player and connected the signal!");
		}
		else
		{
			GD.Print("ERROR: PlayerRef is empty! Please assign the Player node in the Inspector.");
		}
	}

	private void OnPlayerLeveledUp()
    {
        // Check for Level 6 FIRST! (Otherwise Level 2 overrides it)
        if (PlayerRef.CurrentLevel >= 4 && ExplosiveBulletScene != null)
        {
            BulletScene = ExplosiveBulletScene;
            AttackInterval = 2.0f; // Slow the fire rate down so the explosions aren't too overpowered
            _timer.WaitTime = AttackInterval;
            GD.Print("Weapon Upgraded! Level 6 EXPLOSIONS!");
        }
        else if (PlayerRef.CurrentLevel >= 2 && UpgradeBulletScene != null)
        {
            BulletScene = UpgradeBulletScene;
            AttackInterval = 0.09f; 
            _timer.WaitTime = AttackInterval; 
            GD.Print("Weapon Upgraded! Now Firing Red Plasma.");
        }
    }

    private void Fire ()
    {
        if (BulletScene == null) return;

        Node2D nearestEnemy = GetNearestEnemy();
        if (nearestEnemy == null) return; 

        _shootSound.Play();

        // 1. Spawn it as a generic Node2D so C# doesn't panic about the type
        Node2D boltNode = BulletScene.Instantiate<Node2D>();
        GetTree().CurrentScene.AddChild(boltNode);
        boltNode.GlobalPosition = GlobalPosition;
        
        Vector2 dir = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();

        // 2. Safely check which script is attached to the bullet and give it the direction!
        if (boltNode is PlasmaBolt normalBolt)
        {
            normalBolt.Direction = dir;
        }
        else if (boltNode is Blackhole blackholeBolt)
        {
            blackholeBolt.Direction = dir;
        }
    }

    private Node2D GetNearestEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D closest = null;
        float shortestDistance = Range;

        foreach (Node node in enemies)
        {
            if (node is Node2D enemy)
            {
                float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closest = enemy;											
                }
            }
        }
        return closest;
    }
}