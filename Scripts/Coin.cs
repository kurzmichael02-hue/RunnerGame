using Godot;

public partial class Coin : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		// "coin" group is used by the magnet power-up to pull coins toward the player
		AddToGroup("coin");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.AddScore(1);
			SoundManager.Instance.PlayCoin();
			SpawnScorePopup(GlobalPosition);
			QueueFree();
		}
	}

	// Floating "+1" label that drifts up and fades – lives on the scene root so it survives
	// after the coin itself is QueueFree()'d. Node2D wrapper because Godot 4 has no Label2D;
	// Label inside a Node2D positions in world space.
	private void SpawnScorePopup(Vector2 worldPos)
	{
		var wrap = new Node2D { GlobalPosition = worldPos };
		var label = new Label
		{
			Text = "+1",
			Modulate = new Color(1f, 0.88f, 0.25f),
			Position = new Vector2(-12, -20),
			Scale = Vector2.One * 1.6f
		};
		wrap.AddChild(label);
		GetTree().CurrentScene.AddChild(wrap);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(wrap, "global_position", worldPos + new Vector2(0, -55), 0.6f);
		tween.Parallel().TweenProperty(wrap, "modulate:a", 0f, 0.6f);
		tween.TweenCallback(Callable.From(() => wrap.QueueFree()));
	}
}
