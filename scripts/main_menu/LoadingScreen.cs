using Godot;
using System;

public partial class LoadingScreen : Control
{
	public static string NextScenePath { get; set; } = null;
	private SaveManager _saveManager;
	[Export] public string TargetScenePath = "res://scene/levels/area.tscn";
	private TextureProgressBar _bar;

	private const float MinLoadingDuration = 0.85f; // Ensures OS window message pump remains responsive and prevents instant hitching
	private float _elapsedTime = 0.0f;
	private bool _isLoadingStarted = false;
	private bool _isTransitioning = false;
	private bool _isPreloadedReady = false;
	private readonly Godot.Collections.Array _progressArray = new();

	public override void _Ready()
	{
		_saveManager = GetNodeOrNull<SaveManager>("/root/SaveManager");
		TriggerLoad();
		_bar = GetNodeOrNull<TextureProgressBar>("TextureProgressBar");

		if (_bar != null)
		{
			_bar.Value = 0;
		}

		// Ensure we render at least one full frame before starting the load request
		CallDeferred(nameof(StartLoadingProcess));
	}

	private void StartLoadingProcess()
	{
		if (string.IsNullOrEmpty(TargetScenePath) || !ResourceLoader.Exists(TargetScenePath))
		{
			GD.PrintErr($"[LoadingScreen] Target scene '{TargetScenePath}' not found, falling back to Area 1.");
			TargetScenePath = "res://scene/levels/area.tscn";
		}

		// 1. Check if scene is already cached in RAM by ScenePreloader
		if (ScenePreloader.HasPreloaded(TargetScenePath))
		{
			_isPreloadedReady = true;
			GD.Print($"[LoadingScreen] Target scene '{TargetScenePath}' is already preloaded and warm in memory!");
		}
		else
		{
			// 2. Otherwise begin background threaded load
			Error reqErr = ResourceLoader.LoadThreadedRequest(TargetScenePath, useSubThreads: true);
			if (reqErr != Error.Ok && reqErr != Error.AlreadyInUse)
			{
				GD.PrintErr($"[LoadingScreen] LoadThreadedRequest returned: {reqErr}");
			}
		}

		_isLoadingStarted = true;
	}

	public override void _Process(double delta)
	{
		if (!_isLoadingStarted || _isTransitioning) return;

		_elapsedTime += (float)delta;

		bool isReady = _isPreloadedReady || ScenePreloader.HasPreloaded(TargetScenePath);
		float actualLoadProgress = isReady ? 1.0f : 0.0f;

		if (!isReady)
		{
			var status = ResourceLoader.LoadThreadedGetStatus(TargetScenePath, _progressArray);
			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				isReady = true;
				actualLoadProgress = 1.0f;
			}
			else if (_progressArray.Count > 0)
			{
				actualLoadProgress = (float)_progressArray[0];
			}
			else if (status == ResourceLoader.ThreadLoadStatus.Failed || status == ResourceLoader.ThreadLoadStatus.InvalidResource)
			{
				GD.PrintErr($"[LoadingScreen] Threaded load failed for '{TargetScenePath}' (status: {status})");
				_isTransitioning = true;
				SetProcess(false);
				CallDeferred(nameof(FallbackSceneTransition));
				return;
			}
		}

		// Smooth visual progress that takes at least MinLoadingDuration to reach 100%
		float timerProgress = Mathf.Clamp(_elapsedTime / MinLoadingDuration, 0f, 1f);
		float displayedProgress = Mathf.Min(timerProgress, actualLoadProgress);

		if (_bar != null)
		{
			_bar.Value = displayedProgress * 100f;
		}

		// Transition once background load is complete AND minimum display duration has elapsed
		if (isReady && _elapsedTime >= MinLoadingDuration)
		{
			_isTransitioning = true;
			if (_bar != null) _bar.Value = 100f;
			SetProcess(false);
			CallDeferred(nameof(CompleteSceneTransition));
		}
	}

	private void CompleteSceneTransition()
	{
		var packedScene = ScenePreloader.GetPreloadedScene(TargetScenePath) 
		               ?? ResourceLoader.LoadThreadedGet(TargetScenePath) as PackedScene
		               ?? GD.Load<PackedScene>(TargetScenePath);

		if (packedScene == null)
		{
			GD.PrintErr("[LoadingScreen] PackedScene was null, falling back to direct load of Area 1.");
			packedScene = GD.Load<PackedScene>("res://scene/levels/area.tscn");
		}

		if (packedScene != null)
		{
			GetTree().Paused = false; // Always ensure tree is unpaused when loading in
			GetTree().ChangeSceneToPacked(packedScene);
		}
		else
		{
			FallbackSceneTransition();
		}
	}

	private void FallbackSceneTransition()
	{
		GD.PrintErr("[LoadingScreen] Scene transition fallback triggered.");
		GetTree().Paused = false;
		var area1 = ScenePreloader.GetPreloadedScene("res://scene/levels/area.tscn")
		         ?? GD.Load<PackedScene>("res://scene/levels/area.tscn");
		if (area1 != null)
		{
			GetTree().ChangeSceneToPacked(area1);
		}
		else
		{
			GetTree().ChangeSceneToFile("res://scene/main_menu/main_menu.tscn");
		}
	}

	public void TriggerLoad()
	{
		if (!string.IsNullOrEmpty(NextScenePath))
		{
			TargetScenePath = NextScenePath;
			NextScenePath = null;
			GD.Print($"[LoadingScreen] Target area passed via fast memory path: {TargetScenePath}");
			return;
		}

		if (_saveManager != null)
		{
			SaveData loadedData = _saveManager.LoadGame();
			if (loadedData != null && !string.IsNullOrEmpty(loadedData._areaChoosed))
			{
				TargetScenePath = loadedData._areaChoosed;
				GD.Print($"[LoadingScreen] Target area loaded from save: {loadedData._areaChoosed}");
			}
		}
	}
}
