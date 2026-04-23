using Godot;

// Attached to the Game scene root – handles level-wide setup.
// Right now just swaps to the gameplay music when the level loads.
public partial class GameManager : Node2D
{
	private PauseMenu _pauseMenu;
	private PackedScene _pauseMenuScene;
	
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		// Fresh run – clear the level-won flag that would otherwise block ESC pause after a retry
		LevelGoal.Reset();
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
	
		_pauseMenuScene = GD.Load<PackedScene>("res://Scenes/PauseMenu.tscn");
			
	}
	private void EnsurePauseMenu()
	{
		if (_pauseMenu != null) return;

		_pauseMenu = _pauseMenuScene.Instantiate<PauseMenu>();
		GetTree().Root.AddChild(_pauseMenu); // GANZ WICHTIG: Root, nicht CurrentScene

		_pauseMenu.Visible = false;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed && !key.Echo)
		{
			if (Input.IsActionPressed("ui_cancel"))
			{
				TogglePause();
			}
		}
	}
	
	private void TogglePause()
	{
		if (LevelGoal.LevelCompleted) return;

		EnsurePauseMenu();

		if (GetTree().Paused)
		{
			GetTree().Paused = false;
			_pauseMenu.Visible = false;
			SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
		}
		else
		{
			GetTree().Paused = true;
			_pauseMenu.Visible = true;
			SoundManager.Instance.SwitchMusic(SoundManager.Instance.SettingsMusic);
		}
	}
	
	
}
