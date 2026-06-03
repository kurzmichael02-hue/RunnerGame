using Godot;

public partial class SwordPickup : Area2D
{
	private float _time = 0f;

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

	public override void _Process(double delta)
	{
		_time += (float)delta;
		Rotation = 0.18f * Mathf.Sin(_time * Mathf.Pi * 1.6f);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.AddSwordUse();
		SoundManager.Instance.PlayCoin();
		QueueFree();
	}
}
