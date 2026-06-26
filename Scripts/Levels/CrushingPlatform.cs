using Godot;

// Variant of MovingPlatform that kills the player on contact from above (spikes on top).
// Unlike the regular moving platform, this one uses MoveAndCollide so Godot handles
// the physics coupling – otherwise direct Position = ... skips sweep detection.
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
		// AnimatableBody2D hat sync_to_physics=true als default – das verträgt sich nicht
		// mit MoveAndCollide und erzeugt Error-Spam jede Physics-Frame. Wir steuern die
		// Bewegung selbst, also aus.
		SyncToPhysics = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		_time += (float)delta;
		float phase = (Mathf.Sin(_time * Mathf.Pi * 2f / CycleTime) + 1f) * 0.5f;
		Vector2 target = _startPosition.Lerp(_startPosition + EndOffset, phase);
		Vector2 motion = target - Position;
		MoveAndCollide(motion);

		// Spike-kill: player muss von OBEN auf die Spikes kommen.
		// Platform collision shape ist 25 hoch (half=12.5), Spikes ragen bis -22.
		// Player-collision-center ist ~49px über den Player-Füßen, also steht der
		// Player mit center bei ca. platform.Y - 71 wenn er auf den Spikes steht.
		// Daher threshold -90 statt vorher -55 (war zu eng und hat nie gefeuert).
		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.IsDying || player.IsInvincible) return;

		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;

		// toPlayer.Y < 0  → player ist über der platform (kleines Y = höher im Bild)
		// -90 bis -5: trifft wenn player auf/über den spikes steht, nicht wenn er darunter ist
		bool fromAbove = toPlayer.Y < -5f && toPlayer.Y > -90f;
		// plattform ist 150 breit (half=75), kleiner puffer für randberührungen
		bool horizOverlap = Mathf.Abs(toPlayer.X) < 80f;

		if (horizOverlap && fromAbove)
		{
			// Crusher tötet sofort, egal ob groß oder klein
			player.DieFall();
		}
	}
}
