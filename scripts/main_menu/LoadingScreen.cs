using Godot;
using System;

public partial class LoadingScreen : Control
{

	private SaveManager _saveManager;
	[Export] public string TargetScenePath = "res://scene/levels/area.tscn";
	private TextureProgressBar _bar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_saveManager = GetNode<SaveManager>("/root/SaveManager");
		TriggerLoad();
		_bar = GetNode<TextureProgressBar>("TextureProgressBar");
		LoadScene();
	}

	private async void LoadScene()
	{
		if (string.IsNullOrEmpty(TargetScenePath) || !ResourceLoader.Exists(TargetScenePath))
		{
			GD.PrintErr($"[LoadingScreen] Target scene '{TargetScenePath}' not found, falling back to Area 1.");
			TargetScenePath = "res://scene/levels/area.tscn";
		}

		Error reqErr = ResourceLoader.LoadThreadedRequest(TargetScenePath);
		if (reqErr != Error.Ok)
		{
			GD.PrintErr($"[LoadingScreen] LoadThreadedRequest failed with error: {reqErr}");
			TargetScenePath = "res://scene/levels/area.tscn";
			ResourceLoader.LoadThreadedRequest(TargetScenePath);
		}

		var progressArray = new Godot.Collections.Array();

		while (true)
		{
			var status = ResourceLoader.LoadThreadedGetStatus(TargetScenePath, progressArray);

			float progress = 0f;
			if (progressArray.Count > 0)
			{
				progress = (float)progressArray[0];
			}

			if (_bar != null)
			{
				_bar.Value = progress * 100;
			}

			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				break; // Done
			}
			else if (status == ResourceLoader.ThreadLoadStatus.Failed || status == ResourceLoader.ThreadLoadStatus.InvalidResource)
			{
				GD.PrintErr($"[LoadingScreen] Threaded load failed for '{TargetScenePath}' (status: {status})");
				break;
			}

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		var packedScene = ResourceLoader.LoadThreadedGet(TargetScenePath) as PackedScene;
		if (packedScene == null)
		{
			GD.PrintErr("[LoadingScreen] PackedScene was null, falling back to direct load of Area 1.");
			packedScene = GD.Load<PackedScene>("res://scene/levels/area.tscn");
		}

		if (packedScene != null)
		{
			GetTree().ChangeSceneToPacked(packedScene);
		}
		else
		{
			GD.PrintErr("[LoadingScreen] All scene loads failed! Returning to Main Menu.");
			GetTree().ChangeSceneToFile("res://scene/main_menu/main_menu.tscn");
		}
	}
	public void TriggerLoad()
	{
		if (_saveManager != null)
		{
			SaveData loadedData = _saveManager.LoadGame();
			TargetScenePath = loadedData._areaChoosed;
			GD.Print($"{loadedData._areaChoosed}");
		}
	}

}
