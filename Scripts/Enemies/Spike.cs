using Godot;

public partial class Spike : Area2D
{
	public override void _Ready()
	{
		// enemies sind auf layer 2, default-mask ist nur 1. ohne das hier triggert
		// der spike nie für gegner
		SetCollisionMaskValue(2, true);
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// mischa wollte das nur für den charger - mach's aber für alle, sonst
		// ist's komisch wenn ein patroller einfach durch spikes durchstapft
		if (body is Enemy enemy)
		{
			enemy.Kill();
			return;
		}

		if (body is not Player player) return;
		if (player.IsDying) return;
		// Respawn-i-frames: vermeidet einen direkt-tod wenn der spieler auf einem
		// spike-feld respawnt und der trigger sofort wieder feuert
		if (player.IsInvincible) return;

		// Spikes always cost a life, even when big – like falling off the map (#105)
		player.CallDeferred(nameof(Player.DieFall));
	}
}
