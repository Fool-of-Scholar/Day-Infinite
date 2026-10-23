using Godot;


// Controls the screen-space falling leaves effect for Area 2 intro.
// Runs for approx. 1 minute (ActiveDuration), then gently fades out and frees itself.

public partial class IntroLeavesEffect : CanvasLayer
{
    [Export] public float ActiveDuration { get; set; } = 60f; // 1 minute intro
    [Export] public float FadeOutDuration { get; set; } = 4f;

    private CpuParticles2D _particles;
    private float _timer = 0f;
    private bool _isEnding = false;

    public override void _Ready()
    {
        _particles = GetNodeOrNull<CpuParticles2D>("FallingLeaves");
    }

    public override void _Process(double delta)
    {
        if (_isEnding) return;

        _timer += (float)delta;
        if (_timer >= ActiveDuration)
        {
            _isEnding = true;

            // Stop generating new particles
            if (_particles != null)
            {
                _particles.Emitting = false;
            }

            // Smoothly fade out the remaining leaves, then clean up
            Tween tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0f, FadeOutDuration);
            tween.TweenCallback(Callable.From(QueueFree));
        }
    }
}
