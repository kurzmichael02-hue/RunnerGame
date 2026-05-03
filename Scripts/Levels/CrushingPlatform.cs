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

		// die platform hat oben drauf spikes (sieht man im sprite), also jeder
		// kontakt = tödlich. nicht wie die normale moving platform wo man oben
		// drauf landen kann.
		var collision = MoveAndCollide(motion);
		if (collision != null
			&& collision.GetCollider() is Player player
			&& !player.IsDying && !player.IsInvincible)
		{
			player.Die();
		}
	}
}
