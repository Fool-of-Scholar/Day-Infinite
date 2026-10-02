using Godot;
using System;

public partial class Plasma : Node2D

{

	[Export] public  PackedScene BulletScene {get; set;}
	[Export] public float AttackInterval {get; set;} = 1.0f;

	private Timer _timer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		_timer.WaitTime = AttackInterval;
		_timer.OneShot = false;
		_timer.Timeout += Fire;
		_timer.Start();
		
	}

	private void Fire ()
	{
		if (BulletScene == null) return;

		PlasmaBolt bolt = BulletScene.Instantiate<PlasmaBolt>();

		GetTree().CurrentScene.AddChild(bolt);
		bolt.GlobalPosition = GlobalPosition;

		bolt.Direction = GetFiringDirection();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private Vector2 GetFiringDirection() 
	{

		return 	Vector2.FromAngle((float)GD.RandRange(0, Mathf.Tau	));
	}
}
