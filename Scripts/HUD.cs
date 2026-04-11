using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private Player _player;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_livesLabel = GetNode<Label>("LivesLabel");
		_player = GetNode<Player>("/root/Node2D/Player");
		ProcessMode = ProcessModeEnum.Always; // run even when paused
	}

	public override void _Process(double delta)
{
	_scoreLabel.Text = "Score: " + _player.Score;
	_livesLabel.Text = "Lives: " + _player.Lives;

	// ESC toggles pause menu
	if (Input.IsActionJustPressed("ui_cancel"))
	{
		var pauseMenu = GetNode<Control>("PauseMenu");
		bool isPaused = !GetTree().Paused;
		GetTree().Paused = isPaused;
		pauseMenu.Visible = isPaused;
	}
}
}
