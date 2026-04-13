using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private Label _timerLabel;
	private Player _player;
	private float _elapsedTime = 0f;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_livesLabel = GetNode<Label>("LivesLabel");
		_timerLabel = GetNode<Label>("TimerLabel");
		_player = GetNode<Player>("/root/Node2D/Player");
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
		}

		if (Input.IsActionJustPressed("ui_cancel"))
			TogglePause();
	}

	private void TogglePause()
	{
		bool isPaused = !GetTree().Paused;
		GetTree().Paused = isPaused;
		GetNode<Control>("PauseMenu").Visible = isPaused;
		if (isPaused)
			SoundManager.Instance.SwitchMusic(SoundManager.Instance.SettingsMusic);
		else
			SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
	}
}
