using Godot;

public partial class Spike : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// nur player. enemies sollen nicht durch spielerhindernisse sterben
		// (vorher mal anders gemacht, michi will's so)
		if (body is not Player player) return;
		if (player.IsDying) return;
		// respawn-i-frames - sonst stirbt man direkt wenn man auf einem spike-feld respawnt
		if (player.IsInvincible) return;

		// spikes kosten immer ein leben, auch wenn big - wie pit-fall
		player.CallDeferred(nameof(Player.DieFall));
	}
}
