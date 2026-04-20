using Godot;
using System;

public partial class MainMenu : Control
{
	
	private ConfirmationDialog _exitDialog;
	
public override void _Ready()
{
	SoundManager.Instance.SwitchMusic(SoundManager.Instance.StartScreenMusic);

	var vbox = GetNode<VBoxContainer>("VBoxContainer");

	foreach (Node child in vbox.GetChildren())
	{
		if (child is Button button)
		{
			button.MouseEntered += OnAnyButtonHovered;
		}
	}
	
	_exitDialog = GetNode<ConfirmationDialog>("ExitConfirmDialog");
	_exitDialog.Confirmed += OnExitConfirmed;

	// Start button gets focus so arrow keys + enter work without touching the mouse.
	// Deferred because children aren't fully ready yet inside _Ready.
	GetNode<Button>("VBoxContainer/Start").CallDeferred(Button.MethodName.GrabFocus);
}

private void OnAnyButtonHovered()
{
	SoundManager.Instance.PlayMenuHover();
}

	public override void _Process(double delta)
	{
	}

	// START BUTTON
	private void _on_start_pressed()
	{
		
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	// SETTINGS BUTTON
	private void _on_settings_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Settings.tscn");
	}

	// HIGHSCORES BUTTON
	private void _on_highscores_pressed()
	{
		
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/HighScores.tscn");
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
