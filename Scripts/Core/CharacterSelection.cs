using Godot;
using System;

public partial class CharacterSelection : Node
{
	private ConfirmationDialog _exitDialog;
	private Label _selectedLabel;
	private string _selectedCharacter = "Default";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_exitDialog = GetNode<ConfirmationDialog>("ExitConfirmDialog");
		_selectedLabel = GetNode<Label>("SelectedLabel");
		_selectedLabel.Text = "Selected: " + _selectedCharacter;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void _on_main_menu_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}
	
	private void _on_exit_pressed()
	{
		SoundManager.Instance.PlayButton();
		_exitDialog.PopupCentered(); 
	}
	private void _on_button_pressed()
	{
		SelectCharacter("Player 1");
	}
	private void _on_button_2_pressed()
	{
		SelectCharacter("Player 2");
	}
	private void SelectCharacter(string characterName)
	{
	_selectedCharacter = characterName;
	_selectedLabel.Text = "Selected: " + characterName; 
	}
		
	private void _on_exit_confirmed()
	{
		GetTree().Quit();
	}
	
}
