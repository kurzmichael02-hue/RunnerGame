using Godot;

public partial class MovingPlatform : AnimatableBody2D
{
	// Relative offset the platform swings to and back. Set per instance in the editor.
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
	}

	public override void _PhysicsProcess(double delta)
	{
		_time += (float)delta;
		// Sine-based ping-pong so the platform eases at both ends, not jerky
		float phase = (Mathf.Sin(_time * Mathf.Pi * 2f / CycleTime) + 1f) * 0.5f;
		// Direct position set – the one_way_collision + margin on the shape keeps the
		// player perched on top, no need for moveandcollide physics coupling
		Position = _startPosition.Lerp(_startPosition + EndOffset, phase);
	}
}
