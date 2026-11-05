 using Godot;
using System;

public partial class Movement : CharacterBody2D
{
    // --- Signals ---
    [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    [Signal] public delegate void XpChangedEventHandler(int currentXp, int xpToNextLevel, int currentLevel);
    [Signal] public delegate void LeveledUpEventHandler();
    [Signal] public delegate void ScrapChangedEventHandler(int currentScraps);
    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void DashCooldownChangedEventHandler(float remaining, float total);
    [Signal] public delegate void DashedEventHandler();

    // --- Character Health ---
    [Export] public int MaxHealth { get; set; } = 150;   // Balanced: more buffer for early mistakes
    public int CurrentHealth { get; private set; }

    // --- Death VFX ---
    [ExportGroup("Death")]
    [Export] public PackedScene DeathVfxScene { get; set; }

    private Area2D _hurtbox;
    private Timer _invulnerabilityTimer;
    private bool _isInvulnerable = false;
    private bool _isDead = false;

    // --- Audio Settings (Footsteps) ---
    [ExportGroup("Audio Settings")]
    [Export] private AudioStreamPlayer sfxwalk;
    [Export] public float FootstepInterval { get; set; } = 0.35f; // Seconds between footsteps
    private float _footstepTimer = 0.0f;

    // --- Key Hold Audio & Time Tracking Settings ---
    [ExportGroup("Key Hold Audio & Timer")]
    [Export] private AudioStreamPlayer sfxKeyHold;
    [Export] public Key TargetHoldKey { get; set; } = Key.Shift; // Configurable key in Inspector
    
    private float _keyHoldTime = 0.0f;
    private bool _isKeyHolding = false;

    // --- Movement Settings ---
    [Export] public int Speed { get; set; } = 120;   // Balanced: was 60, survivor genre needs responsive movement

    // --- Dash Settings ---
    [ExportGroup("Dash Settings")]
    [Export] public float DashSpeed { get; set; } = 360f;
    [Export] public float DashDuration { get; set; } = 0.22f;
    [Export] public float DashCooldown { get; set; } = 5.0f; // Balanced: 5.0s base cooldown
    [Export] private AudioStreamPlayer sfxDash;

    private bool _isDashing = false;
    private float _dashTimer = 0.0f;
    private float _dashCooldownTimer = 0.0f;
    private Vector2 _dashDirection = Vector2.Down;

    public bool IsDashing => _isDashing;
    public float DashCooldownRemaining => _dashCooldownTimer;
    public float DashCooldownTotal => DashCooldown;
    public bool HasTakenDamageThisRun { get; private set; } = false;

    // --- Progression & Leveling Fields ---
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXp { get; private set; } = 0;
    public int XpToNextLevel { get; private set; } = 50;   // Balanced: early levels feel fast and rewarding
    public int CurrentScraps { get; private set; } = 0;

    // --- Detection Areas for Pickups ---
    private Area2D _magnetArea;
    private Area2D _collectionArea;

    // --- Visual & Animation References ---
    private AnimationPlayer _animationPlayer;
    private Sprite2D _idleSprite;
    private Sprite2D _walkSprite;

    private Vector2 _lastFacingDirection = Vector2.Down;
    private string _currentFacingSuffix = "down";

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;

        // Cache node references from the scene tree
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _idleSprite = GetNode<Sprite2D>("Idle");
        _walkSprite = GetNode<Sprite2D>("Walk");
        sfxwalk = GetNodeOrNull<AudioStreamPlayer>("sfxwalk");
        sfxKeyHold = GetNodeOrNull<AudioStreamPlayer>("sfxKeyHold");
        sfxDash = GetNodeOrNull<AudioStreamPlayer>("sfxDash");

        // Cache pickup detection areas & hurtbox
        _magnetArea = GetNode<Area2D>("MagnetArea");
        _collectionArea = GetNode<Area2D>("CollectionArea");
        _hurtbox = GetNode<Area2D>("Hurtbox");
        _invulnerabilityTimer = GetNode<Timer>("Hurtbox/InvulnerabilityTimer");

        // Subscribe to area overlap signals
        _magnetArea.AreaEntered += OnMagnetEntered;
        _collectionArea.AreaEntered += OnCollectionEntered;
        _hurtbox.AreaEntered += OnHurtboxEntered;
        _invulnerabilityTimer.Timeout += OnInvulnerabilityEnded;

        // --- Character Selection ---
        ApplySelectedCharacter();

        // Broadcast the Initial State
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        EmitSignal(SignalName.XpChanged, CurrentXp, XpToNextLevel, CurrentLevel);
        EmitSignal(SignalName.ScrapChanged, CurrentScraps);
    }

    private void ApplySelectedCharacter()
    {
        SaveManager sm = GetNodeOrNull<SaveManager>("/root/SaveManager");
        int charIdx = sm?.LoadGame()?._selectedCharacter ?? 0;

        if (charIdx == 1) // DMO
        {
            // Swap to DMO textures
            var dmoIdle = GD.Load<Texture2D>("res://assets/new/south-east idle (1).png");
            var dmoWalk = GD.Load<Texture2D>("res://assets/new/Sprinting SS.png");

            if (dmoIdle != null)
            {
                _idleSprite.Texture = dmoIdle;
                _idleSprite.Hframes = 4;
                _idleSprite.Vframes = 1;
                _idleSprite.Frame = 0;
            }
            if (dmoWalk != null)
            {
                _walkSprite.Texture = dmoWalk;
                _walkSprite.Hframes = 8;
                _walkSprite.Vframes = 1;
                _walkSprite.Frame = 0;
            }

            // Scale DMO down so it matches Null Beta's visual size (~0.5x since 64px vs ~32px)
            Scale = new Vector2(0.5f, 0.5f);
            GD.Print("[Movement] DMO character applied — scaled to 0.5x");
        }
        else
        {
            GD.Print("[Movement] Null Beta character (default)");
        }
    }

    private void OnHurtboxEntered(Area2D area)
    {
        if (area.Name == "Hitbox" && !_isInvulnerable)
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isInvulnerable || _isDead) return;

        HasTakenDamageThisRun = true;
        CurrentHealth -= damage;
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        _isInvulnerable = true;
        _invulnerabilityTimer.Start();

        Modulate = new Color(1.0f, 0.2f, 0.2f, 1.0f); // Red Flash
        GD.Print($"PLAYER HIT! Health: {CurrentHealth}/{MaxHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void OnInvulnerabilityEnded()
    {
        _isInvulnerable = false;
        Modulate = new Color(1, 1, 1, 1); // Reset to normal color
    }

    private async void Die()
    {
        if (_isDead) return;
        _isDead = true;

        GD.Print("PLAYER OVER: You have Perished");

        // Save scraps to the global wallet before dying
        if (CurrentScraps > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddScraps(CurrentScraps);
        }

        // --- 1. Disable further input & physics ---
        SetPhysicsProcess(false);
        SetProcess(false);
        _hurtbox.SetDeferred("monitoring", false);

        // --- 2. Hide the player sprite, spawn death VFX in place ---
        _idleSprite.Visible = false;
        _walkSprite.Visible = false;
        if (DeathVfxScene != null)
        {
            Node2D vfx = DeathVfxScene.Instantiate<Node2D>();
            vfx.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.AddChild(vfx);
            // Auto-free after explosion animation duration
            GetTree().CreateTimer(1.5f).Timeout += () => { if (IsInstanceValid(vfx)) vfx.QueueFree(); };
        }

        // --- 3. Wait for dramatic death explosion to fully play in view ---
        await ToSignal(GetTree().CreateTimer(0.85f), SceneTreeTimer.SignalName.Timeout);

        // --- 4. Signal HUD to show the death screen & pause ---
        EmitSignal(SignalName.PlayerDied);
        GetTree().Paused = true;
    }

    public bool TriggerDash()
    {
        if (_isDashing || _dashCooldownTimer > 0f || _isDead) return false;

        Vector2 inputVector = Input.GetVector("left", "right", "upward", "downward");
        if (inputVector.Length() > 0.15f)
        {
            _dashDirection = inputVector.Normalized();
            _lastFacingDirection = _dashDirection;
        }
        else if (_lastFacingDirection.LengthSquared() > 0.01f)
        {
            _dashDirection = _lastFacingDirection.Normalized();
        }
        else
        {
            _dashDirection = Vector2.Down;
        }

        _isDashing = true;
        _dashTimer = DashDuration;
        _dashCooldownTimer = DashCooldown;
        _isInvulnerable = true;

        if (sfxDash != null && !sfxDash.Playing)
        {
            sfxDash.Play();
        }

        EmitSignal(SignalName.Dashed);
        EmitSignal(SignalName.DashCooldownChanged, _dashCooldownTimer, DashCooldown);
        return true;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Continuous check for overlapping hitboxes while vulnerable
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

        // --- Dash Cooldown Tracking ---
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= (float)delta;
            if (_dashCooldownTimer < 0f) _dashCooldownTimer = 0f;
            EmitSignal(SignalName.DashCooldownChanged, _dashCooldownTimer, DashCooldown);
        }

        // --- Dash Trigger Input ---
        if (Input.IsActionJustPressed("dash") || Input.IsKeyPressed(Key.Space))
        {
            TriggerDash();
        }

        // --- Active Dash Processing ---
        if (_isDashing)
        {
            _dashTimer -= (float)delta;
            Velocity = _dashDirection * DashSpeed;

            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                if (_invulnerabilityTimer.IsStopped())
                {
                    _isInvulnerable = false;
                }
            }

            _UpdateAnimation();
            MoveAndSlide();
            return;
        }

        // --- 1. Movement Input & Footsteps ---
        Vector2 inputVector = Input.GetVector("left", "right", "upward", "downward");
        float deadzone = 0.15f;

        if (inputVector.Length() > deadzone)
        {
            Velocity = inputVector * Speed;
            _lastFacingDirection = inputVector.Normalized();

            // Footstep audio timer logic
            _footstepTimer += (float)delta;
            if (_footstepTimer >= FootstepInterval)
            {
                PlayFootstep();
                _footstepTimer = 0.0f; 
            }
        }
        else
        {
            Velocity = Vector2.Zero;
            _footstepTimer = FootstepInterval; // Ready to play immediately when movement resumes
        }

        // --- 2. Key Hold Audio & Duration Math Logic ---
        if (Input.IsKeyPressed(TargetHoldKey))
        {
            if (!_isKeyHolding)
            {
                _isKeyHolding = true;
                _keyHoldTime = 0.0f;

                if (sfxKeyHold != null && !sfxKeyHold.Playing)
                {
                    sfxKeyHold.Play();
                }
            }

            // Accumulate duration math while the key is held down
            _keyHoldTime += (float)delta;
        }
        else
        {
            if (_isKeyHolding)
            {
                _isKeyHolding = false;
                GD.Print($"[Hold Timer] Key released! Total time held: {_keyHoldTime:F2} seconds.");

                if (sfxKeyHold != null && sfxKeyHold.Playing)
                {
                    sfxKeyHold.Stop();
                }
            }
        }

        _UpdateAnimation();
        MoveAndSlide();
    }

    private void _UpdateAnimation()
    {
        // 1. Determine facing suffix with directional hysteresis
        // Hysteresis prevents diagonal joystick wobble from flipping between side and up/down every frame
        float absX = MathF.Abs(_lastFacingDirection.X);
        float absY = MathF.Abs(_lastFacingDirection.Y);

        if (_currentFacingSuffix == "side")
        {
            // Stay in side unless vertical clearly dominates
            if (absY > absX * 1.35f)
            {
                _currentFacingSuffix = _lastFacingDirection.Y < 0 ? "up" : "down";
            }
        }
        else
        {
            // In up/down: only switch to side if horizontal clearly dominates
            if (absX > absY * 1.35f)
            {
                _currentFacingSuffix = "side";
            }
            else
            {
                _currentFacingSuffix = _lastFacingDirection.Y < 0 ? "up" : "down";
            }
        }

        bool flipH = false;
        if (_currentFacingSuffix == "side")
        {
            flipH = _lastFacingDirection.X < 0;
        }

        // Apply flip to both sprites
        _idleSprite.FlipH = flipH;
        _walkSprite.FlipH = flipH;

        // 2. Dash takes priority
        if (_isDashing)
        {
            string dashAnim = "dash_" + _currentFacingSuffix;
            if (_animationPlayer.HasAnimation(dashAnim))
            {
                if (_animationPlayer.CurrentAnimation != dashAnim)
                {
                    _animationPlayer.Play(dashAnim);
                }
            }
            return;
        }

        // 3. Normal Walk vs Idle
        bool isMoving = Velocity.LengthSquared() > 1.0f;
        string animBase = isMoving ? "walk" : "idle";

        // Toggle active sprite visibility
        _walkSprite.Visible = isMoving;
        _idleSprite.Visible = !isMoving;

        // Play matching animation smoothly without restarting if already playing
        string animName = animBase + "_" + _currentFacingSuffix;
        if (_animationPlayer.HasAnimation(animName))
        {
            if (_animationPlayer.CurrentAnimation != animName)
            {
                _animationPlayer.Play(animName);
            }
        }
    }

    // --- Signal Callbacks & Progression Logic ---

    private void OnMagnetEntered(Area2D area)
    {
        ExperienceGem gem = area as ExperienceGem ?? area.GetParent() as ExperienceGem;
        if (gem != null)
        {
            gem.Target = this;
        }

        ScrapItem scrap = area as ScrapItem ?? area.GetParent() as ScrapItem;
        if (scrap != null)
        {
            scrap.Target = this;
        }

        HealthBattery battery = area as HealthBattery ?? area.GetParent() as HealthBattery;

        if (battery != null)
        {
            battery.Target = this;
        }
    }

    private void OnCollectionEntered(Area2D area)
    {
        ExperienceGem gem = area as ExperienceGem ?? area.GetParent() as ExperienceGem;
        if (gem != null)
        {
            gem.Collect(this);
            return;
        }   

        HealthBattery battery = area as HealthBattery ?? area.GetParent() as HealthBattery;
        if (battery != null)
        {
            battery.Collect(this);
            return;
        }

        ScrapItem scrap = area as ScrapItem ?? area.GetParent() as ScrapItem;
        if (scrap != null)
        {
            scrap.Collect(this);
            return;
        }
    }

    public void AddScrap(int amount)
    {
        CurrentScraps += amount;
        GameManager.Instance?.AddScraps(amount); // Adds to global wallet
        EmitSignal(SignalName.ScrapChanged, CurrentScraps);
        GD.Print($"[Scrap Collected] Run Scraps: {CurrentScraps}");
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        GD.Print($"[Player Healed] Health: {CurrentHealth}/{MaxHealth}");
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
        XpToNextLevel = (int)(XpToNextLevel * 1.3f); // Balanced: 30% increase (was 50%) — more gradual mid-game gating
        
        EmitSignal(SignalName.XpChanged, CurrentXp, XpToNextLevel, CurrentLevel);
        EmitSignal(SignalName.LeveledUp);

        GD.Print($"LEVELED UP! Reached Level {CurrentLevel}. Next threshold: {XpToNextLevel} XP");

    }

    public void UpgradeSpeed(int bonus)
    {
        Speed += bonus;
        GD.Print($"Upgraded Speed! -- {Speed} -- ");
    }

    public void UpgradeHealth(int bonus)
    {
        MaxHealth += bonus;
        CurrentHealth = Mathf.Min(CurrentHealth + bonus, MaxHealth);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        GD.Print($"Upgrade Health -- {MaxHealth} --");
    }

    public void ApplyDashUpgrade(int level)
    {
        if (level <= 0) return;
        // Level 1: 4.4s, Level 2: 3.8s, Level 3: 3.2s, Level 4: 2.6s, Level 5: 2.0s
        DashCooldown = Mathf.Max(2.0f, 5.0f - (level * 0.6f));
        DashSpeed = 360f + (level * 25f);
        GD.Print($"[Passive] Dash Upgraded to Level {level}: Cooldown {DashCooldown:F1}s, Speed {DashSpeed}");
    }

    public void PlayDeathVfxOnly()
    {
    // 1. Stop movement and input
    SetPhysicsProcess(false);
    SetProcess(false);

    // 2. Hide player sprites
    if (_idleSprite != null) _idleSprite.Visible = false;
    if (_walkSprite != null) _walkSprite.Visible = false;

    // 3. Spawn the Death VFX at player's location
    if (DeathVfxScene != null)
        {
            Node2D vfx = DeathVfxScene.Instantiate<Node2D>();
            vfx.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.AddChild(vfx);
            GetTree().CreateTimer(1.5f).Timeout += () => { if (IsInstanceValid(vfx)) vfx.QueueFree(); };
        }
    }


    private void PlayFootstep()
    {
        if (sfxwalk == null || sfxwalk.Stream == null) return;

        // Modulate pitch slightly per step for natural variation
        sfxwalk.PitchScale = (float)GD.RandRange(0.9f, 1.1f);
        sfxwalk.Play();
    }

    // Add this inside Movement.cs
    public void EvolveWeapon()
    {
        WeaponManager weapon = GetNodeOrNull<WeaponManager>("WeaponManager");
        if (weapon != null)
        {
            // Load your new gun and bullet scenes (adjust paths as needed)
            PackedScene rocketBullet = GD.Load<PackedScene>("res://weapon_new/rocket/rocket_bullet.tscn");
            
            GD.Print("Player Weapon Evolved to Rocket!");
        }
        else
        {
            GD.PrintErr("Could not find WeaponManager on Player!");
        }
    }
}