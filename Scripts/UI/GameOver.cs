using Godot;
using System;

public partial class GameOver : Control
{
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameOverMusic);

		// Show the final run score next to the highscore, flag a new record if the run beat it
		var finalLabel = GetNodeOrNull<Label>("FinalScoreLabel");
		if (finalLabel != null)
		{
			int run = Player.LastRunScore;
			int best = Player.LoadHighscore();
			if (run > 0 && run >= best)
				finalLabel.Text = $"Score: {run}   —   NEW HIGHSCORE!";
			else
				finalLabel.Text = $"Score: {run}   |   Highscore: {best}";
		}

		var vbox = GetNode<VBoxContainer>("VBoxContainer");

		foreach (Node child in vbox.GetChildren())
		{
			if (child is Button button)
			{
				button.MouseEntered += OnAnyButtonHovered;
			}
		}

		// Restart takes initial focus so arrow keys + enter work without the mouse
		GetNode<Button>("VBoxContainer/Restart").CallDeferred(Button.MethodName.GrabFocus);
	}

	private void OnAnyButtonHovered()
	{
		SoundManager.Instance.PlayMenuHover();
	}

		public override void _Process(double delta)
		{
		}

		// RESTART BUTTON
		private void _on_restart_pressed()
		{
			
			SoundManager.Instance.PlayButton();
			GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
		}

		// Main Menu pressed
		private void _on_mainMenu_pressed()
		{
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		}

		// EXIT BUTTON
		private void _on_exit_pressed()
		{

			SoundManager.Instance.PlayButton();
			GetTree().Quit();
		}
}
