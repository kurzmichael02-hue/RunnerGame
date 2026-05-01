using Godot;

public partial class Spike : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		if (player.IsDying) return;
		// Respawn-i-frames: vermeidet einen direkt-tod wenn der spieler auf einem
		// spike-feld respawnt und der trigger sofort wieder feuert
		if (player.IsInvincible) return;

		// Spikes always cost a life, even when big – like falling off the map (#105)
		player.CallDeferred(nameof(Player.DieFall));
	}
}
