using Godot;
using System.Collections.Generic;

public partial class VfxPool : Node
{
    // This allows any script in your game to access the pool instantly
    public static VfxPool Instance { get; private set; }

    // A dictionary to store different pools for different explosion types
    private Dictionary<string, Queue<GpuParticles2D>> _pools = new Dictionary<string, Queue<GpuParticles2D>>();

    // NEW: Keep track of how many items we've permanently created
    private Dictionary<string, int> _totalCreated = new Dictionary<string, int>();
    
    // NEW: Your custom limit!
    public int MaxExplosions = 5;

    public override void _Ready()
    {
        Instance = this;
    }

    public void SpawnEffect(PackedScene scene, Vector2 position)
    {
        if (scene == null) return;
        
        // Use the file path (e.g., "res://mine_explosion.tscn") as the unique key
        string key = scene.ResourcePath;
        
        // If we've never seen this explosion type before, create a new queue for it
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GpuParticles2D>();
            _totalCreated[key] = 0; // Start the counter for this specific effect
        }

        GpuParticles2D effect;
        
        // Pull an old explosion out of the pool if one is available
        if (_pools[key].Count > 0)
        {
            effect = _pools[key].Dequeue();
        }
        else // Otherwise, instantiate a new one and tag it with its key
        {

            if (_totalCreated[key] >= MaxExplosions)
            {
                return; 
            }

            effect = scene.Instantiate<GpuParticles2D>();
            effect.SetMeta("pool_key", key); 
            AddChild(effect);

            _totalCreated[key]++; // Increase our total created counter
        }

        // Move it to the target, make it visible, and force it to restart
        effect.GlobalPosition = position;
        effect.Restart(); 
    }

    public void ReturnEffect(GpuParticles2D effect)
    {
        // Stop the particles from drawing
        effect.Emitting = false; 
        
        // Read the tag to know which pool queue to put it back into
        string key = effect.GetMeta("pool_key").AsString();
        
        if (_pools.ContainsKey(key))
        {
            _pools[key].Enqueue(effect);
        }
        else
        {
            effect.QueueFree(); // Fallback failsafe
        }
    }
}