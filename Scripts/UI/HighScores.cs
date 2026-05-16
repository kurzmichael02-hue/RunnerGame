using Godot;
using System;

public partial class HighScores : Control
{
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameOverMusic);

		int highscore = LoadHighscore();
		// label liegt jetzt in VBoxContainer (bartolmay hat das vbox-layout gemacht)
		// und mischa will alltime + session zeile für zeile sehen
		var highscoreLabel = GetNode<Label>("VBoxContainer/HighScoreLabel");
		highscoreLabel.Text =
			$"Alltime:  {highscore}\n" +
			$"Session:  {Player.SessionHighscore}";


		 

		// Best time was saved by levelgoal but never read back – now both show up (#68)

		var bestTimeLabel = GetNodeOrNull<Label>("VBoxContainer/BestTimeLabel");
		if (bestTimeLabel != null)
		{
			float best = LoadBestTime();
			bestTimeLabel.Text = best >= float.MaxValue
				? "Best Time:  --:--"
				: $"Best Time:  {(int)(best / 60):00}:{(int)(best % 60):00}";
			// goldener farbton damit die zeit sich vom highscore-block absetzt
			bestTimeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
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
			SoundManager.Instance.PlayButton();
			GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
		}
}
