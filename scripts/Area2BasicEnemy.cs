using Godot;


public partial class Area2BasicEnemy : Enemy
{
    public override void _Ready()
    {
       
        if (MaxHealth == 30) MaxHealth = 80;       
        if (MoveSpeed == 70.0f) MoveSpeed = 68f;  

        base._Ready();
    }
}
