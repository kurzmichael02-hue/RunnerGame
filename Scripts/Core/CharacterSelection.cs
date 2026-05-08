using Godot;
using System;

public partial class CharacterSelection : Node
{
	private ConfirmationDialog _exitDialog;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_exitDialog = GetNode<ConfirmationDialog>("ExitConfirmDialog");
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
	private void _on_exit_confirmed()
	{
		GetTree().Quit();
	}
	
}
