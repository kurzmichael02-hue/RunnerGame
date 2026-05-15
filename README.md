# 2D Runner Game

A Mario-inspired 2D platformer built for the Software Engineering module at DHBW Mannheim. Godot 4.6 + C#, five-person team, one semester.

[![engine](https://img.shields.io/badge/engine-Godot%204.6.2-blue)]()
[![language](https://img.shields.io/badge/language-C%23%20%C2%B7%20.NET%208-purple)]()
[![team](https://img.shields.io/badge/team-5%20devs-orange)]()
[![status](https://img.shields.io/badge/status-active-brightgreen)]()

---

## Team

| Member | Role |
|---|---|
| Michael Kurz | Game Logic |
| Schayan | Level & Environment |
| Bartolmay | UI & Menus |
| Maksym Mykhailych | Project Lead & Docs |
| Tim | Sound & FX |

---

## What it is

A platformer with proper movement tech: coyote time, jump buffer, variable jump height, double jump, wall slide, wall jump, P-Speed boost. Sword-based combat with deflectable projectiles. Five enemy types, stackable power-ups, persistent highscore and best-time tracking.

---

## Features

**Movement**
Acceleration / deceleration, variable jump height, coyote time, jump buffer, ducking, double jump, wall slide, wall jump, P-Speed boost for sustained running.

**Combat**
Sword attack with limited uses per life (refillable via pickups), cooldown and short movement lockout on swing. Deflects enemy projectiles back at the shooter. Stomp-chain bonus for killing multiple enemies mid-air.

**Power-ups** (all timed)
Shield, Magnet, Star, Fire Flower, Heart pickup, Sword pickup.

**Enemies**
Patrol, Fast, Jumping, Charger, Shooter.

**HUD**
Hearts, progress bar, power-up timers, attack cooldown with ammo count, score and chain popups.

**Persistence**
Highscore and best-time saved locally. Game Over and Highscore screens show the current run vs the record.

---

## Controls

| Action | Key |
|---|---|
| Move | `A` / `D` or arrow keys |
| Jump | `Space` / `W` / `↑` |
| Duck | `S` / `↓` |
| Attack | `J` / `Left-click` |
| Pause | `Esc` |

(Defaults — rebindable in Settings.)

---

## Tech stack

- **Engine:** Godot 4.6.2
- **Language:** C# (.NET 8)
- **Workflow:** Git / GitHub, branch per feature, merged into `dev`
- **Planning:** Taiga (user stories + sprints)
- **Audio:** FL Studio, Audacity, Serum
- **Communication:** Discord, WhatsApp

---

## Project structure

```
res://
├── Scenes/       → Game scene, power-up / enemy / pickup scenes, menus, settings
├── Scripts/      → Gameplay code (Player, Enemy, Projectile, HUD, menus, ...)
├── Sounds/       → SFX (jump, coin, death, sword, checkpoint, ...)
├── Music/        → Background and state-based music
└── leveldesign/  → Sprites, tilesets, background layers
```

---

## Run it locally

1. Install [Godot 4.6.2 (.NET)](https://godotengine.org/download/) and [.NET 8 SDK](https://dotnet.microsoft.com/download)
2. Clone the repo:
   ```bash
   git clone https://github.com/kurzmichael02-hue/RunnerGame.git
   ```
3. Open the project folder in Godot
4. Hit ▶ (or `F5`) to play

To build the C# assemblies first time: Godot → Project → Tools → C# → Create C# solution, then `Build`.

---

## Branches

| Branch | Purpose |
|---|---|
| `main` | stable releases |
| `dev` | shared integration |
| `feature/game-logic` | Michael |
| `feature/level-design` | Schayan |
| `feature/ui` | Bartolmay |
| `feature/sound` | Tim |

---

## Status

In development — DHBW Semester 3-4.
