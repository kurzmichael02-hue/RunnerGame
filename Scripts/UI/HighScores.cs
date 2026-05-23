using Godot;
using System;

public partial class HighScores : Control
{
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(
			SoundManager.Instance.GameOverMusic
		);

		// LABELS
		var globalTitleLabel =
			GetNode<Label>("GlobalTitle");

		var highscoreLabel =
			GetNode<Label>("VBoxContainer/HighScoreLabel");

		var bestTimeLabel =
			GetNode<Label>("VBoxContainer/BestTimeLabel");

		var sessionLabel =
			GetNode<Label>("VBoxContainer/SessionHighScoreLabel");

		// LEVEL NAME
		string levelName =
			Player.CurrentLevelPath
				.GetFile()
				.GetBaseName();

		string displayName = levelName switch
		{
			"Level1" => "LEVEL 1",
			"TestLevel" => "TEST LEVEL",
			_ => levelName.ToUpper()
		};

		// TITLE
		globalTitleLabel.Text = displayName;

		// LEVEL HIGHSCORE
		int levelHighscore = LoadLevelHighscore();

		highscoreLabel.Text =
			$"Highscore: {levelHighscore}";

		// SESSION SCORE
		int session = LoadLevelSession();

		sessionLabel.Text = $"Session: {session}";

		// BEST TIME
		float best = LoadBestTime();

		bestTimeLabel.Text = best >= float.MaxValue
			? "Best Time: --:--"
			: $"Best Time: {(int)(best / 60):00}:{(int)(best % 60):00}";

		// BUTTON HOVER SOUND
		var vbox =
			GetNode<VBoxContainer>("VBoxContainer");

		foreach (Node child in vbox.GetChildren())
		{
			if (child is Button button)
			{
				button.MouseEntered +=
					OnAnyButtonHovered;
			}
		}

		// AUTO FOCUS
		GetNode<Button>("MainMenu")
			.CallDeferred(Button.MethodName.GrabFocus);
	}

	private void OnAnyButtonHovered()
	{
		SoundManager.Instance.PlayMenuHover();
	}

	// LEVEL HIGHSCORE
	private int LoadLevelHighscore()
	{
		string levelName =
			Player.CurrentLevelPath
				.GetFile()
				.GetBaseName();

		string path =
			$"user://{levelName}_highscore.dat";

		if (!FileAccess.FileExists(path))
			return 0;

		using var file =
			FileAccess.Open(path, FileAccess.ModeFlags.Read);

		return (int)(uint)file.Get32();
	}
	
	private int LoadLevelSession()
	{
		string levelName =
			Player.CurrentLevelPath
				.GetFile()
				.GetBaseName();

		string path =
			$"user://{levelName}_session.dat";

		if (!FileAccess.FileExists(path))
			return 0;

		using var file =
			FileAccess.Open(path, FileAccess.ModeFlags.Read);

		return (int)(uint)file.Get32();
	}

	// BEST TIME
	private float LoadBestTime()
	{
		string levelName =
			Player.CurrentLevelPath
				.GetFile()
				.GetBaseName();

		string path =
			$"user://{levelName}_time.dat";

		if (!FileAccess.FileExists(path))
			return float.MaxValue;

		using var file =
			FileAccess.Open(path, FileAccess.ModeFlags.Read);

		return file.GetFloat();
	}

	// MAIN MENU BUTTON
	private void _on_mainMenu_pressed()
	{
		SoundManager.Instance.PlayButton();

		GetTree().ChangeSceneToFile(
			"res://Scenes/Main/MainMenu.tscn"
		);
	}
}
