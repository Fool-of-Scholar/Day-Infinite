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
		ResourceLoader.LoadThreadedRequest(TargetScenePath);

		var progressArray = new Godot.Collections.Array();

		while (true)
		{
			var status = ResourceLoader.LoadThreadedGetStatus(TargetScenePath, progressArray);

			float progress = 0f;
			if (progressArray.Count > 0)
			{
				progress = (float)progressArray[0];
			}

			_bar.Value = progress * 100;

			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				break; //Done
			}
			else if(status == ResourceLoader.ThreadLoadStatus.Failed)
			{
				GD.PrintErr("Failed to load Large scene");
			}

			await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
		}
		var packedScene = ResourceLoader.LoadThreadedGet(TargetScenePath) as PackedScene;
		GetTree().ChangeSceneToPacked(packedScene);
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
