using Godot;

public partial class MagnetPowerUp : Area2D
{
	private bool _initialOverlapChecked = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_initialOverlapChecked) return;
		_initialOverlapChecked = true;
		foreach (var body in GetOverlappingBodies())
			OnBodyEntered(body);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.ActivateMagnet();
		QueueFree();
	}
}
