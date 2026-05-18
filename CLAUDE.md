# RunnerGame

DHBW Mannheim · Software Engineering Gruppenproject · Godot 4.6.2 + C# (.NET 8)

## Team & Rollen

| Member | Branch | Zuständig |
|---|---|---|
| Michael Kurz | `feature/game-logic` | Game Logic (mein Branch) |
| Schayan | `feature/level-design` | Level & Environment |
| Bartolmay | `feature/ui` | UI & Menus |
| Maksym | `dev` | Project Lead, Docs, Merges |
| Tim | `feature/sound` | Sound & FX |

## Vor jedem Start — PFLICHT

1. `git fetch origin` — remote-Stand holen
2. `git log --oneline origin/dev ^HEAD` — was ist auf dev das ich nicht hab?
3. Wenn da was steht: erst mergen, dann arbeiten. Niemals blind drauflosarbeiten.
4. Datum + Uhrzeit beachten — commits anderer Leute die "gestern" oder "heute" sind können alles verändern.

## Branch-Strategie

- `main` — stable
- `dev` — shared integration, Maksym merged rein
- Nichts in `dev` pushen ohne: Tim hat getestet + Bartolmay hat Styling gemacht

## Stack

- Godot 4.6.2 Mono (portable .exe unter `D:\Studium\Godot_v4.6.2-stable_mono_win64`)
- C# (.NET 8), Solution: `Mario.sln`
- Build: `dotnet build Mario.csproj`
- Kein Autoload — SoundManager ist ein Node im Level (Singleton via `Instance`)

## Architektur

- `Player.cs` + `Player.Combat.cs` + `Player.Profile.cs` — partial class, Movement/Combat/Persistenz getrennt
- `GameManager.cs` — Pause, Musik, Levelstart. Gruppe: `"game_manager"` (wichtig für PauseMenu-Lookup)
- `SoundManager.cs` — Singleton via `SoundManager.Instance`, in jedem Level als Node instanziiert
- `Enemy.cs` — alle 5 Typen (Patrol, Fast, Jumping, Charger, Shooter) in einer Klasse
- `HUD.cs` — Gruppe: `"hud"`, zeigt Score+Coins, Hearts, PowerUp-Timer, Angriff-Cooldown, Fortschrittsbar
- Persistenz: `user://highscore.dat`, `user://profile.cfg` (Coins, Charakter), `user://settings.cfg`, `user://level1_time.dat`

## Szenen

- `Scenes/Levels/Level1.tscn` — einziges echtes Level
- `Scenes/Levels/TestLevel.tscn` — Dev-Testlevel, Button in LevelSelection versteckt (`visible=false`)
- `Scenes/Main/` — MainMenu, Settings, LevelSelection, GameOver, Highscores, PauseMenu, CharacterSelection, Volume, Controls, LevelCompleteScreen, Game
- `Scenes/level_objects/` — checkpoint, crushing_platform, moving_platform, projectile, spike, item_block, level_goal

## Offen / bekannte Baustellen

- **Tim (Char 2, orange)** — in `Player.Profile.cs` definiert (Preis 250 Coins) aber kein Button in `CharacterSelection.tscn`. Bartolmay muss dritten Button im Editor anlegen.
- **Brightness-Slider** — Logik in `Volume.cs` fertig, Bartolmay hat Node gebaut. Styling ggf. noch ausstehend.
- **Coins im HUD** — läuft, Bartolmay macht Styling wenn Zeit ist.

## Branch-Status (Mai 2026)

`feature/game-logic` → in `dev` gemergt (aa69dff, 17.05.2026). Tim hat getestet ✓.

## Was bereits gefixed wurde (feature/game-logic, Mai 2026)

- Star-Musik ging nach Pause/Resume verloren
- PauseMenu Resume-Pfad war hardcoded `/root/Node2D` → jetzt Group-Lookup
- PauseMenu: `_UnhandledInput` rief `_Input` doppelt auf → ESC-Bug
- Mute-Button-State beim Öffnen falsch (Flags zu spät gesetzt)
- Projektile flogen durch Wände/Tiles
- GrowthPickup driftete über Zeit weg von Spawn-Position
- Volume-Änderungen in Settings wurden nicht gespeichert
- "NEW HIGHSCORE" wurde bei Gleichstand angezeigt
- Crushing Platform: `Die()` statt `DieFall()` → Spieler konnte Crusher durch Schrumpfen überleben
- LevelCompleteScreen: kein Button-Sound auf Main Menu / Retry
- Dead code in `UpdateAnimation`, auskommentierter `direction`-Block in `_PhysicsProcess`
