using Godot;
using System;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	// START BUTTON
	private void _on_start_pressed()
	{
		GD.Print("Start gedrückt");
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	// SETTINGS BUTTON
	private void _on_settings_pressed()
	{
		GD.Print("Settings gedrückt");
	}

	// HIGHSCORES BUTTON
	private void _on_highscores_pressed()
	{
		GD.Print("Highscores gedrückt");
	}

	// EXIT BUTTON
	private void _on_exit_pressed()
	{
		GD.Print("Spiel wird beendet");
		GetTree().Quit();
	}
}
