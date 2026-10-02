using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public PackedScene GemScene { get; set; }
	[Export] public int MaxHealth {get; set; } = 50;
	[Export] public float MoveSpeed {get; set;} = 100.0f;

	public int CurrentHealth {get; private set;}
	public Node2D TargetPlayer {get; set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		TargetPlayer =  GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (TargetPlayer == null) return;

		Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();

		Velocity	 = direction * MoveSpeed;
		MoveAndSlide();
	}

	public void TakeDamage(int damageAmount)
	{
		CurrentHealth -= damageAmount;
		GD.Print($"Enemy took {damageAmount} damage! Remaining: {CurrentHealth}");
		if (CurrentHealth <= 0)
		{
			Die();
		}

	}
	private void Die ()
	{
		if (GemScene != null)
		{
			ExperienceGem gem = GemScene.Instantiate<ExperienceGem>();
			gem.GlobalPosition = GlobalPosition;
			GetTree().CurrentScene.CallDeferred("add_child", gem);
		}
		QueueFree();
	}
}
