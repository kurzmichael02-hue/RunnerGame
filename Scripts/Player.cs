using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -600.0f;

	private Vector2 _gravity;

	public override void _Ready()
	{
		_gravity = new Vector2(0, 980f);
	}

	public override void _PhysicsProcess(double delta)
	{
		GD.Print(IsOnFloor());
		Vector2 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity += _gravity * (float)delta;

		// Jump (Story #9)
if (Input.IsKeyPressed(Key.Space) && IsOnFloor())
	velocity.Y = JumpVelocity;

		// Left/Right Movement
		float direction = Input.GetAxis("ui_left", "ui_right");
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}
}
