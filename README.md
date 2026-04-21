# 2D Runner Game

2D jump 'n' run built as a group project for the Software Engineering
module at DHBW Mannheim. Godot 4.6 + C#, five people, one semester.

A Mario-inspired platformer with movement tech (coyote time, jump buffer,
double jump, wall jump, P-Speed), a sword-based combat system with
deflectable projectiles, five enemy types, stackable power-ups and a full
scoring + best-time persistence layer.

## Team

| Member | Role |
|---|---|
| Michael Kurz | Game Logic |
| Schayan | Level & Environment |
| Bartolmay | UI & Menus |
| Maksym Mykhailych | Project Lead & Docs |
| Tim | Sound & FX |

## Tech Stack

- Godot 4.6.2 · C# (.NET 8)
- Git / GitHub – branch per feature, merged into `dev`
- Taiga – user stories & sprints
- FL Studio, Audacity, Serum – audio

## Features

**Movement**
Acceleration/deceleration, variable jump height, coyote time, jump buffer,
ducking, double jump, wall slide, wall jump, P-Speed boost for sustained
running.

**Combat**
Sword attack with limited uses per life (refillable via pickups), cooldown
and a short movement lockout on swing. Deflects enemy projectiles back at
the shooter. Stomp-chain bonus for killing multiple enemies mid-air.

**Power-ups** (all timed)
Shield, Magnet, Star, Fire Flower, Heart pickup, Sword pickup.

**Enemies**
Patrol, Fast, Jumping, Charger, Shooter.

**HUD**
Hearts display, progress bar, power-up timers, attack cooldown indicator
with ammo count, score + chain popups.

**Persistence**
Highscore and best-time saved locally. Game Over and Highscore screens
show the run vs. the record.

## Project Structure

```
res://
├── Scenes/       → Game scene, power-up / enemy / pickup scenes, menus, settings
├── Scripts/      → Gameplay code (Player, Enemy, Projectile, HUD, menus, ...)
├── Sounds/       → SFX (jump, coin, death, sword, checkpoint, ...)
├── Music/        → Background and state-based music
└── leveldesign/  → Sprites, tileset, background layers
```

## Branches

- `main` – stable
- `dev` – shared integration
- `feature/game-logic` – Michael
- `feature/level-design` – Schayan
- `feature/ui` – Bartolmay
- `feature/sound` – Tim

## Status

In development – DHBW Semester 3
