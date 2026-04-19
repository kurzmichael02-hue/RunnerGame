using Godot;
using System;

public partial class HighScores : Control
{
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameOverMusic);

		int highscore = LoadHighscore();
		var highscoreLabel = GetNode<Label>("HighScoreLabel");
		highscoreLabel.Text = $"Highscore: {highscore}";

		var vbox = GetNode<VBoxContainer>("VBoxContainer");

		foreach (Node child in vbox.GetChildren())
		{
			if (child is Button button)
			{
				button.MouseEntered += OnAnyButtonHovered;
			}
		}
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
		

		// Main Menu pressed
		private void _on_mainMenu_pressed()
		{
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		}
}
