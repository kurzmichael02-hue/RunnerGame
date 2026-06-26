using Godot;
using System.Collections.Generic;

// Alles rund um persistente daten – coins, charakter-auswahl, highscore.
// Steht als eigenes file, weil Player.cs sonst zu groß wird.
public partial class Player : CharacterBody2D
{
	// ===== HIGHSCORE / SESSION =====

	public static int LastRunScore = 0;

	public static string CurrentLevelPath = "res://Scenes/Levels/Level1.tscn";

	public static int SessionHighscore = 0;

	// ===== SHOP / CHARACTER =====

	public static int Coins = 0;

	public static int SelectedCharacter = 0;

	public static HashSet<int> UnlockedCharacters = new() { 0 };

	private const string ProfilePath = "user://profile.cfg";

	// ===== PROFILE LOAD / SAVE =====

	public static void LoadProfile()
	{
		var config = new ConfigFile();

		if (config.Load(ProfilePath) != Error.Ok)
			return;

		Coins = (int)config.GetValue("currency", "coins", 0);
		SelectedCharacter = (int)config.GetValue("character", "selected", 0);

		string unlockStr = (string)config.GetValue("character", "unlocked", "0");

		UnlockedCharacters.Clear();

		foreach (var part in unlockStr.Split(','))
		{
			if (int.TryParse(part, out int id))
				UnlockedCharacters.Add(id);
		}

		// Default character muss immer verfügbar sein
		UnlockedCharacters.Add(0);
	}

	public static void SaveProfile()
	{
		var config = new ConfigFile();

		config.Load(ProfilePath);

		config.SetValue("currency", "coins", Coins);
		config.SetValue("character", "selected", SelectedCharacter);
		config.SetValue("character", "unlocked", string.Join(",", UnlockedCharacters));

		config.Save(ProfilePath);
	}

	// ===== SHOP API =====

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

	public static bool BuyCharacter(int id)
	{
		if (UnlockedCharacters.Contains(id))
			return false;

		int price = GetCharacterPrice(id);

		if (price < 0 || Coins < price)
			return false;

		Coins -= price;
		UnlockedCharacters.Add(id);

		SaveProfile();

		return true;
	}

	public static bool SelectCharacter(int id)
	{
		if (!UnlockedCharacters.Contains(id))
			return false;

		SelectedCharacter = id;

		SaveProfile();

		return true;
	}

	// ===== HIGHSCORE =====

	public static int LoadHighscore()
	{
		string path = "user://highscore.dat";

		if (!FileAccess.FileExists(path))
			return 0;

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

		return (int)(uint)file.Get32();
	}

	public void SaveHighscorePublic()
	{
		SaveHighscore(_score);
	}

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

	public void AddCoin(int amount)
	{
		AddScore(amount);
		Coins += amount;
		SaveProfile();
	}

	public void AddLife()
	{
		_lives = Mathf.Min(_lives + 1, 9);
	}

	public void AddScore(int amount)
	{
		_score += amount;

		int earned = _score / 100;

		while (_livesFromScoreGranted < earned)
		{
			_livesFromScoreGranted++;
			_lives = Mathf.Min(_lives + 1, 9);
		}

		if (_score > SessionHighscore)
			SessionHighscore = _score;
	}
}
