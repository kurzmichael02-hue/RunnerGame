using Godot;

public partial class Checkpoint : Area2D
{
	private bool _activated = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_activated) return;
		if (body is not Player player) return;

		_activated = true;
		player.SetCheckpoint(GlobalPosition);
		// visual feedback – turn green
		GetNode<Polygon2D>("Polygon2D").Color = new Color(0, 1, 0);
		SoundManager.Instance.PlayCheckpoint();
	}
}
