using Godot;


public partial class DamageNumber : Node2D
{
    // Duration the label stays alive (seconds)
    private const float Lifetime = 0.75f;

    // How far upward the number drifts (pixels per second)
    private const float DriftSpeed = 40f;

    private Label _label;
    private float _elapsed = 0f;
    private float _driftX;

    private static readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();
        // Slight horizontal jitter so numbers from rapid hits don't overlap perfectly
        _driftX = _rng.RandfRange(-18f, 18f);

        _label = GetNode<Label>("Label");

        // Start small, punch-in scale effect
        _label.Scale = new Vector2(0.3f, 0.3f);
    }

  
    /// Call immediately after instantiating to set the damage value and optionally mark it as critical.
   
    public void Setup(int damage, bool isCritical = false)
    {
        // Must be called in _Ready or after, so defer if needed
        CallDeferred(MethodName.ApplySetup, damage, isCritical);
    }

    private void ApplySetup(int damage, bool isCritical)
    {
        if (_label == null) _label = GetNode<Label>("Label");

        _label.Text = damage.ToString();

        if (isCritical)
        {
            // Bigger, red-orange for critical hits
            _label.AddThemeColorOverride("font_color", new Color(1.0f, 0.25f, 0.1f));
            _label.AddThemeFontSizeOverride("font_size", 20);
        }
        else
        {
            _label.AddThemeColorOverride("font_color", new Color(1.0f, 0.95f, 0.4f)); // Yellow-white
            _label.AddThemeFontSizeOverride("font_size", 14);
        }
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;
        float t = _elapsed / Lifetime; // 0 → 1 over lifetime

        // Drift upward + jitter
        Position += new Vector2(_driftX * (float)delta, -DriftSpeed * (float)delta);

        // Punch-in scale: quickly grows to 1.2 then settles to 1.0
        float scaleT = Mathf.Clamp(_elapsed / 0.12f, 0f, 1f);
        float scalePunch = scaleT < 0.5f
            ? Mathf.Lerp(0.3f, 1.2f, scaleT / 0.5f)
            : Mathf.Lerp(1.2f, 1.0f, (scaleT - 0.5f) / 0.5f);
        _label.Scale = new Vector2(scalePunch, scalePunch);

        // Fade out in the last 40% of lifetime
        float fadeStart = 0.6f;
        float alpha = t < fadeStart ? 1f : Mathf.Lerp(1f, 0f, (t - fadeStart) / (1f - fadeStart));
        Modulate = new Color(1, 1, 1, alpha);

        if (_elapsed >= Lifetime)
        {
            QueueFree();
        }
    }
}
