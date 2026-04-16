using Godot;

public partial class LevelGoal : Area2D
{
	private bool _reached = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (_reached) return;
		if (body is not Player player) return;

		_reached = true;

		// Flash the goal node gold
		var tween = CreateTween();
		tween.TweenProperty(GetNode<Polygon2D>("Polygon2D"), "color",
			new Color(1, 1, 1), 0.1f);
		tween.TweenProperty(GetNode<Polygon2D>("Polygon2D"), "color",
			new Color(1, 0.84f, 0), 0.1f);
		tween.SetLoops(5);

		// Wait 1 second then pause
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

		GetTree().Paused = true;
	}
}
