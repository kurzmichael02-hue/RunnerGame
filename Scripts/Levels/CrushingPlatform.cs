using Godot;

// Variant of MovingPlatform that kills the player on contact while moving downward.
// No one-way collision – the player can't jump through it from underneath either.
public partial class CrushingPlatform : AnimatableBody2D
{
	[Export] public Vector2 EndOffset = new Vector2(0, 80);
	[Export] public float CycleTime = 4f;

	private Vector2 _startPosition;
	private float _time = 0f;

	public override void _Ready()
	{
		_startPosition = Position;
		ProcessMode = ProcessModeEnum.Pausable;
	}

	public override void _PhysicsProcess(double delta)
	{
		_time += (float)delta;
		float phase = (Mathf.Sin(_time * Mathf.Pi * 2f / CycleTime) + 1f) * 0.5f;
		Vector2 target = _startPosition.Lerp(_startPosition + EndOffset, phase);
		Vector2 motion = target - Position;

		// MoveAndCollide pushes whatever's in the way and returns info on what we hit.
		// If we hit the player while moving down, the player gets squished.
		var collision = MoveAndCollide(motion);
		if (collision != null && motion.Y > 0f
			&& collision.GetCollider() is Player player && !player.IsDying)
		{
			player.DieFall();
		}
	}
}
