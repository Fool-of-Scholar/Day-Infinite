using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [ExportGroup("Blood and Juice Scene")]
    [Export] public PackedScene GemScene { get; set; }

    [Export] public PackedScene BloodScene {get; set;}

    [ExportGroup("Scrap Drops")]
    [Export] public PackedScene ScrapScene { get; set; }
    [Export] public float ScrapDropChance { get; set; } = 0.15f; // 15% rare chance

    [ExportGroup("Stats")]
    [Export] public int MaxHealth { get; set; } = 50;
    [Export] public float MoveSpeed { get; set; } = 100.0f;
    [Export] public bool SpriteFacesLeftByDefault { get; set; } = true;

    public int CurrentHealth { get; protected set; }
    public Node2D TargetPlayer { get; protected set; }

    protected AnimatedSprite2D _animSprite;

    public override void _Ready()
    {
        AddToGroup("enemies");
        CurrentHealth = MaxHealth;
        
        
        // Grab the AnimatedSprite2D and start the walk cycle
        _animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _animSprite?.Play("walk");

        FindPlayer();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (TargetPlayer == null || !IsInstanceValid(TargetPlayer))
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * MoveSpeed;

        // Sprite faces LEFT in the raw sprite sheet:
        // Moving Right (Velocity.X > 0) -> FlipH = true
        // Moving Left  (Velocity.X < 0) -> FlipH = false
        UpdateFacing(Velocity.X);

        MoveAndSlide();
    }

    protected void UpdateFacing(float moveX)
    {
        if (_animSprite != null && MathF.Abs(moveX) > 1f)
        {
            bool movingRight = moveX > 0;
            _animSprite.FlipH = SpriteFacesLeftByDefault ? movingRight : !movingRight;
        }
    }

    protected void FindPlayer()
    {
        TargetPlayer = GetTree().GetFirstNodeInGroup("player") as Node2D;

        if (TargetPlayer == null && GetTree().CurrentScene != null)
        {
            TargetPlayer = GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");
        }
    }

    
    public virtual void TakeDamage(int damageAmount)
    {
        CurrentHealth -= damageAmount;

        // Flash white on any hit so hits still feel responsive
        FlashDamage();

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    protected void SpawnBlood (){
        if (BloodScene == null) return;

        Node2D blood = BloodScene.Instantiate<AnimatedSprite2D>();
        blood.GlobalPosition = GlobalPosition;

        GetTree().CurrentScene.AddChild(blood);
    }

    protected async void FlashDamage()
    {
        Color original = Modulate;
        Modulate = new Color(2.5f, 2.5f, 2.5f); // Bright hit flash
        await ToSignal(GetTree().CreateTimer(0.07f), SceneTreeTimer.SignalName.Timeout);

        if (IsInstanceValid(this) && CurrentHealth > 0)
        {
            Modulate = original;
        }
    }
    protected virtual void Die()
    {
    // Blood only spawns once the enemy is dead
    SpawnBlood();

        

        // 2. Rare Chance to Drop Scrap
        if (ScrapScene != null)
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            rng.Randomize();
            if (rng.Randf() <= ScrapDropChance) // 15% roll succeeds
            {
                Node2D scrap = ScrapScene.Instantiate<Node2D>();
                scrap.GlobalPosition = GlobalPosition;
                GetTree().CurrentScene.CallDeferred("add_child", scrap);
                GD.Print("[Drop] A rare scrap was discovered!");
            }
        }

        QueueFree();
    }
}