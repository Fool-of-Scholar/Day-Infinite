using Godot;
using System;

public partial class Movement : CharacterBody2D
{

    //Signals

    [Signal] public delegate void HealthChangedEventHandler (int currentHealth, int maxHealth);
    [Signal] public delegate void XpChangedEventHandler (int currentXp, int xpToNextLevel, int currentLevel);
    [Signal] public delegate void LeveledUpEventHandler ();
    // Character Health
    [Export] public int MaxHealth {get; set; } = 100;
    public int CurrentHealth {get; private set;}
    private Area2D _hurtbox;
    private Timer _invulnerabilityTimer;
    private bool _isInvulnerable = false;


    [Export]
    public int Speed { get; set; } = 150;

    // --- Progression & Leveling Fields ---
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXp { get; private set; } = 0;
    public int XpToNextLevel { get; private set; } = 100;

    // --- Detection Areas for Pickups ---
    private Area2D _magnetArea;
    private Area2D _collectionArea;

    // --- Visual & Animation References ---
    private AnimationPlayer _animationPlayer;
    private Sprite2D _idleSprite;
    private Sprite2D _walkSprite;

    private Vector2 _lastFacingDirection = Vector2.Down;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        
        // Cache node references from the scene tree
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _idleSprite = GetNode<Sprite2D>("Idle");
        _walkSprite = GetNode<Sprite2D>("Walk");

        // Cache pickup detection areas
        _magnetArea = GetNode<Area2D>("MagnetArea");
        _collectionArea = GetNode<Area2D>("CollectionArea");
        _hurtbox = GetNode<Area2D>("Hurtbox");
        _invulnerabilityTimer = GetNode<Timer>("Hurtbox/InvulnerabilityTimer");

        // Subscribe to area overlap signals or Signals
        _magnetArea.AreaEntered += OnMagnetEntered;
        _collectionArea.AreaEntered += OnCollectionEntered;
        _hurtbox.AreaEntered += OnHurtboxEntered;
        _invulnerabilityTimer.Timeout += OnInvulnerabilityEnded;

        //BroadCast the Initial State
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        EmitSignal(SignalName.XpChanged, CurrentXp, XpToNextLevel, CurrentLevel); //Ill be back

    }

    private void OnHurtboxEntered (Area2D area)
    {
        if (area.Name == "Hitbox" && !_isInvulnerable)
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage (int damage)
    {
        if (_isInvulnerable) return;

        CurrentHealth -= damage;

        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        _isInvulnerable = true;
        _invulnerabilityTimer.Start();

        Modulate = new Color (1.0f, 0.2f, 0.2f, 1.0f); // Red Flash

        GD.Print($"PLAYER HIT! Health: {CurrentHealth}/{MaxHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void OnInvulnerabilityEnded ()
    {
        _isInvulnerable = false;
        Modulate = new Color (1, 1, 1, 1); // Reset to normal color
    }

    private void Die ()
    {
        GD.Print("PLAYER OVER: You have Perished");
        GetTree().Paused = true;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!_isInvulnerable && _hurtbox.HasOverlappingAreas())
        {
            foreach (Area2D area in _hurtbox.GetOverlappingAreas())
            {
                if (area.Name == "Hitbox")
                {
                    TakeDamage(10);
                } 
            }
        }
        Vector2 inputVector = Input.GetVector("left", "right", "upward", "downward");

        if (inputVector != Vector2.Zero)
        {
            Velocity = inputVector * Speed;
            _lastFacingDirection = inputVector;
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        _UpdateAnimation();
        MoveAndSlide();
    }

    private void _UpdateAnimation()
    {
        bool isMoving = Velocity != Vector2.Zero;
        string animBase = isMoving ? "walk" : "idle";

        // 1. Toggle active sprite visibility
        _walkSprite.Visible = isMoving;
        _idleSprite.Visible = !isMoving;

        // 2. Determine facing suffix and horizontal flip
        string directionSuffix;
        bool flipH = false;

        if (MathF.Abs(_lastFacingDirection.X) > MathF.Abs(_lastFacingDirection.Y))
        {
            directionSuffix = "side";
            flipH = _lastFacingDirection.X < 0;
        }
        else
        {
            directionSuffix = _lastFacingDirection.Y < 0 ? "up" : "down";
        }

        // 3. Apply flip
        _idleSprite.FlipH = flipH;
        _walkSprite.FlipH = flipH;

        // 4. Play matching animation
        _animationPlayer.Play(animBase + "_" + directionSuffix);
    }

    // --- Signal Callbacks & Progression Logic ---

    private void OnMagnetEntered(Area2D area)
    {
        // If an ExperienceGem enters magnet range, assign this character as its target
        ExperienceGem gem = area as ExperienceGem ?? area.GetParent() as ExperienceGem;
        if (gem != null)
        {
            gem.Target = this;
        }
    }

    private void OnCollectionEntered(Area2D area)
    {
        // When the gem contacts the body, absorb XP and remove the gem from the scene
        ExperienceGem gem = area as ExperienceGem ?? area.GetParent() as ExperienceGem;
        if (gem != null)
        {
            AddExperience(gem.ExperienceValue);
            gem.QueueFree();
        }   
    }

    public void AddExperience(int amount)
    {
        CurrentXp += amount;
        EmitSignal(SignalName.XpChanged, CurrentXp, XpToNextLevel, CurrentLevel);
        GD.Print($"XP: {CurrentXp}/{XpToNextLevel} (Level {CurrentLevel})");

        if (CurrentXp >= XpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {

        CurrentLevel++;
        CurrentXp -= XpToNextLevel;
        XpToNextLevel = (int)(XpToNextLevel * 1.5f); // Increase XP threshold by 50% per level
        
        EmitSignal(SignalName.XpChanged, CurrentXp, XpToNextLevel, CurrentLevel);
        EmitSignal(SignalName.LeveledUp);

        GD.Print($"LEVELED UP! Reached Level {CurrentLevel}. Next threshold: {XpToNextLevel} XP");
    }

    public void UpgradeSpeed (int bonus)
    {
        Speed += bonus;
        GD.Print($"Upgraded Speed! -- {Speed} -- ");

    }

    public void UpgradeHealth (int bonus)
    {
        MaxHealth += bonus;

        CurrentHealth = Mathf.Min(CurrentHealth + bonus, MaxHealth);
        EmitSignal (SignalName.HealthChanged, CurrentHealth, MaxHealth);

        GD.Print($"Upgrade Health -- {MaxHealth} --");

    }

}