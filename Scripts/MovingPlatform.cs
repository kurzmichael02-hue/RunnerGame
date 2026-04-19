using Godot;

public partial class MovingPlatform : AnimatableBody2D
{
	// Relative offset the platform swings to and back. Set in the editor per instance.
	[Export] public Vector2 EndOffset = new Vector2(200, 0);
	// Seconds for one full back-and-forth cycle
	[Export] public float CycleTime = 4f;

	private Vector2 _startPosition;
	private float _time = 0f;

	public override void _Ready()
	{
		_startPosition = Position;
		// Respect ESC pause – platform freezes with the rest of the world
		ProcessMode = ProcessModeEnum.Pausable;
		// Interpolate between physics frames so the movement stays smooth
		SyncToPhysics = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		_time += (float)delta;
		// Sine-based ping-pong: eases at both ends so the platform isn't jerky
		float phase = (Mathf.Sin(_time * Mathf.Pi * 2f / CycleTime) + 1f) * 0.5f;
		Position = _startPosition.Lerp(_startPosition + EndOffset, phase);
	}
}
