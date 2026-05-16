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
- `Scenes/Levels/TestLevel.tscn` — Dev-Testlevel, noch im LevelSelection-Menü sichtbar
- `Scenes/Main/` — MainMenu, Settings, LevelSelection, GameOver, Highscores, PauseMenu, CharacterSelection
- `Scenes/LevelCompleteScreen.tscn` — Level-Abschluss-Screen
- `Scenes/level_objects/` — checkpoint, crushing_platform, moving_platform, projectile, spike, item_block, level_goal

## Offen / bekannte Baustellen

- **Tim (Char 2, orange)** — in `Player.Profile.cs` definiert (Preis 250 Coins) aber kein Button in `CharacterSelection.tscn`. Bartolmay muss den dritten Button im Editor anlegen.
- **TestLevel im Menü sichtbar** — `LevelSelection.cs` hat `_on_testLevel_pressed()`, Button ist in der Scene aktiv
- **Brightness + Coins im HUD** — funktional implementiert, Bartolmay macht Styling

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
