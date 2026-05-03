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
		// static flag muss bei jedem level-load zurück, sonst blockt das hud
		// nach gameover->restart die ganze pause-funktion
		Reset();
		BodyEntered += OnBodyEntered;
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (_reached) return;
		if (body is not Player player) return;

		_reached = true;
		LevelCompleted = true;
		// Freeze the player immediately so they don't run past the goal during the celebration
		player.ProcessMode = Node.ProcessModeEnum.Disabled;

		// Time bonus – sub-minute clears top out at +600, two-minute+ gives nothing.
		// Gives the player a real reason to speedrun instead of dawdling for coins.
		var hud = GetTree().GetFirstNodeInGroup("hud") as HUD;
		if (hud != null)
		{
			SaveBestTime(hud.ElapsedTime);
			int bonus = Mathf.Max(0, (int)((120f - hud.ElapsedTime) * 5f));
			if (bonus > 0)
			{
				player.AddScore(bonus);
				SpawnBonusPopup(player.GlobalPosition, bonus);
			}
		}

		player.SaveHighscorePublic();

		// Gold blink – works for both the placeholder polygon and schayans flag sprite
		var poly = GetNodeOrNull<Polygon2D>("Polygon2D");
		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (poly != null)
		{
			var tween = CreateTween();
			tween.TweenProperty(poly, "color", new Color(1, 1, 1), 0.1f);
			tween.TweenProperty(poly, "color", new Color(1, 0.84f, 0), 0.1f);
			tween.SetLoops(5);
		}
		else if (sprite != null)
		{
			var tween = CreateTween();
			tween.TweenProperty(sprite, "modulate", new Color(1.4f, 1.4f, 1.4f), 0.1f);
			tween.TweenProperty(sprite, "modulate", Colors.White, 0.1f);
			tween.SetLoops(5);
		}


		// kleine Wartezeit für Feedback
		await ToSignal(GetTree().CreateTimer(1.2f), SceneTreeTimer.SignalName.Timeout);

		// Screen anzeigen
		ShowScreen();

		// Mini Delay für UI Stabilität
		await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);

		// Spiel pausieren
		GetTree().Paused = true;
		
		
	}
	
	private void ShowScreen()
	{
		var scene = GD.Load<PackedScene>("res://Scenes/LevelCompleteScreen.tscn");

		if (scene == null)
		{
			GD.PrintErr("LevelCompleteScreen konnte nicht geladen werden.");
			return;
		}

		var screen = scene.Instantiate<LevelCompleteScreen>();

		if (screen == null)
		{
			GD.PrintErr("LevelCompleteScreen Instanz fehlgeschlagen.");
			return;
		}

		screen.ProcessMode = Node.ProcessModeEnum.Always;

		GetTree().CurrentScene.AddChild(screen);

		screen.CallDeferred(nameof(LevelCompleteScreen.ShowScreen));
	}
	
	

	// Floating gold label above the player showing the bonus – runs faster than the
	// 1s pre-pause window so the tween has time to finish before GetTree().Paused kicks in
	private void SpawnBonusPopup(Vector2 worldPos, int amount)
	{
		var wrap = new Node2D { GlobalPosition = worldPos + new Vector2(0, -60) };
		var label = new Label
		{
			Text = $"TIME BONUS +{amount}",
			Modulate = new Color(1f, 0.85f, 0.25f),
			Position = new Vector2(-80, -15),
			Scale = Vector2.One * 1.8f
		};
		wrap.AddChild(label);
		GetTree().CurrentScene.AddChild(wrap);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(wrap, "global_position", wrap.GlobalPosition + new Vector2(0, -80), 0.8f);
		tween.Parallel().TweenProperty(wrap, "modulate:a", 0f, 0.8f);
		tween.TweenCallback(Callable.From(() => wrap.QueueFree()));
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
