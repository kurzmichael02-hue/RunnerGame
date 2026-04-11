# 2D Runner Game 

2D jump 'n' run built as a group project for the Software Engineering module at DHBW Mannheim.
Godot 4.5 + C#, five people, one semester.

## Team

| Member | Role |
|---|---|
| Michael Kurz | Game Logic |
| Schayan | Level & Environment |
| Bartolmay | UI & Menus |
| Maksym Mykhailych | Project Lead & Docs |
| Tim | Sound & FX |

## Tech Stack

- Godot 4.6.2 · C#
- Git / GitHub – branch per feature
- Taiga – user stories
- FL Studio, Audacity, Serum – audio


## Project Structure
```
res://
├── Sounds/   → Sound Effects
├── Music/    → Music
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
In Development – DHBW Semester 3
