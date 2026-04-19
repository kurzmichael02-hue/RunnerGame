using Godot;

public partial class Checkpoint : Area2D
{
	private bool _activated = false;

	public override void _Process(double delta)
	{
		// Distance-based trigger instead of BodyEntered – more reliable with
		// overlapping areas, fast-moving players, and physics-layer mismatches.
		if (_activated) return;

		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.IsDying) return;

		if (GlobalPosition.DistanceTo(player.GlobalPosition) < 45f)
		{
			_activated = true;
			// Respawn position is this checkpoint's world position until the next one is hit
			player.SetCheckpoint(GlobalPosition);
			// Visual feedback – turn green so the player sees it registered (#22)
			GetNode<Polygon2D>("Polygon2D").Color = new Color(0, 1, 0);
			SoundManager.Instance.PlayCheckpoint();
		}
	}
}
