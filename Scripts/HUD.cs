using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private Label _timerLabel;
	private Label _positionLabel;
	private Label _powerUpLabel;
	private Player _player;
	private float _elapsedTime = 0f;
	public float ElapsedTime => _elapsedTime;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_livesLabel = GetNode<Label>("LivesLabel");
		_timerLabel = GetNode<Label>("TimerLabel");
		_positionLabel = GetNode<Label>("PositionLabel");
		_powerUpLabel = GetNode<Label>("PowerUpLabel");
		// Group lookup instead of absolute path – survives scene-root renames and works
		// even if the level file gets restructured later
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		// Always so the pause menu (child of HUD) still receives input while the tree is paused
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Process(double delta)
	{
		_scoreLabel.Text = "Score: " + _player.Score;
		_livesLabel.Text = "Lives: " + _player.Lives;
		_powerUpLabel.Text = BuildPowerUpText();

		// Timer only runs when not paused
		if (!GetTree().Paused)
		{
			_elapsedTime += (float)delta;
			int minutes = (int)(_elapsedTime / 60);
			int seconds = (int)(_elapsedTime % 60);
			_timerLabel.Text = $"{minutes:00}:{seconds:00}";
			int meters = (int)(_player.Position.X / 50f);
_positionLabel.Text = meters + "m";
		}

		if (Input.IsActionJustPressed("ui_cancel"))
			TogglePause();
	}

// Builds a compact string of all active power-ups with remaining seconds.
// Empty string when nothing is active so the label just disappears visually.
private string BuildPowerUpText()
{
	var parts = new System.Collections.Generic.List<string>();
	if (_player.StarTimeLeft > 0f) parts.Add($"STAR {_player.StarTimeLeft:0.0}s");
	if (_player.ShieldTimeLeft > 0f) parts.Add($"SHIELD {_player.ShieldTimeLeft:0.0}s");
	if (_player.MagnetTimeLeft > 0f) parts.Add($"MAGNET {_player.MagnetTimeLeft:0.0}s");
	return string.Join("  |  ", parts);
}

private void TogglePause()
{
	// After the goal is reached, ESC is dead – stops the player opening pause on the win screen
	if (LevelGoal.LevelCompleted) return;
	bool isPaused = !GetTree().Paused;
	GetTree().Paused = isPaused;
	GetNode<Control>("PauseMenu").Visible = isPaused;
	if (isPaused)
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.SettingsMusic);
	else
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
}
}
