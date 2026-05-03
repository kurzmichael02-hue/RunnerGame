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

		// platform einfach bewegen, kollision regelt godot
		MoveAndCollide(motion);

		// nur sterben wenn der player VON OBEN auf die spikes kommt.
		// von unten dagegen springen oder daneben stehen darf nicht killen.
		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player == null || player.IsDying || player.IsInvincible) return;

		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
		// horizontal überlappt + player ist ÜBER der platform (negative dy = oben).
		// dy > -55: nicht zu hoch in der luft (sonst stirbt man beim drüberspringen)
		// dy < -8: player muss zumindest leicht über der platform-mitte sein
		bool fromAbove = toPlayer.Y < -8f && toPlayer.Y > -55f;
		bool horizOverlap = Mathf.Abs(toPlayer.X) < 78f;

		if (horizOverlap && fromAbove)
		{
			player.Die();
		}
	}
}
