using Godot;
using System;

// FIXED: GPUParticles2D capitalization
public partial class MineExplosion : GpuParticles2D
{

    private GpuParticles2D _areaExplosion;
    public override void _Ready()
    {
        this.Emitting = true; // Trigger the particle burst
        


        // Delete this node when the particles finish their lifetime to prevent lag
        Finished += () => QueueFree(); 
    }
}