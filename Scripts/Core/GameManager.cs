using Godot;

// Zentraler Controller für Pause + Musik + Levelstart
public partial class GameManager : Node2D
{
	private PauseMenu _pauseMenu;
	private PackedScene _pauseMenuScene;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		AddToGroup("game_manager");

		// Reset nach Retry
		LevelGoal.Reset();

		// Startmusik
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);


		_pauseMenuScene = GD.Load<PackedScene>("res://Scenes/Main/PauseMenu.tscn");
	}

	private void EnsurePauseMenu()
	{
		if (_pauseMenu != null) return;

		_pauseMenu = _pauseMenuScene.Instantiate<PauseMenu>();

		// WICHTIG: auf Root → funktioniert auch wenn pausiert
		GetTree().Root.AddChild(_pauseMenu);

		_pauseMenu.Visible = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
	}

	public void TogglePause()
	{
		if (LevelGoal.LevelCompleted) return;

		EnsurePauseMenu();

		bool isPaused = GetTree().Paused;

		if (isPaused)
		{
			// RESUME
			GetTree().Paused = false;

			if (_pauseMenu != null)
			{
				_pauseMenu.Visible = false;
				_pauseMenu.QueueFree();
				_pauseMenu = null;
			}

			// Musik zurück – Star hat Vorrang, sonst normale Spielmusik
			var player = GetTree().GetFirstNodeInGroup("player") as Player;
			bool starActive = player != null && (player.StarActive || player.StarInvincibilityActive);
			SoundManager.Instance.SwitchMusic(starActive
				? SoundManager.Instance.StarMusic
				: SoundManager.Instance.GameMusic);
		}
		else
		{
			// PAUSE
			GetTree().Paused = true;

			if (_pauseMenu != null)
				_pauseMenu.Visible = true;

			// Pause-Musik
			SoundManager.Instance.SwitchMusic(SoundManager.Instance.SettingsMusic);
		}
	}
}
