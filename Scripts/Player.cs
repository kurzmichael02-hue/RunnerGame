using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float InitialSpeed = 300.0f;
	[Export] public float MaxSpeed = 800.0f;
	[Export] public float SpeedIncreaseRate = 10.0f;
	[Export] public float JumpVelocity = -600.0f;

	private float _currentSpeed;
	private bool _isSliding = false;
	private Vector2 _gravity;

	public override void _Ready()
	{
		_currentSpeed = InitialSpeed;
		_gravity = GetGravity();
		GD.Print("Player Position: " + Position);
	}

	public override void _PhysicsProcess(double delta)
	{
		GD.Print("OnFloor: " + IsOnFloor() + " Velocity: " + Velocity);
		Vector2 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity += _gravity * (float)delta;

		// Auto-run (Story #8)
		if (_currentSpeed < MaxSpeed)
			_currentSpeed += SpeedIncreaseRate * (float)delta; // Story #11

		velocity.X = _currentSpeed;

		// Jump (Story #9)
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Slide (Story #9)
		if (Input.IsActionPressed("ui_down") && IsOnFloor())
			_isSliding = true;
		else
			_isSliding = false;

		Velocity = velocity;
		MoveAndSlide();
	}
}
