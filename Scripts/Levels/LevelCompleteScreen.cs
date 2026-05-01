using Godot;
using System;

public partial class LevelCompleteScreen : CanvasLayer
{
	private Player _player;
	private HUD _hud;

	private Label _scoreLabel;
	private Label _livesLabel;
	private Label _timeLabel;

	private bool _shown = false;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// Sauber über Gruppen holen
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		_hud = GetTree().GetFirstNodeInGroup("hud") as HUD;

		// Labels holen
		_scoreLabel = GetNode<Label>("Panel/VBoxContainer/ScoreLabel");
		_livesLabel = GetNode<Label>("Panel/VBoxContainer/LivesLabel");
		_timeLabel = GetNode<Label>("Panel/VBoxContainer/TimeLabel");

		// Buttons verbinden
		GetNode<Button>("Panel/VBoxContainer/MainMenu").Pressed += OnMainMenu;
		GetNode<Button>("Panel/VBoxContainer/Retry").Pressed += OnRetry;
	}
	
	public void ShowScreen()
{
	if (_shown) return;

	_shown = true;
	Visible = true;

	_scoreLabel.Text = "Score: " + _player.Score;
	_livesLabel.Text = "Lives: " + _player.Lives;

	float time = _hud.ElapsedTime;
	int minutes = (int)(time / 60);
	int seconds = (int)(time % 60);

	_timeLabel.Text = $"Time: {minutes:00}:{seconds:00}";
}


	private void OnMainMenu()
	{
		LevelGoal.Reset();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}

	private void OnRetry()
	{
		LevelGoal.Reset();
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}
