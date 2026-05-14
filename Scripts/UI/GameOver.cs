using Godot;
using System;

public partial class GameOver : Control
{
	private ConfirmationDialog _exitDialog;
	
	public override void _Ready()
	{
SoundManager.Instance?.SwitchMusic(SoundManager.Instance.GameOverMusic);
		_exitDialog = GetNode<ConfirmationDialog>("ExitConfirmDialog");
_exitDialog.Confirmed += OnExitConfirmed;
		
		// run/alltime/session zeile für zeile - sonst läuft das in einen unleserlichen
		// mega-string. styling sitzt in der scene, hier nur text setzen
		var finalLabel = GetNodeOrNull<Label>("FinalScoreLabel");
		if (finalLabel != null)
		{
			int run = Player.LastRunScore;
			int best = Player.LoadHighscore();
			int session = Player.SessionHighscore;

			string topLine = (run > 0 && run >= best)
				? $"Score: {run} ★ NEW HIGHSCORE ★"
				: $"Score: {run}";

			finalLabel.Text =
				$"{topLine}\n" +
				$"Alltime: {best}\n" +
				$"Session: {session}";
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


		// RESTART BUTTON
		private void _on_restart_pressed()
		{
			SoundManager.Instance.PlayButton();
			GetTree().ChangeSceneToFile(Player.CurrentLevelPath);
		}

		// Main Menu pressed
		private void _on_mainMenu_pressed()
		{
			SoundManager.Instance.PlayButton();
			GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
		}

		// EXIT BUTTON
		
		private void _on_exit_pressed()
	{
		SoundManager.Instance.PlayButton();
		_exitDialog.PopupCentered();
	}
	
		//CONFIRM EXIT
	private void OnExitConfirmed()
	{
		GetTree().Quit();
	}
}
