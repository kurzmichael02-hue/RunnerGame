using Godot;

public partial class LevelGoal : Area2D
{
	public static bool LevelCompleted = false;

	private bool _reached = false;
	
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
		player.SetPhysicsProcess(false);

		var hud = GetTree().Root.FindChild("HUD", true, false) as HUD;
		if (hud != null)
			SaveBestTime(hud.ElapsedTime);

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
