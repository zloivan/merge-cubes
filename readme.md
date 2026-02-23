# Elements – Architecture & Project Decomposition

---

## 1. High-Level Architecture

### Guiding Principles
- **VContainer** for all DI (LifetimeScope per scene)
- **SOLID** – each class has one reason to change
- No magic numbers – all tuning values live in `ScriptableObject` configs
- World-space gameplay (not UI Canvas), camera is orthographic portrait
- GridSystem embedded as plain C# classes (no package dependency)
- EventBus for cross-system communication; direct calls within the same system
- No premature pooling; add only where profiler proves necessary

---

## 2. Tech Conventions

| Tool | When to use |
|---|---|
| **UniTask** | All async coordination: awaiting animation completion, normalization steps, save I/O, level load sequencing |
| **DOTween** | All tween-based animation (block move, fall, destroy scale, UI transitions). Use easing functions, not manual lerp in Update |
| **Update / coroutine** | Only when logic is genuinely simpler — e.g. BalloonSpawner sin movement (per-frame positional math, no easing needed) |

Rule: if DOTween easing makes intent clearer → use DOTween. If Update is 3 lines and DOTween would be 10 → use Update.

---

## 3. Layer Map

```
┌─────────────────────────────────────────────┐
│  Presentation (MonoBehaviours / Views)       │
│  BlockView, BoardView, BalloonView, HUD      │
├─────────────────────────────────────────────┤
│  Application (Controllers / UseCases)        │
│  GameController, BoardController,            │
│  NormalizationController, LevelController    │
├─────────────────────────────────────────────┤
│  Domain (Pure C# – no Unity deps)            │
│  BoardModel, MatchFinder, GravityResolver,   │
│  SwipeValidator                              │
├─────────────────────────────────────────────┤
│  Infrastructure                              │
│  SaveService, LevelRepository, InputService  │
├─────────────────────────────────────────────┤
│  Config / Data (ScriptableObjects)           │
│  LevelData, BlockConfig, BalloonConfig,      │
│  GameConfig                                  │
└─────────────────────────────────────────────┘
```

---

## 3. Core Classes & Responsibilities

### Domain
| Class | Responsibility |
|---|---|
| `BoardModel` | Pure grid state: `BlockType[,]` array + helpers (`IsEmpty`, `Swap`, `Remove`) |
| `SwipeValidator` | Validates a proposed move (bounds, upward-swap-only rule) |
| `GravityResolver` | Returns list of `(from, to)` drops for all floating blocks |
| `MatchFinder` | BFS flood-fill → connected same-type regions → filter by "contains line ≥ 3" → returns `HashSet<GridPosition>` per region |

### Application
| Class | Responsibility |
|---|---|
| `BoardController` | Receives validated swipe → mutates `BoardModel` → fires `SwapExecutedEvent` → triggers normalization loop |
| `NormalizationController` | Runs gravity → match → destroy cycle until stable; fires events per step |
| `LevelController` | Loads level, detects win (empty board), advances level index, triggers save |
| `GameController` | Entry point; owns restart logic; delegates to LevelController |

### Presentation
| Class | Responsibility |
|---|---|
| `BoardView` | Instantiates `BlockView` prefabs, subscribes to board events, maps `GridPosition` → world pos via GridSystem |
| `BlockView` | Plays idle / destroy animation; tweens to new world position on move/fall |
| `InputService` | Detects swipe on a block (screen → world raycast), fires `SwipeInputEvent` |
| `BalloonSpawner` | Manages ≤3 balloons; spawns off-screen L/R at random height; moves each along `x + A·sin(ωt + φ)` trajectory |
| `HUDController` | Restart button → `RestartRequestedEvent`; Next button (shown on win) |

### Infrastructure
| Class | Responsibility |
|---|---|
| `LevelRepository` | Loads `LevelData` assets; wraps array with cycling index logic |
| `SaveService` | JSON serialize/deserialize `SaveData` (level index + `BlockType[,]`) to `Application.persistentDataPath` |

---

## 4. Key Data Flows

### Swipe → Normalize
```
InputService
  → SwipeInputEvent
    → BoardController.OnSwipe()
      → SwipeValidator.Validate()
      → BoardModel.Swap() / Move()
      → SwapExecutedEvent
        → NormalizationController.RunCycle()
          loop until stable:
            GravityResolver → drops → BlocksFellEvent
            MatchFinder     → regions → BlocksMatchedEvent
          → NormalizationCompleteEvent
            → LevelController.CheckWin()
```

### Save Trigger Points
- After every `NormalizationCompleteEvent` (board is stable)
- On `Application.quitting` / `OnApplicationPause(true)`

---

## 5. Level Format (ScriptableObject + JSON)

```
LevelData (ScriptableObject)
  int   Width
  int   Height
  BlockType[] InitialBlocks   // row-major, 0 = empty, 1 = Fire, 2 = Water
```

Runtime levels loaded from `Resources/Levels/Level_001.asset` etc.  
Three levels ship with the project; adding a new level = create new SO, fill grid, done – no code.

---

## 6. Camera & Resolution Fit

**Orthographic camera** — mandatory for flat world-space grid.

`CameraFitter : MonoBehaviour` subscribes to `LevelLoadedEvent` and recalculates `Camera.orthographicSize` each time a level loads:

```
orthographicSize = (gridHeight * cellSize / 2) + verticalPadding

// Clamp for wide/tablet screens:
requiredHalfWidth = (gridWidth * cellSize / 2) + horizontalPadding
cameraHalfWidth   = orthographicSize * Screen.width / Screen.height
if (requiredHalfWidth > cameraHalfWidth)
    orthographicSize = requiredHalfWidth * Screen.height / Screen.width
```

`verticalPadding` and `horizontalPadding` are tunable constants in `GameConfig` SO.  
`CameraFitter` depends only on `LevelData` and `GameConfig` — no other system coupling.

---

## 7. Data Layers (Config vs Persistence)

| Layer | Type | Who writes | Who reads |
|---|---|---|---|
| Static config | ScriptableObjects | Designer (Editor) | `LevelRepository`, VContainer |
| Runtime save | JSON (`persistentDataPath`) | `SaveService` | `SaveService` |
| Domain state | Pure C# POCOs | Controllers | Controllers, Views via events |

**ScriptableObjects are read-only in runtime builds.** Never write to them at runtime.  
`SaveData` is a plain C# class (no Unity deps): `{ int levelIndex; int[] blocks; }`.  
`LevelRepository` converts `LevelData` SO → plain domain struct before handing to controllers — SO never leaks into domain layer.

**Centralized config:** One `GameConfig` SO holds references to `BlockConfig[]`, `BalloonConfig`, level list, camera padding, animation timings. Designers open one asset, navigate everything from there.

---

## 8. GridSystem Integration

Use `GridSystemSquare<BlockType>` (pure C#).  
`GetWorldPosition(GridPosition)` drives all view placement.  
Camera orthographic size calculated at runtime to fit grid + margins inside safe portrait area.

---

## 7. VContainer Scene Scope

```
GameLifetimeScope
  ├─ GameConfig (SO) – bind as instance
  ├─ LevelRepository
  ├─ SaveService
  ├─ BoardModel         (new() – plain C#)
  ├─ SwipeValidator
  ├─ GravityResolver
  ├─ MatchFinder
  ├─ BoardController
  ├─ NormalizationController
  ├─ LevelController
  ├─ GameController
  ├─ InputService       (MonoBehaviour – FindComponent)
  ├─ BoardView          (MonoBehaviour)
  ├─ BalloonSpawner     (MonoBehaviour)
  └─ HUDController      (MonoBehaviour)
```

---

## 8. Events (EventBus<T>)

| Event | Payload | Raised by | Consumed by |
|---|---|---|---|
| `SwipeInputEvent` | `GridPosition from, Direction dir` | InputService | BoardController |
| `SwapExecutedEvent` | `GridPosition a, b` | BoardController | BoardView |
| `BlocksFellEvent` | `List<(GridPos from, GridPos to)>` | NormalizationController | BoardView |
| `BlocksDestroyedEvent` | `HashSet<GridPosition>` | NormalizationController | BoardView |
| `NormalizationCompleteEvent` | – | NormalizationController | LevelController, SaveService |
| `LevelWonEvent` | – | LevelController | HUDController, LevelController |
| `LevelLoadedEvent` | `LevelData` | LevelController | BoardView, BoardController |
| `RestartRequestedEvent` | – | HUDController | GameController |

---

## 9. GitHub Projects – Labels

| Label | Color | Usage |
|---|---|---|
| `feature` | blue | New functionality |
| `bug` | red | Defect |
| `config` | yellow | Data / SO / serialization |
| `arch` | purple | Setup, DI, structure |
| `polish` | pink | Animations, feel, UX |
| `docs` | gray | README, video, submission |
| `blocked` | orange | Waiting on dependency |

---

## 10. Milestones & Issues

### 🏁 M1 – Project Bootstrap (Day 1 AM)
| # | Title | Labels | Notes |
|---|---|---|---|
| 1 | Unity project setup, folder structure, IL2CPP target | `arch` | Android build target, portrait lock |
| 2 | Embed GridSystem, EventBus, SoundSystem packages as src | `arch` | Delete packages after copy |
| 3 | VContainer install; create `GameLifetimeScope` skeleton | `arch` | |
| 4 | Define all ScriptableObject configs (GameConfig, BlockConfig, BalloonConfig) | `config` | No magic numbers rule enforced here |
| 5 | Implement `LevelData` SO + create 3 level assets | `config` | |
| 6 | `LevelRepository` – load + cycle levels | `feature` | |

---

### 🏁 M2 – Core Board Logic (Day 1 PM – Day 2)
| # | Title | Labels |
|---|---|---|
| 7 | `BoardModel` – grid state, Swap, Move, Remove, IsEmpty | `feature` |
| 8 | `SwipeValidator` – bounds check, upward-swap-only rule | `feature` |
| 9 | `GravityResolver` – compute all drops bottom-up | `feature` |
| 10 | `MatchFinder` – BFS flood-fill, line-of-3 filter, simultaneous region collection | `feature` |
| 11 | `BoardController` – swipe → model mutation → events | `feature` |
| 12 | `NormalizationController` – gravity+match loop until stable | `feature` |
| 13 | Unit-test normalization edge cases (fire-row example from FAQ) | `feature` | Plain NUnit, no Unity runner needed |

---

### 🏁 M3 – Presentation & Input (Day 2 PM – Day 3 AM)
| # | Title | Labels |
|---|---|---|
| 14 | `InputService` – touch/mouse swipe detection, world raycast, `SwipeInputEvent` | `feature` |
| 15 | `BoardView` – spawn `BlockView` prefabs, map grid → world pos, subscribe to events | `feature` |
| 16 | `BlockView` – tween move/fall (DOTween), idle anim, destroy anim; lock input during fall/destroy | `feature` |
| 17 | Camera auto-fit to grid in portrait at any resolution | `feature` |
| 18 | `HUDController` – Restart button (always visible), Next button (win only) | `feature` |
| 19 | `BalloonSpawner` – spawn, sin trajectory, max 3, respawn on exit | `feature` |
| 20 | `LevelController` – win detection, auto-advance after last destroy anim | `feature` |
| 21 | `GameController` – restart flow (reload level from save-cleared state) | `feature` |

---

### 🏁 M4 – Save & Persistence (Day 3 PM)
| # | Title | Labels |
|---|---|---|
| 22 | `SaveData` model – level index + serializable board state | `config` |
| 23 | `SaveService` – JSON write/read to `persistentDataPath` | `feature` |
| 24 | Save on `NormalizationCompleteEvent` + `OnApplicationPause` | `feature` |
| 25 | Load save on boot; fall back to level 1 if no save | `feature` |
| 26 | Validate save version / corruption guard | `feature` |

---

### 🏁 M5 – Integration & Smoke Test (Day 3 PM)
| # | Title | Labels |
|---|---|---|
| 27 | Full play-through: all 3 levels cycle correctly | `feature` |
| 28 | Save/restore smoke test (suspend & reopen) | `feature` |
| 29 | Input lock during normalization verified | `feature` |
| 30 | Buttons work during fall/destroy without crash | `feature` |

---

### 🏁 M6 – Polish (Day 4 – Thursday)
| # | Title | Labels |
|---|---|---|
| 31 | Block idle animations hooked up | `polish` |
| 32 | Block destroy VFX / animation | `polish` |
| 33 | Sound: swap, destroy, win, background music | `polish` |
| 34 | Balloon sprites, smooth sin movement visual check | `polish` |
| 35 | Background parallax / static bg image fits all resolutions | `polish` |
| 36 | UI visual pass (fonts, button art, layout) | `polish` |
| 37 | 60fps profiler pass on mid-range Android | `polish` |
| 38 | Fix any bugs found during polish | `bug` |

---

### 🏁 M7 – Release Prep (Day 5 – Friday)
| # | Title | Labels |
|---|---|---|
| 39 | IL2CPP Android build passes | `docs` |
| 40 | README: setup, architecture summary, how to add a level | `docs` |
| 41 | Code cleanup: remove TODOs, dead code, debug logs | `docs` |
| 42 | Git: squash/rebase messy WIP commits, write clean history | `docs` |
| 43 | Record gameplay video (all 3 levels + save restore demo) | `docs` |
| 44 | Final submission: zip project + APK + video link | `docs` |

---

## 11. Day-by-Day Schedule

| Day | Focus | Milestone target |
|---|---|---|
| Mon | Bootstrap + Board Logic | M1, M2 |
| Tue | Presentation + Input | M3 |
| Wed | Save + Integration | M4, M5 |
| Thu | Polish only | M6 |
| Fri | Release prep + submission | M7 |
