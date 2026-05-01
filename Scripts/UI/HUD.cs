using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private Label _timerLabel;
	private Label _positionLabel;
	private Label _powerUpLabel;
	private ColorRect _attackFill;
	private Label _attackLabel;
	private TextureRect _progressRect;
	private Texture2D[] _progressTextures;
	// Lives display – hbox with texturerect hearts, filled/empty based on current lives
	private HBoxContainer _heartsBox;
	private Texture2D _heartFull;
	private Texture2D _heartEmpty;
	// Cached so we don't poke the scene tree every frame
	private Node2D _levelGoal;
	private float _playerStartX = 200f;
	private Player _player;
	private float _elapsedTime = 0f;
	public float ElapsedTime => _elapsedTime;

	public override void _Ready()
	{
		AddToGroup("hud");
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_livesLabel = GetNode<Label>("LivesLabel");
		_timerLabel = GetNode<Label>("TimerLabel");
		_positionLabel = GetNode<Label>("PositionLabel");
		_powerUpLabel = GetNode<Label>("PowerUpLabel");
		_attackFill = GetNode<ColorRect>("AttackIndicator/Fill");
		_attackLabel = GetNode<Label>("AttackIndicator/Label");
		_progressRect = GetNodeOrNull<TextureRect>("ProgressBar");
		// Group lookup instead of absolute path – survives scene-root renames and works
		// even if the level file gets restructured later
		_player = GetTree().GetFirstNodeInGroup("player") as Player;

		// Progress bar (#24): 10 frames from start to goal, plus one "Finished" sprite
		if (_progressRect != null)
		{
			_progressTextures = new Texture2D[11];
			for (int i = 0; i < 10; i++)
				_progressTextures[i] = GD.Load<Texture2D>($"res://leveldesign/fortschrittsanzeige{i}.png");
			_progressTextures[10] = GD.Load<Texture2D>("res://leveldesign/fortschrittsanzeigeFinished.png");
			_progressRect.Texture = _progressTextures[0];
		}
		if (_player != null) _playerStartX = _player.Position.X;
		_levelGoal = GetTree().Root.FindChild("LevelGoal", true, false) as Node2D;

		// Hearts HUD (schayans assets): row of texture rects, filled = life left
		_heartsBox = GetNodeOrNull<HBoxContainer>("Hearts");
		if (_heartsBox != null)
		{
			_heartFull = GD.Load<Texture2D>("res://leveldesign/herzen.png");
			_heartEmpty = GD.Load<Texture2D>("res://leveldesign/herzenleer.png");
		}
		// Always so the pause menu (child of HUD) still receives input while the tree is paused
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Process(double delta)
	{
		_scoreLabel.Text = "Score: " + _player.Score;
		_livesLabel.Text = "Lives: " + _player.Lives;
		_powerUpLabel.Text = BuildPowerUpText();
		UpdateHearts();

		// Attack cooldown ring: Fill shrinks from the top while the cooldown is active,
		// label shows remaining sword uses, goes red when empty
		float readiness = _player.AttackReadiness;
		_attackFill.OffsetTop = 70f * (1f - readiness);
		_attackLabel.Text = $"J\n×{_player.SwordUses}";
		if (_player.SwordUses <= 0)
			_attackLabel.Modulate = new Color(0.85f, 0.25f, 0.25f);
		else if (readiness >= 1f)
			_attackLabel.Modulate = new Color(1f, 1f, 1f);
		else
			_attackLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);

		// Progress bar: frame index = how close to the goal in percent, 10 = won
		if (_progressRect != null && _progressTextures != null)
		{
			int idx = 0;
			if (LevelGoal.LevelCompleted)
				idx = 10;
			else if (_levelGoal != null)
			{
				float span = _levelGoal.GlobalPosition.X - _playerStartX;
				if (span > 0f)
				{
					float ratio = Mathf.Clamp((_player.Position.X - _playerStartX) / span, 0f, 1f);
					idx = Mathf.Clamp((int)(ratio * 10f), 0, 9);
				}
			}
			_progressRect.Texture = _progressTextures[idx];
		}

		// Timer only runs when not paused
		if (!GetTree().Paused)
		{
			_elapsedTime += (float)delta;
			int minutes = (int)(_elapsedTime / 60);
			int seconds = (int)(_elapsedTime % 60);
			_timerLabel.Text = $"{minutes:00}:{seconds:00}";
			int meters = (int)(_player.Position.X / 50f);
_positionLabel.Text = meters + "m";
		}

		
	}

// Spawns one heart texturerect per max-so-far life slot, swaps between the full
// and empty heart texture. the row grows when you pick up extra lives so the
// empty-heart count never shrinks mid-game.
private int _maxHeartsShown = 3;
private void UpdateHearts()
{
	if (_heartsBox == null || _heartFull == null) return;
	int lives = _player.Lives;
	if (lives > _maxHeartsShown) _maxHeartsShown = lives;

	// Add missing heart slots lazily instead of building them all up-front
	while (_heartsBox.GetChildCount() < _maxHeartsShown)
	{
		var tr = new TextureRect
		{
			Texture = _heartFull,
			CustomMinimumSize = new Vector2(36, 36),
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
		};
		_heartsBox.AddChild(tr);
	}

	for (int i = 0; i < _heartsBox.GetChildCount(); i++)
	{
		if (_heartsBox.GetChild(i) is TextureRect tr)
			tr.Texture = i < lives ? _heartFull : _heartEmpty;
	}
}

// Builds a compact string of all active power-ups with remaining seconds.
// Empty string when nothing is active so the label just disappears visually.
private string BuildPowerUpText()
{
	var parts = new System.Collections.Generic.List<string>();
	if (_player.StarTimeLeft > 0f) parts.Add($"STAR {_player.StarTimeLeft:0.0}s");
	if (_player.FireTimeLeft > 0f) parts.Add($"FIRE {_player.FireTimeLeft:0.0}s");
	if (_player.ShieldTimeLeft > 0f) parts.Add($"SHIELD {_player.ShieldTimeLeft:0.0}s");
	if (_player.MagnetTimeLeft > 0f) parts.Add($"MAGNET {_player.MagnetTimeLeft:0.0}s");
	return string.Join("  |  ", parts);
}

}
