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
		// One-shot: checkpoint only fires once, otherwise color keeps flipping (#22)
		if (_activated) return;
		if (body is not Player player) return;

		_activated = true;
		// Respawn position is this checkpoint's world position until the next one is hit
		player.SetCheckpoint(GlobalPosition);
		// Visual feedback – turn green so the player sees it registered
		GetNode<Polygon2D>("Polygon2D").Color = new Color(0, 1, 0);
		SoundManager.Instance.PlayCheckpoint();
	}
}
