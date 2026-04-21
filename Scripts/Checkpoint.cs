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

		// Generous trigger zone so it still registers when the player is jumping over it,
		// running fast with star, or ducking (vertical offset can be 50+ pixels)
		float horizDist = Mathf.Abs(GlobalPosition.X - player.GlobalPosition.X);
		float vertDist = Mathf.Abs(GlobalPosition.Y - player.GlobalPosition.Y);
		if (horizDist < 35f && vertDist < 110f)
		{
			_activated = true;
			// Respawn position is this checkpoint's world position until the next one is hit
			player.SetCheckpoint(GlobalPosition);
			// Visual feedback – tint both polygon and sprite green so it works
			// whether the checkpoint uses schayans sprite or the placeholder polygon (#22)
			var poly = GetNodeOrNull<Polygon2D>("Polygon2D");
			if (poly != null) poly.Color = new Color(0, 1, 0);
			var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (sprite != null) sprite.Modulate = new Color(0.2f, 1f, 0.35f);
			SoundManager.Instance.PlayCheckpoint();
		}
	}
}
