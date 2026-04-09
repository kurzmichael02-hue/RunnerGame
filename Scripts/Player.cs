using Godot;

public partial class Player : CharacterBody2D
{
	// Movement
	[Export] public float MaxSpeed = 300f;
	[Export] public float Acceleration = 2000f;
	[Export] public float Deceleration = 1500f;
	[Export] public float AirAcceleration = 1200f;
	[Export] public float TurnAcceleration = 3000f;

	// Jump
	[Export] public float JumpVelocity = -500f;
	[Export] public float JumpGravity = 900f;
	[Export] public float FallGravity = 1800f;
	[Export] public float MaxFallSpeed = 800f;
	[Export] public float CoyoteTime = 0.1f;
	[Export] public float JumpBufferTime = 0.12f;

	// State
	private int _lives = 3;
	private int _score = 0;
	private bool _isDying = false;
	private bool _shieldActive = false;
	private float _shieldTimer = 0f;
	private float _invincibilityTimer = 0f;
	private float _coyoteTimer = 0f;
	private float _jumpBufferTimer = 0f;
	private bool _isJumping = false;

	public int Lives => _lives;
	public int Score => _score;

	public override void _Ready()
	{
		GD.Print("Lives: " + _lives);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		// Asymmetric gravity – hold jump = lower gravity while rising
		float gravity = (velocity.Y < 0 && Input.IsActionPressed("jump"))
			? JumpGravity
			: FallGravity;
		if (!IsOnFloor())
			velocity.Y += gravity * dt;
		velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		// Coyote time
		if (IsOnFloor())
		{
			_coyoteTimer = CoyoteTime;
			_isJumping = false;
		}
		else
			_coyoteTimer -= dt;

		// Jump buffer
		if (Input.IsActionJustPressed("jump"))
			_jumpBufferTimer = JumpBufferTime;
		else
			_jumpBufferTimer -= dt;

		// Short hop on release
		if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
			velocity.Y *= 0.5f;

		// Execute jump
		bool canJump = IsOnFloor() || (_coyoteTimer > 0f && !_isJumping);
		if (_jumpBufferTimer > 0f && canJump)
		{
			velocity.Y = JumpVelocity;
			_jumpBufferTimer = 0f;
			_coyoteTimer = 0f;
			_isJumping = true;
		}

		// Horizontal movement with acceleration
		float direction = 0;
		if (Input.IsActionPressed("move_left")) direction = -1;
		else if (Input.IsActionPressed("move_right")) direction = 1;

		bool isTurning = (direction > 0 && velocity.X < -10)
					  || (direction < 0 && velocity.X > 10);

		if (direction != 0)
		{
			float accel = !IsOnFloor() ? AirAcceleration
						: isTurning    ? TurnAcceleration
						:                Acceleration;
			velocity.X = Mathf.MoveToward(velocity.X, direction * MaxSpeed, accel * dt);
		}
		else
		{
			float decel = IsOnFloor() ? Deceleration : AirAcceleration;
			velocity.X = Mathf.MoveToward(velocity.X, 0f, decel * dt);
		}

		// Left boundary
		if (Position.X < 0)
			Position = new Vector2(0, Position.Y);

		Velocity = velocity;
		MoveAndSlide();

		// Skip collision when invincible
		if (_invincibilityTimer > 0)
		{
			_invincibilityTimer -= dt;
			return;
		}

		// Enemy collision
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var col = GetSlideCollision(i);
			if (col.GetCollider() is Enemy)
			{
				if (_shieldActive) { _invincibilityTimer = 1f; return; }
				Die();
				break;
			}
		}

		// Shield timer
		if (_shieldActive)
		{
			_shieldTimer -= dt;
			if (_shieldTimer <= 0)
			{
				_shieldActive = false;
				GD.Print("Shield expired!");
			}
		}
	}

	public void AddScore(int amount)
	{
		_score += amount;
		GD.Print("Score: " + _score);
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
		GD.Print("Lives: " + _lives);

		if (_lives <= 0)
		{
			GD.Print("GAME OVER");
			Visible = false;
			GetTree().Paused = true;
		}
		else
		{
			_invincibilityTimer = 1.5f;
			Position = new Vector2(200, 260);
			_isDying = false;
		}
	}
}
