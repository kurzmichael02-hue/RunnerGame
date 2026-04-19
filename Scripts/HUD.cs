using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private Label _timerLabel;
	private Label _positionLabel;
	private Player _player;
	private float _elapsedTime = 0f;
	public float ElapsedTime => _elapsedTime;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_livesLabel = GetNode<Label>("LivesLabel");
		_timerLabel = GetNode<Label>("TimerLabel");
		_positionLabel = GetNode<Label>("PositionLabel");
		_player = GetNode<Player>("/root/Node2D/Player");
		// Always so the pause menu (child of HUD) still receives input while the tree is paused
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Process(double delta)
	{
		_scoreLabel.Text = "Score: " + _player.Score;
		_livesLabel.Text = "Lives: " + _player.Lives;

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
