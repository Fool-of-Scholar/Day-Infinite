using Godot;
using System;

public partial class ExperienceGem : Area2D
{
	[Export] public int ExperienceValue {get; set; } = 10;
	[Export] public float BaseSpeed {get; set; } =250.0f;

	public Node2D Target {get; set; }
	private float _currentSpeed;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_currentSpeed = BaseSpeed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// When a magnet targets this gem, smoothly accelerate toward the player
		if (Target != null)
		{
			Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
			GlobalPosition += direction * _currentSpeed * (float)delta;
			_currentSpeed += 300.0f * (float)delta;
		}
	}
}
