using Godot;
using System;

public partial class BlackholeExplosion : GpuParticles2D
{

    public override void _Ready()
        {
            // When the particle animation finishes, destroy itself
            Finished += () => QueueFree();
        }

}
