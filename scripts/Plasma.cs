using Godot;
using System;

public partial class Plasma : Node2D

{

	[Export] public  PackedScene BulletScene {get; set;}
	[Export] public float AttackInterval {get; set;} = 0.6f;
	[Export] public float Range {get; set;} = 600f;
	private Timer _timer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		_timer.WaitTime = AttackInterval;
		_timer.Timeout += Fire;

		
	}

	private void Fire ()
	{
		if (BulletScene == null) return;

		Node2D nearestEnemy = GetNearestEnemy();

		if (nearestEnemy == null) return; // no shots when no enemy


		PlasmaBolt bolt = BulletScene.Instantiate<PlasmaBolt>();

		GetTree().CurrentScene.AddChild(bolt);
		bolt.GlobalPosition = GlobalPosition;

		bolt.Direction = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();
		

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

	// Called every frame. 'delta' is the elapsed time since the previous frame.

}
