using Godot;

public partial class StarPowerUp : Area2D
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
		float s = 1f + 0.15f * Mathf.Sin(_time * Mathf.Pi * 3f);
		Scale = new Vector2(s, s);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.ActivateStar();
		QueueFree();
	}
}
