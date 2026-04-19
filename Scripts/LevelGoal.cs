using Godot;

public partial class LevelGoal : Area2D
{
	// Global flag – HUD reads this to block ESC/pause after the level is won
	public static bool LevelCompleted = false;
	private bool _reached = false;

	// Called when starting a new level run so the flag doesn't carry over
	public static void Reset()
	{
		LevelCompleted = false;
	}

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (_reached) return;
		if (body is not Player player) return;

		_reached = true;
		LevelCompleted = true;
		// Freeze the player immediately so they don't run past the goal during the celebration
		player.SetPhysicsProcess(false);
		player.SaveHighscorePublic();

		// Grab the HUD for the elapsed time – stored as the new best if faster
		var hud = GetTree().Root.FindChild("HUD", true, false) as HUD;
		if (hud != null)
			SaveBestTime(hud.ElapsedTime);

		// Gold blink on the goal flag (white ↔ gold, 5 loops)
		var tween = CreateTween();
		tween.TweenProperty(GetNode<Polygon2D>("Polygon2D"), "color",
			new Color(1, 1, 1), 0.1f);
		tween.TweenProperty(GetNode<Polygon2D>("Polygon2D"), "color",
			new Color(1, 0.84f, 0), 0.1f);
		tween.SetLoops(5);

		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		GetTree().Paused = true;
	}

	private void SaveBestTime(float time)
	{
		string path = "user://level1_time.dat";
		float best = float.MaxValue;

		if (FileAccess.FileExists(path))
		{
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			best = file.GetFloat();
		}

		if (time < best)
		{
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			file.StoreFloat(time);
		}
	}
}
