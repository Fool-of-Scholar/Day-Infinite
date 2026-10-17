using Godot;
using System;
using System.Security.Cryptography.X509Certificates;


public partial class ArExplosion : GpuParticles2D
{	
	public override void _Ready()
    {
        this.Emitting = true; 

        
        
        
        // Trigger the particle burst
        
        // Delete this node when the particles finish their lifetime
        Finished += () => QueueFree(); 
    }

}