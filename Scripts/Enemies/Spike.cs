using Godot;

public partial class Spike : Area2D
{
	public override void _Ready()
	{
		// Default-mask ist nur layer 1 (player). Enemy ist auf layer 2 - ohne mask 2
		// triggert der spike nie für gegner und mischas charger-auf-spike-feature
		// würde garnix machen
		SetCollisionMaskValue(2, true);
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Mischas request: charger der auf spikes läuft soll sterben (oder springen).
		// Ich mach's für alle gegner-typen, ist konsistenter mit klassischem mario.
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
