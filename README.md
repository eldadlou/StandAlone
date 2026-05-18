# StandAlone is RTS game still in develop

A Unity real-time strategy (RTS) prototype built around modular runtime systems, dependency injection, and performance-oriented patterns (spatial partitioning, object pooling, centralized combat detection).

> **Note:** This repository currently tracks only `Assets/Scripts/`. Scenes, prefabs, art, and other Unity assets are not included in version control. Some scripts may be incomplete or require scene setup to run.

## Features

- **Unit combat** — Tanks, vehicles, and combat units with weapon mounts, accuracy, and projectile-based fire
- **Command & selection** — RTS-style unit selection, box select, and command routing
- **Movement & pathfinding** — NavMesh-based movement with formation and separation settings
- **Teams & AI** — Player vs AI teams with allied support and team assignment
- **Runtime systems** — Fire, detection, explosions, audio, particles, and UI wired through a shared service container
- **Performance** — Spatial grid queries, object pooling, and configurable limits via `GameConfig`

## Requirements

| Tool | Version |
|------|---------|
| [Unity](https://unity.com/download) | **6000.0.47f1** (Unity 6) |
| Render pipeline | Universal Render Pipeline (URP) |

Use the same Unity editor version as listed in `ProjectSettings/ProjectVersion.txt` to avoid project upgrade prompts.

## Getting started

1. **Clone the repository**
   ```bash
   git clone https://github.com/eldadlou/StandAlone.git
   cd StandAlone
   ```

2. **Open in Unity Hub** — Add the project folder and open it with Unity **6000.0.47f1**.

3. **Scene setup** — Because only scripts are tracked in git, you need a local Unity project with scenes and prefabs. Add these components to your bootstrap scene as needed:
   - `GameManager` — coordinates players and high-level game flow
   - `SystemInitializer` — registers combat, movement, UI, audio, pooling, and detection systems with the dependency container
   - `InputHandler` / `CommandSystem` — player input and unit commands
   - `SelectionManager` — unit selection and feedback

4. **Optional configuration** — Create a `GameConfig` asset via **Assets → Create → MyGame → Game Configuration** to tune movement, combat, pooling, and performance defaults.

## Project structure (`Assets/Scripts`)

```
Assets/Scripts/
├── Core/                 # DI container, units, commands, config, pooling, spatial grid
│   ├── Commands/
│   ├── Configuration/
│   ├── Services/         # Service interfaces (IDependencyResolver, ISpatialUnitQuery, …)
│   ├── SpatialPartitioning/
│   └── Units/
├── Game/                 # GameManager, AI, teams, tests, debug utilities
├── Input/                # Input handling and command system
├── Presentation/         # Camera, selection UI, unit visuals, health display
├── RuntimeSystems/       # Combat, movement, audio, effects
│   ├── Audio/
│   ├── Combat/
│   ├── Effects/
│   └── Movement/
└── Editor/               # Custom inspectors and asset creation tools
```

## Architecture

### Dependency injection

`DependencyContainer` is a lightweight service locator used across the codebase. Systems register at startup (typically from `SystemInitializer` or their own `RegisterWithDependencyContainer` methods) and are resolved by interface or concrete type:

```csharp
DependencyContainer.Instance.Register<IMyService>(implementation);
var service = DependencyContainer.Instance.Resolve<IMyService>();
```

`SystemInitializer` bootstraps core runtime systems on `Awake` and subscribes them to units via `GameEvents.OnUnitCreated`.

### Layering

| Layer | Responsibility |
|-------|----------------|
| **Core** | Domain types (`Unit`, `CombatUnit`), interfaces, events, configuration |
| **RuntimeSystems** | Frame/update logic: firing, pathfinding, explosions, audio |
| **Presentation** | Visuals, UI, camera, selection feedback |
| **Input** | Commands and player input |
| **Game** | Session orchestration (`GameManager`, players, AI) |

### Key types

- `GameManager` — Singleton entry point; initializes players and systems
- `SystemInitializer` — Wires and registers runtime systems with the DI container
- `GameConfig` — ScriptableObject for tunable gameplay and performance settings
- `CentralizedDetectionManager` / `LightweightFireSystem` — Combat detection and firing
- `SpatialGrid` — Spatial queries for units and combat

## Development status

The project is under active development. The initial commit notes that **some scripts are not fully wired or tested** in isolation. Expect to adjust scene references, prefabs, and missing registrations when bringing up a new environment.

## Contributing

1. Fork the repository and create a feature branch.
2. Keep changes focused; match existing namespaces (`MyGame.*`) and patterns.
3. Open a pull request with a short description of behavior changes and how you tested them in the Unity editor.

## License

No license file is included yet. All rights reserved by the repository owner unless a license is added.
