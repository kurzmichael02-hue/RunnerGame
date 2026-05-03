using Godot;
using System;

public partial class HighScores : Control
{
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameOverMusic);

		int highscore = LoadHighscore();
		var highscoreLabel = GetNode<Label>("HighScoreLabel");
		// Mischa wollte session + gesamt highscore klar getrennt sehen, schön formatiert.
		// Multi-line statt einer langen zeile, damit auf der highscore-screen sofort
		// klar ist was alltime ist und was die laufende session.
		highscoreLabel.Text =
			$"Alltime:  {highscore}\n" +
			$"Session:  {Player.SessionHighscore}";
		highscoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
		highscoreLabel.AddThemeFontSizeOverride("font_size", 36);
		highscoreLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
		highscoreLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.8f));
		highscoreLabel.AddThemeConstantOverride("shadow_offset_y", 2);
		highscoreLabel.AddThemeConstantOverride("shadow_offset_x", 2);

		// Best time was saved by levelgoal but never read back – now both show up (#68)
		var bestTimeLabel = GetNodeOrNull<Label>("BestTimeLabel");
		if (bestTimeLabel != null)
		{
			float best = LoadBestTime();
			bestTimeLabel.Text = best >= float.MaxValue
				? "Best Time:  --:--"
				: $"Best Time:  {(int)(best / 60):00}:{(int)(best % 60):00}";
			bestTimeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			bestTimeLabel.AddThemeFontSizeOverride("font_size", 32);
			bestTimeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
			bestTimeLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.8f));
			bestTimeLabel.AddThemeConstantOverride("shadow_offset_y", 2);
			bestTimeLabel.AddThemeConstantOverride("shadow_offset_x", 2);
		}

		var vbox = GetNode<VBoxContainer>("VBoxContainer");

		foreach (Node child in vbox.GetChildren())
		{
			if (child is Button button)
			{
				button.MouseEntered += OnAnyButtonHovered;
			}
		}

		// Main Menu button grabs focus so enter/arrow-keys work without the mouse
		GetNode<Button>("VBoxContainer/MainMenu").CallDeferred(Button.MethodName.GrabFocus);
	}

	private void OnAnyButtonHovered()
	{
		SoundManager.Instance.PlayMenuHover();
	}

		public override void _Process(double delta)
		{
		}
		
		private int LoadHighscore()
		{
			string path = "user://highscore.dat";

			if (FileAccess.FileExists(path))
			{
				using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
				return (int)(uint)file.Get32();
			}

			return 0;
		}

		private float LoadBestTime()
		{
			string path = "user://level1_time.dat";
			if (!FileAccess.FileExists(path)) return float.MaxValue;
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			return file.GetFloat();
		}
		

		// Main Menu pressed
		private void _on_mainMenu_pressed()
		{
			GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
		}
}
