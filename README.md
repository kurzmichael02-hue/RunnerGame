# 2D Runner Game 🎮
A 2D jump 'n' run game developed as a Software Engineering project at DHBW.
Built with Godot and C#, featuring level selection, power-ups, a highscore system and smooth controls.

## Team
| Member | Role |
|---|---|
| Michael Kurz | Game Logic |
| Schayan | Level & Environment |
| Bartolmay | UI & Menu Design |
| Maksym Mykhailych | Project Lead & Documentation |
| Tim | Sound & FX |

## Tech Stack
- **Engine:** Godot 4.6 + C#
- **Version Control:** Git & GitHub
- **Branch Workflow:** `main` / `dev` / `feature-*`
- **Sound:** FL Studio 2025, Audacity, Serum

## Project Structure
```
res://
├── Audio/    → Music & Sound Effects
├── Scenes/   → Levels, Main Menu, Game Logic
├── Scripts/  → Gameplay Code (Movement, Collision, Power-ups)
├── Sprites/  → Characters, Backgrounds, Assets
└── Prefabs/  → Reusable Game Objects
```

## System Design
```
┌─────────────────────────────────────────────────┐
│                   GAME SCENE                    │
│                                                 │
│  ┌──────────┐     ┌──────────┐  ┌───────────┐  │
│  │  Player  │────▶│ Enemy    │  │   Coin    │  │
│  │          │     │ (patrol) │  │ (collect) │  │
│  │ Movement │     └──────────┘  └─────┬─────┘  │
│  │ Jump     │          │              │         │
│  │ Slide    │     collision           │score    │
│  │ Lives    │◀─────────┘              │         │
│  └────┬─────┘                         │         │
│       │ die()                   ┌─────▼──────┐  │
│       │                         │GameManager │  │
│  ┌────▼─────┐                   │            │  │
│  │ Respawn  │                   │ Score      │  │
│  │ Point    │                   │ Lives      │  │
│  └──────────┘                   │ GameOver   │  │
│                                 └────────────┘  │
└─────────────────────────────────────────────────┘
```

## Status
🚧 In Development – DHBW Semester 3
