using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// ScenePreloader Singleton (Autoload)
/// Asynchronously loads and caches gameplay and UI scenes in the background
/// during the Opening Screen, Main Menu, and Chooser screens so gameplay
/// transitions are instantaneous with zero disk I/O stalls or stutter.
/// </summary>
public partial class ScenePreloader : Node
{
    public static ScenePreloader Instance { get; private set; }

    private static readonly Dictionary<string, PackedScene> _cachedScenes = new();
    private static readonly HashSet<string> _pendingPaths = new();

    // Priority gameplay scenes to preload as soon as the game opens
    private static readonly string[] PriorityScenes = new string[]
    {
        "res://scene/levels/area.tscn",
        "res://scene/levels/area_2.tscn",
        "res://scene/main_menu/loading_screen.tscn",
        "res://scene/player.tscn",
        "res://scene/HUD.tscn"
    };

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        StartPreloading();
    }

    /// <summary>
    /// Begins asynchronous background loading of all priority gameplay scenes.
    /// </summary>
    public void StartPreloading()
    {
        foreach (string path in PriorityScenes)
        {
            PreloadSceneAsync(path);
        }
    }

    /// <summary>
    /// Enqueues a scene for background threaded loading.
    /// </summary>
    public static void PreloadSceneAsync(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (_cachedScenes.ContainsKey(path) && _cachedScenes[path] != null) return;
        if (_pendingPaths.Contains(path)) return;

        if (ResourceLoader.Exists(path))
        {
            _pendingPaths.Add(path);
            Error err = ResourceLoader.LoadThreadedRequest(path, useSubThreads: true);
            if (err == Error.Ok || err == Error.AlreadyInUse)
            {
                GD.Print($"[ScenePreloader] Queued background preload for: {path}");
            }
            else
            {
                GD.Print($"[ScenePreloader] LoadThreadedRequest returned {err} for {path}");
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_pendingPaths.Count == 0) return;

        List<string> finished = null;
        var progress = new Godot.Collections.Array();

        foreach (string path in _pendingPaths)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(path, progress);
            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                var scene = ResourceLoader.LoadThreadedGet(path) as PackedScene;
                if (scene != null)
                {
                    _cachedScenes[path] = scene;
                    GD.Print($"[ScenePreloader] Finished preloading & cached in memory: {path}");
                }
                finished ??= new List<string>();
                finished.Add(path);
            }
            else if (status == ResourceLoader.ThreadLoadStatus.Failed || status == ResourceLoader.ThreadLoadStatus.InvalidResource)
            {
                GD.PrintErr($"[ScenePreloader] Preload failed for: {path} (status: {status})");
                finished ??= new List<string>();
                finished.Add(path);
            }
        }

        if (finished != null)
        {
            foreach (string path in finished)
            {
                _pendingPaths.Remove(path);
            }
        }
    }

    /// <summary>
    /// Returns the preloaded PackedScene immediately from memory cache if available.
    /// Falls back to threaded loader or direct GD.Load if not yet cached.
    /// </summary>
    public static PackedScene GetPreloadedScene(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // 1. Check RAM cache
        if (_cachedScenes.TryGetValue(path, out var scene) && scene != null)
        {
            return scene;
        }

        // 2. Check if threaded loader has finished it
        var status = ResourceLoader.LoadThreadedGetStatus(path);
        if (status == ResourceLoader.ThreadLoadStatus.Loaded)
        {
            var loaded = ResourceLoader.LoadThreadedGet(path) as PackedScene;
            if (loaded != null)
            {
                _cachedScenes[path] = loaded;
                return loaded;
            }
        }

        // 3. Fallback direct load
        if (ResourceLoader.Exists(path))
        {
            var fallback = GD.Load<PackedScene>(path);
            if (fallback != null)
            {
                _cachedScenes[path] = fallback;
                return fallback;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if the scene is already compiled and ready in RAM.
    /// </summary>
    public static bool HasPreloaded(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (_cachedScenes.ContainsKey(path) && _cachedScenes[path] != null) return true;
        return ResourceLoader.LoadThreadedGetStatus(path) == ResourceLoader.ThreadLoadStatus.Loaded;
    }
}
