using Godot;
using System;

public partial class BloodSplash : AnimatedSprite2D
{
    private static readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();

        // Juice: Random rotation so every hit looks unique
        Rotation = _rng.RandfRange(0f, Mathf.Tau);

        // Juice: Slight size variation (85% to 125% scale)
        float scaleVariance = _rng.RandfRange(0.85f, 1.25f);
        Scale = new Vector2(scaleVariance, scaleVariance);

        // Automatically clean up from memory when the splash frames finish
        AnimationFinished += QueueFree;
        Play();
    }
}