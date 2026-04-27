using Godot;

// Schwert + feuerblume + alles was beim attack-input passiert.
// Liegt als partial neben player.cs und player.profile.cs damit die haupt-datei
// auf movement / animation / state fokussiert bleibt.
public partial class Player : CharacterBody2D
{
	// ===== SWORD =====

	// J/X = swing. Cooldown plus kurzer movement-lockout damit man nicht im
	// vollen lauf durch gegner slashen kann (tims balance-feedback).
	private float _attackCooldown = 0f;
	private float _attackLockout = 0f;
	private const float AttackCooldownMax = 0.6f;
	public float AttackReadiness => 1f - Mathf.Clamp(_attackCooldown / AttackCooldownMax, 0f, 1f);
	private int _facing = 1;

	// 3 swings pro leben, refill via sword-pickups, max 5 (tim wollte das so)
	private int _swordUses = 3;
	private const int MaxSwordUses = 5;
	public int SwordUses => _swordUses;
	private float _noSwordFlashTimer = 0f;

	// ===== FIRE FLOWER (#45) =====

	// 10s lang spuckt jeder swing zusätzlich einen fireball aus
	private bool _fireActive = false;
	private float _fireTimer = 0f;
	public float FireTimeLeft => _fireActive ? _fireTimer : 0f;
	private PackedScene _projectileScene;

	// ===== ATTACK =====

	// Schwert-swing – tötet jeden gegner im kurzen arc vor dem player.
	// Placeholder-visual bis schayan eine richtige swing-animation liefert.
	private void Attack()
	{
		_attackCooldown = AttackCooldownMax;
		// Kurzer brake damit man nicht im power-slash durchläuft
		_attackLockout = 0.25f;
		_swordUses--;
		SpawnSwordSwoosh();
		SoundManager.Instance.PlaySwordAttack();

		// Hitbox: 120px reach in blickrichtung, 85px tall – größer als zum start
		// um den langsameren cadence + movement-lockout zu kompensieren
		foreach (Node node in GetTree().GetNodesInGroup("enemy"))
		{
			if (node is not Enemy enemy) continue;
			Vector2 toEnemy = enemy.GlobalPosition - GlobalPosition;
			bool sameSide = Mathf.Sign(toEnemy.X) == _facing || Mathf.Abs(toEnemy.X) < 15f;
			if (sameSide && Mathf.Abs(toEnemy.X) < 120f && Mathf.Abs(toEnemy.Y) < 85f)
				enemy.Kill();
		}

		// Gleiche hitbox deflectet auch reinkommende projektile (#53-ish).
		// Timing-reward: deflected shot fliegt zurück und tötet den shooter.
		foreach (Node node in GetTree().GetNodesInGroup("projectile"))
		{
			if (node is not Projectile proj) continue;
			Vector2 toProj = proj.GlobalPosition - GlobalPosition;
			bool sameSide = Mathf.Sign(toProj.X) == _facing || Mathf.Abs(toProj.X) < 15f;
			if (sameSide && Mathf.Abs(toProj.X) < 120f && Mathf.Abs(toProj.Y) < 85f)
				proj.Deflect();
		}

		// Mit fire-flower aktiv spawnt jeder swing zusätzlich einen fireball
		if (_fireActive)
			SpawnFireball();

		// Kleiner horizontal-shake für follow-through-feel
		Shake(2f, 0.05f);
	}

	// Wird vom physics-process gerufen wenn der spieler attack drückt aber gerade
	// keine swings mehr hat – player blinkt kurz rot via Modulate.
	public void PlayNoSwordFlash()
	{
		_noSwordFlashTimer = 0.25f;
	}

	// Refill-pickup. Cap bei 5 damit der spieler nicht endlos sammelt.
	public void AddSwordUse()
	{
		_swordUses = Mathf.Min(_swordUses + 1, MaxSwordUses);
	}

	// Fireball spawn aus dem player heraus mit player-shot-coloring.
	// Mask layer 2 (enemies) – kann den player selbst nicht treffen.
	private void SpawnFireball()
	{
		if (_projectileScene == null) return;
		if (_projectileScene.Instantiate() is not Projectile proj) return;
		proj.SetAsPlayerShot();
		proj.Velocity = new Vector2(_facing * 450f, 0f);
		proj.GlobalPosition = GlobalPosition + new Vector2(_facing * 28f, 0f);
		GetTree().CurrentScene.AddChild(proj);
	}

	// Weißer crescent-arc als swing-visual, fadet über 0.18s aus.
	private void SpawnSwordSwoosh()
	{
		var wrap = new Node2D { GlobalPosition = GlobalPosition + new Vector2(_facing * 35f, 0f) };
		var poly = new Polygon2D
		{
			Color = new Color(1f, 1f, 1f, 0.85f),
			Polygon = new Vector2[]
			{
				new Vector2(0, -30), new Vector2(15, -18), new Vector2(25, 0),
				new Vector2(15, 18), new Vector2(0, 30), new Vector2(5, 12),
				new Vector2(10, 0), new Vector2(5, -12)
			}
		};
		if (_facing < 0) poly.Scale = new Vector2(-1, 1);
		wrap.AddChild(poly);
		GetTree().CurrentScene.AddChild(wrap);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(poly, "modulate:a", 0f, 0.18f);
		tween.Parallel().TweenProperty(wrap, "scale", new Vector2(1.3f, 1.3f), 0.18f);
		tween.TweenCallback(Callable.From(() => wrap.QueueFree()));
	}
}
