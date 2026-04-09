using Godot;

public partial class PauseMenu : Control
{
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always; // runs even when paused
		GetNode<Button>("VBoxContainer/Resume").Pressed += OnResumePressed;
		GetNode<Button>("VBoxContainer/Settings").Pressed += OnSettingsPressed;
		GetNode<Button>("VBoxContainer/Exit").Pressed += OnExitPressed;
	}

	private void OnResumePressed()
	{
		Visible = false;
		GetTree().Paused = false;
	}

	private void OnSettingsPressed()
	{
		GD.Print("Settings – coming soon");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
