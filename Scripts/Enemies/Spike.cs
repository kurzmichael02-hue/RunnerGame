using Godot;

public partial class Spike : Area2D
{
	private Player _playerInside;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	// BodyEntered feuert nur beim Eintritt — wenn der Player gerade invincible ist
	// (respawn-i-frames), passiert gar nichts und die Spikes töten nie.
	// Deshalb: Player tracken und jeden Frame prüfen sobald i-frames ablaufen.
	public override void _PhysicsProcess(double delta)
	{
		if (_playerInside == null) return;
		if (_playerInside.IsDying || _playerInside.IsInvincible) return;

		_playerInside.CallDeferred(nameof(Player.DieFall));
		_playerInside = null;
	}

	private void OnBodyEntered(Node2D body)
	{
		// nur player — enemies sollen nicht durch spikes sterben (michi will's so)
		if (body is not Player player) return;
		_playerInside = player;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player) _playerInside = null;
	}
}
