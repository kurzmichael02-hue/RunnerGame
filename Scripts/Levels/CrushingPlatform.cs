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

		// platform bewegen, moveandcollide returnt wenn was im weg ist
		var collision = MoveAndCollide(motion);
		if (collision != null
			&& collision.GetCollider() is Player crushed
			&& !crushed.IsDying && !crushed.IsInvincible)
		{
			crushed.Die();
			return;
		}

		// moveandcollide kriegt's nicht mit wenn der player oben mitfährt statt
		// dagegen gedrückt zu werden. deswegen manuell checken ob er im
		// platform-bereich steht - die hat oben drauf spikes, also jeder kontakt = tot
		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.IsDying || player.IsInvincible) return;

		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
		// platform ist 150 breit + spikes 10px hoch, body 24 hoch
		if (Mathf.Abs(toPlayer.X) < 80f && Mathf.Abs(toPlayer.Y) < 35f)
		{
			player.Die();
		}
	}
}
