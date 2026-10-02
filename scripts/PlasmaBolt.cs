using Godot;
using System;

public partial class PlasmaBolt : Area2D
{
	[Export] public float Speed {get; set; } = 400.0f; 
	[Export] public int Damage {get; set;} = 25;

	[Export] public float LifeSpan {get; set; } = 3.0f;

	public Vector2 Direction {get; set; } = Vector2.Right;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().CreateTimer(LifeSpan).Timeout 	+= QueueFree;

		//listen to any impact
		AreaEntered += OnAreaEntered;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * Speed * (float)delta;

	}

	private void OnAreaEntered(Area2D otherArea)
	{
		if (otherArea.GetParent() is Enemy enemy)
		{
			enemy.TakeDamage(Damage);
		}
		QueueFree();
	}
}
