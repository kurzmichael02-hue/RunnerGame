using Godot;
using System.Collections.Generic;

// Alles rund um persistente daten – coins, charakter-auswahl, highscore.
// Steht als eigenes file, weil player.cs sonst über 1000 zeilen wird und das
// shop/profile-system saubere trennung verdient. partial-class teilt fields
// mit der haupt-player.cs, calls bleiben player.coins / player.loadprofile usw.
public partial class Player : CharacterBody2D
{
	// ===== HIGHSCORE / SESSION =====

	// Score des letzten runs. Static damit der wert den scenewechsel zu gameover
	// überlebt und dort als final score angezeigt werden kann.
	public static int LastRunScore = 0;

	// zuletzt gespieltes level – gameover restart lädt das gleiche level nach
	public static string CurrentLevelPath = "res://Scenes/Levels/Level1.tscn";

	// Höchster score in der laufenden process-session – resettet sobald das spiel
	// geschlossen wird. Der alltime-rekord landet in user://highscore.dat.
	public static int SessionHighscore = 0;

	// ===== SHOP / CHARACTER =====

	// Persistente shop-currency. Jeder coin pickup erhöht das, gespeichert in
	// user://profile.cfg. Bartolmays mainmenu kann den wert direkt anzeigen.
	public static int Coins = 0;

	// Aktive charakter-id. Persistiert über sessions hinweg.
	public static int SelectedCharacter = 0;

	// Freigeschaltete charaktere. Default-figur (0) ist immer drin damit die
	// auswahl nie leer wird, auch wenn die save kaputt ist.
	public static HashSet<int> UnlockedCharacters = new() { 0 };

	private const string ProfilePath = "user://profile.cfg";

	// Farb-tints pro charakter – kein eigenes sprite-sheet nötig, tint unterscheidet die figuren visuell.
	// SelfModulate auf jedem AnimatedSprite2D-knoten damit hit-blink / star-effekte auf dem parent-node
	// nicht interferieren.
private static readonly Color[] CharacterTints = {
	new Color(1f, 1f, 1f),
	new Color(1f, 1f, 1f),
	new Color(1f, 1f, 1f),
};

	// ===== PROFILE LOAD / SAVE =====

	// Liest coins, gewählten char und freigeschaltete liste aus profile.cfg.
	// Fällt still auf defaults zurück wenn die datei fehlt oder corrupt ist.
	public static void LoadProfile()
	{
		var config = new ConfigFile();
		if (config.Load(ProfilePath) != Error.Ok) return;

		Coins = (int)config.GetValue("currency", "coins", 0);
		SelectedCharacter = (int)config.GetValue("character", "selected", 0);

		string unlockStr = (string)config.GetValue("character", "unlocked", "0");
		UnlockedCharacters.Clear();
		foreach (var part in unlockStr.Split(','))
		{
			if (int.TryParse(part, out int id)) UnlockedCharacters.Add(id);
		}
		// Default-figur muss verfügbar bleiben, auch wenn die save kaputt war
		UnlockedCharacters.Add(0);
	}

	public static void SaveProfile()
	{
		var config = new ConfigFile();
		// Existierende sections behalten, falls noch andere kram drin steht
		config.Load(ProfilePath);
		config.SetValue("currency", "coins", Coins);
		config.SetValue("character", "selected", SelectedCharacter);
		config.SetValue("character", "unlocked", string.Join(",", UnlockedCharacters));
		config.Save(ProfilePath);
	}

	// ===== SHOP API =====

	// Preise pro charakter. Bartolmays ui ruft das beim rendern der shop-buttons.
	// -1 = unbekannte id, kein gratis-fallback.
	public static int GetCharacterPrice(int id)
	{
		return id switch
		{
			0 => 0,
			1 => 100,
			2 => 250,
			_ => -1,
		};
	}

	// Returns true wenn der kauf durchging. Ui sollte danach den coin-counter refreshen.
	public static bool BuyCharacter(int id)
	{
		if (UnlockedCharacters.Contains(id)) return false;
		int price = GetCharacterPrice(id);
		if (price < 0 || Coins < price) return false;

		Coins -= price;
		UnlockedCharacters.Add(id);
		SaveProfile();
		return true;
	}

	// Wechselt aktiven char nur wenn er freigeschaltet ist. Beim nächsten
	// Game.tscn-load wird das im player._ready / applycharactertexture übernommen.
	public static bool SelectCharacter(int id)
	{
		if (!UnlockedCharacters.Contains(id)) return false;
		SelectedCharacter = id;
		SaveProfile();
		return true;
	}

	// ===== HIGHSCORE PERSISTENCE =====

	public static int LoadHighscore()
	{
		string path = "user://highscore.dat";
		if (!FileAccess.FileExists(path)) return 0;
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		return (int)(uint)file.Get32();
	}

	public void SaveHighscorePublic() => SaveHighscore(_score);

	private void SaveHighscore(int score)
	{
		string path = "user://highscore.dat";
		int best = 0;

		if (FileAccess.FileExists(path))
		{
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			best = (int)(uint)file.Get32();
		}

		if (score > best)
		{
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			file.Store32((uint)score);
		}
	}

	// ===== INSTANCE HELPERS =====

	// Coin pickup entry – run-score + shop-coins gehen beide hoch. Profile wird
	// direkt gespeichert damit ein crash mid-level keine münzen schluckt.
	public void AddCoin(int amount)
	{
		AddScore(amount);
		Coins += amount;
		SaveProfile();
	}

	// Direkter leben-pickup. Cap bei 9 weil das hud-label sonst überläuft.
	public void AddLife()
	{
		_lives = Mathf.Min(_lives + 1, 9);
	}

	// Score wächst, jede 100er-stufe gibt ein extra leben (#41). Vergleich gegen
	// die zähl-variable statt modulo, damit ein +6 stomp-bonus, der von 96 auf 102
	// springt, das leben trotzdem auslöst.
	public void AddScore(int amount)
	{
		_score += amount;
		int earned = _score / 100;
		while (_livesFromScoreGranted < earned)
		{
			_livesFromScoreGranted++;
			_lives = Mathf.Min(_lives + 1, 9);
		}
		// Best-score in dieser session tracken
		if (_score > SessionHighscore) SessionHighscore = _score;
	}

	private void ApplyCharacterTexture()
	{
		int idx = Mathf.Clamp(SelectedCharacter, 0, CharacterTints.Length - 1);
		Color tint = CharacterTints[idx];
		string[] nodes = { "AnimatedSprite2D", "AttackSprite", "DuckSprite" };
		foreach (string name in nodes)
		{
			var node = GetNodeOrNull<CanvasItem>(name);
			if (node != null) node.SelfModulate = tint;
		}
	}
}
