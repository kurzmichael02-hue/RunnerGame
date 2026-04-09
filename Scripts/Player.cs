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
	private float _invincibilityTimer = 0f; // brief invincibility after getting hit

	public int Lives => _lives;
	public int Score => _score;
	private int _score = 0;

	public override void _Ready()
	{
		_gravity = new Vector2(0, 1800f);
		GD.Print("Lives: " + _lives);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity += _gravity * (float)delta;

		// Jump – Space, W or arrow up
		bool jumpPressed = Input.IsKeyPressed(Key.Space)
			|| Input.IsKeyPressed(Key.W)
			|| Input.IsKeyPressed(Key.Up);
		if (jumpPressed && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Left/Right – A/D or arrow keys
		float direction = 0;
		if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))
			direction = -1;
		else if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D))
			direction = 1;

		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
		
		// prevent player from going off the left edge
if (Position.X < 0)
	Position = new Vector2(0, Position.Y);

		// Invincibility timer after hit
		if (_invincibilityTimer > 0)
		{
			_invincibilityTimer -= (float)delta;
			return; // skip collision check while invincible
		}

		// Enemy collision – only one hit per contact
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is Enemy)
			{
				if (_shieldActive)
				{
					_invincibilityTimer = 1.0f;
					return;
				}
				Die();
				break; // stop after first hit, prevents multi-hit in same frame
			}
		}

		// Shield timer
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
		GD.Print("Lives: " + _lives);

		if (_lives <= 0)
		{
			GD.Print("GAME OVER");
			Visible = false;
			_lives = 0; // force to 0 so HUD shows correct value before pause
			GetTree().Paused = true;
		}
		else
		{
			_invincibilityTimer = 1.5f; // 1.5s invincible after respawn
			Position = new Vector2(200, 260);
			_isDying = false;
		}
	}
	
	public void AddScore(int amount)
{
	_score += amount;
	GD.Print("Score: " + _score);
}
}
