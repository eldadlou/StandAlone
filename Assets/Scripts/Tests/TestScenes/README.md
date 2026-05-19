# Test scenes

Minimal PlayMode scenes for manual QA and optional CI scene-load tests.

| Scene | Purpose |
|-------|---------|
| `Test_Bootstrap.unity` | Empty bootstrap with `SystemInitializer` hook point |
| `Test_Combat_TwoUnits.unity` | Two opposing units for combat smoke tests |

Open in the Editor, assign unit prefabs, and add to **File > Build Settings** if you load them via `SceneManager` in tests.

Integration tests in `MyGame.Integration.Tests` currently bootstrap systems in code and do not require these scenes.
