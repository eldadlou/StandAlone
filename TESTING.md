# Testing Guide

Automated QA uses the [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) (NUnit). Manual scene debugging uses `*Debug` MonoBehaviours under `Assets/Scripts/Game/Debug/`.

## Assemblies

| Assembly | Folder | Purpose |
|----------|--------|---------|
| `MyGame.Core` | `Core/` | DI, events, spatial grid, commands, shared types |
| `MyGame.RuntimeSystems` | `RuntimeSystems/` | Combat, movement, audio, effects |
| `MyGame.Presentation` | `Presentation/`, `Presentation/Input/` | UI, selection, camera, input handlers |
| `MyGame.Game` | `Game/` | Game flow, units, AI, system bootstrap |
| `MyGame.Editor` | `Editor/` | Editor tooling |
| `MyGame.Core.Tests` | `Tests/EditMode/Core/` | Fast EditMode unit tests |
| `MyGame.Integration.Tests` | `Tests/PlayMode/Integration/` | PlayMode integration tests |

Input runtime code lives under `Presentation/Input/` and compiles into `MyGame.Presentation`.

## Run tests locally

1. Open the project in Unity **6000.0.47f1**.
2. Open **Window > General > Test Runner**.
3. **EditMode** — run `MyGame.Core.Tests` (fast, no play mode).
4. **PlayMode** — run `MyGame.Integration.Tests` (slower; bootstraps systems in code).

Filter by category: `Core`, `Integration`, `Performance`.

## Test categories

- **Core** — `DependencyContainer`, `CommandManager`, `GameEvents`, `SpatialGrid`
- **Integration** — `SystemInitializer`, cross-system smoke tests
- **Performance** — spatial grid query benchmark (`SpatialGridPerformanceTests`)

## Flaky test policy

- PlayMode tests that involve NavMesh or physics must assert outcomes (health, events), not exact transforms.
- If a PlayMode test fails intermittently, mark it `[Explicit]` and file an issue before disabling.
- Always call `GameEvents.ClearAllEvents()` and `SystemInitializer.ClearAllSystems()` in teardown.

## Adding a new test

1. Add EditMode tests under `Tests/EditMode/` when no scene is required.
2. Add PlayMode tests under `Tests/PlayMode/Integration/` when `MonoBehaviour` lifecycle is required.
3. Use naming: `MethodName_State_ExpectedResult`.
4. Prefer `[Category("...")]` for Test Runner filters and CI.

## CI

GitHub Actions runs:

- **editmode-tests** — every push/PR
- **playmode-tests** — every push/PR (loads `Assets/Scripts/Tests/TestScenes/` when present)

Results are uploaded as `EditModeResults.xml` and `PlayModeResults.xml`.

### Secrets (repository settings)

- `UNITY_LICENSE` — Unity activation file contents
- `UNITY_EMAIL` / `UNITY_PASSWORD` — Unity ID (Personal/Plus) for activation

## Code coverage (optional)

Install **Code Coverage** via Package Manager (`com.unity.testtools.codecoverage`). Enable in **Window > Analysis > Code Coverage** when investigating gaps in `MyGame.Core` and `MyGame.RuntimeSystems`.
