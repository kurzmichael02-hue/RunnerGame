using Godot;
using System;

public partial class MainMenu : Control
{
	
	private ConfirmationDialog _exitDialog;
	private Label _coinsLabel;
	
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
	
	Player.LoadProfile();
	_coinsLabel = GetNode<Label>("CoinsContainer/CoinsLabel");
	_coinsLabel.Text = $"{Player.Coins}";

	// Start button gets focus so arrow keys + enter work without touching the mouse.
	// Deferred because children aren't fully ready yet inside _Ready.
	GetNode<Button>("VBoxContainer/Start").CallDeferred(Button.MethodName.GrabFocus);
	Controls.LoadSettings();
}

private void OnAnyButtonHovered()
{
	SoundManager.Instance.PlayMenuHover();
}

	// START BUTTON
	private void _on_start_pressed()
	{
		
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/LevelSelection.tscn");
	}

	// SETTINGS BUTTON
	private void _on_settings_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/Settings.tscn");
	}

	// HIGHSCORES BUTTON
	private void _on_highscores_pressed()
	{
		
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/Highscores.tscn");
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

	private void _on_character_selection_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/CharacterSelection.tscn");
	}
}
