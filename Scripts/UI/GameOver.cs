using Godot;
using System;

public partial class GameOver : Control
{
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameOverMusic);

		// run + alltime + session, alles in eigener zeile statt einem mega-string
		// (war vorher kaum lesbar). session resettet sich wenn das spiel zu geht
		var finalLabel = GetNodeOrNull<Label>("FinalScoreLabel");
		if (finalLabel != null)
		{
			int run = Player.LastRunScore;
			int best = Player.LoadHighscore();
			int session = Player.SessionHighscore;

			string topLine = (run > 0 && run >= best)
				? $"Score:    {run}   NEW HIGHSCORE"
				: $"Score:    {run}";

			finalLabel.Text =
				$"{topLine}\n" +
				$"Alltime:  {best}\n" +
				$"Session:  {session}";
			finalLabel.HorizontalAlignment = HorizontalAlignment.Center;
			finalLabel.AddThemeFontSizeOverride("font_size", 32);
			finalLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
			finalLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.8f));
			finalLabel.AddThemeConstantOverride("shadow_offset_y", 2);
			finalLabel.AddThemeConstantOverride("shadow_offset_x", 2);
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
			GetTree().ChangeSceneToFile("res://Scenes/Main/Game.tscn");
		}

		// Main Menu pressed
		private void _on_mainMenu_pressed()
		{
			GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
		}

		// EXIT BUTTON
		private void _on_exit_pressed()
		{

			SoundManager.Instance.PlayButton();
			GetTree().Quit();
		}
}
