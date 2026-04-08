using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -600.0f;

	private Vector2 _gravity;
	private int _lives = 3;
	private bool _isDying = false;
	private bool _shieldActive = false;
	private float _shieldTimer = 0f;

	public override void _Ready()
	{
		_gravity = new Vector2(0, 1800f);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity += _gravity * (float)delta;

		// Jump – Space, W oder Pfeil hoch (Story #9)
		bool jumpPressed = Input.IsKeyPressed(Key.Space) 
			|| Input.IsKeyPressed(Key.W) 
			|| Input.IsKeyPressed(Key.Up);
		if (jumpPressed && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Links/Rechts – A/D oder Pfeiltasten
		float direction = 0;
		if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))
			direction = -1;
		else if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D))
			direction = 1;

		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		// Slide (Story #9)
		if (Input.IsKeyPressed(Key.Shift) && IsOnFloor())
			GD.Print("Sliding!");

		Velocity = velocity;
		MoveAndSlide();

		// Enemy collision
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is Enemy)
			{
				if (_shieldActive) return;
				Die();
			}
		}

		// Shield timer (Story #20)
		if (_shieldActive)
		{
			_shieldTimer -= (float)delta;
			if (_shieldTimer <= 0)
			{
				_shieldActive = false;
				GD.Print("Shield expired!");
			}
		}
	}

	public void ActivateShield()
	{
		_shieldActive = true;
		_shieldTimer = 10f;
		GD.Print("Shield activated!");
	}

	public void Die()
	{
		if (_isDying) return;
		_isDying = true;

		_lives--;
		_lives = Mathf.Max(_lives, 0);
		GD.Print("Lives left: " + _lives);

		if (_lives <= 0)
		{
			GD.Print("GAME OVER");
			Visible = false; // Player verstecken
			GetTree().Paused = true;
		}
		else
		{
			Position = new Vector2(200, 290);
			_isDying = false;
		}
	}
}
