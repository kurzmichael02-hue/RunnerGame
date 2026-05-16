using Godot;
using System;

public partial class LevelCompleteScreen : CanvasLayer
{
	private Player _player;
	private HUD _hud;

	private Label _scoreLabel;
	private Label _livesLabel;
	private Label _timeLabel;
	private bool _changingScene = false;

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
		//GetNode<Button>("Panel/VBoxContainer/MainMenu").Pressed += OnMainMenu;
		//GetNode<Button>("Panel/VBoxContainer/Retry").Pressed += OnRetry;
		
		GetNode<Button>("Panel/VBoxContainer/MainMenu").Pressed += OnMainMenu;

		GetNode<Button>("Panel/VBoxContainer/Retry").Pressed += OnRetry;
		ConnectHoverRecursive(this);
	}
	
	public enum MenuSource
	{
		PauseMenu,
		Settings
	}

	public void ShowScreen()
{

	if (_shown) return;

	_shown = true;
	Visible = true;
	
	if (SoundManager.Instance != null)
	{
		SoundManager.Instance.SwitchMusic(
			SoundManager.Instance.GameOverMusic
		);
	}

	_scoreLabel.Text = "Score: " + _player.Score;
	_livesLabel.Text = "Lives: " + _player.Lives;

	float time = _hud.ElapsedTime;
	
	int minutes = (int)(time / 60);
	int seconds = (int)(time % 60);

	_timeLabel.Text = $"Time: {minutes:00}:{seconds:00}";
}




private void OnMainMenu()
{
	ChangeSceneSafe(
		"res://Scenes/Main/MainMenu.tscn"
	);
}

private void ChangeSceneSafe(string path)
{
	if (_changingScene)
		return;

	_changingScene = true;

	if (!IsInsideTree())
		return;

	if (SoundManager.Instance != null)
		SoundManager.Instance.PlayButton();

	LevelGoal.Reset();

	GetTree().Paused = false;

	CallDeferred(
		nameof(DeferredSceneChange),
		path
	);
}

private void DeferredSceneChange(string path)
{
	if (!IsInsideTree())
		return;

	SceneTree tree = GetTree();

	if (tree == null)
		return;

	tree.ChangeSceneToFile(path);
}


private void OnRetry()
{
	RetrySceneSafe();
}

private void RetrySceneSafe()
{
	if (_changingScene)
		return;

	_changingScene = true;

	if (!IsInsideTree())
		return;

	if (SoundManager.Instance != null)
		SoundManager.Instance.PlayButton();

	LevelGoal.Reset();

	GetTree().Paused = false;

	CallDeferred(nameof(DeferredRetry));
}

private void DeferredRetry()
{
	if (!IsInsideTree())
		return;

	SceneTree tree = GetTree();

	if (tree == null)
		return;

	tree.ReloadCurrentScene();
}
private void ConnectHoverRecursive(Node node)
{
	foreach (Node child in node.GetChildren())
	{
		if (child is Button button)
		{
			button.MouseEntered += () =>
			{
				if (SoundManager.Instance != null)
					SoundManager.Instance.PlayMenuHover();
			};
		}

		ConnectHoverRecursive(child);
	}
}

}
