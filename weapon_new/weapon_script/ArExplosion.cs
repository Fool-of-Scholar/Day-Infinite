using Godot;
using System;

public partial class ArExplosion : GpuParticles2D
{	
	public override void _Ready()
    {
        Emitting = true; // Trigger the particle burst
        
        // Delete this node when the particles finish their lifetime
        Finished += () => QueueFree(); 
    }

}