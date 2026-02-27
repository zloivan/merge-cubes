# Cube Merge – Architecture & Project Decomposition

---

## 1. High-Level Architecture

### Guiding Principles
- **VContainer** for all DI (LifetimeScope per scene)
- **SOLID** – each class has one reason to change
- No magic numbers – all tuning values live in `ScriptableObject` configs
- World-space gameplay (not UI Canvas), camera is orthographic portrait
- GridSystem embedded as plain C# classes (source-copied, no package); EventBus and SoundSystem remain as UPM package references
- EventBus for cross-system communication; direct calls within the same system
- No premature pooling; add only where profiler proves necessary

---

## 2. Tech Conventions

| Tool | When to use |
|---|---|
| **UniTask** | All async coordination: awaiting animation completion, normalization steps, save I/O, level load sequencing |
| **DOTween** | All tween-based animation (block move, fall, destroy scale, UI transitions). Use easing functions, not manual lerp in Update |
| **Update** | Only when genuinely simpler — e.g. BalloonSpawner sin movement (per-frame positional math, no easing needed) |

Rule: if DOTween easing makes intent clearer → use DOTween. If Update is 3 lines and DOTween would be 10 → use Update.

---

## 3. Layer Map (!!)

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

## 4. Core Classes & Responsibilities (!!)

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

## 5. Key Data Flows (!!)

### Swipe → Normalize
```
InputService
  → SwipeInputEvent
    → BoardController.OnSwipe()
      → SwipeValidator.Validate()
      → BoardModel.Swap() / Move()
      → SwapExecutedEvent
        → NormalizationController.RunCycleAsync()
          loop until stable:
            GravityResolver → drops → BlocksFellEvent
            MatchFinder     → regions → BlocksDestroyedEvent
          → NormalizationCompleteEvent
            → LevelController.CheckWin()
```

### Save Trigger Points
- After every `NormalizationCompleteEvent` (board is stable)
- On `Application.quitting` / `OnApplicationPause(true)`

---

## 6. Level Format (ScriptableObject)

```
LevelData (ScriptableObject)
  int   Width
  int   Height
  BlockType[] InitialBlocks   // row-major, index = row * Width + col, row 0 = bottom
```

Five levels ship with the project; adding a new level = create new SO, fill grid, done – no code.

---

## 7. GridSystem Integration

GridSystem source is copied from another project into `_Project/Scripts/Core/Grid/` — **not** a package reference.  
EventBus and SoundSystem remain as UPM package references.

### Vertical Grid Adaptation

The game requires a **vertical world-space grid** (X = column, Y = row, Z = 0) for a portrait 2D layout. The base `GridSystemBase<T>` uses a 3D coordinate convention (`X, Z` axes). We introduce a specialized subclass:

**`GridSystemVertical<T> : GridSystemBase<T>`**
- Overrides `GetWorldPosition(GridPosition)` → returns `new Vector3(pos.X, pos.Z, 0) * cellSize`  
  (maps grid Z→world Y, keeping Z=0 for flat 2D)
- Overrides `GetGridPosition(Vector3)` → inverse of above
- Overrides `GetNeighbors(GridPosition)` → same 4-directional as `GridSystemSquare` (no change needed)
- This is the **only** grid system class used in this project; `GridSystemSquare` and `GridSystemHex` are kept but unused


## 8. Camera & Resolution Fit (!!)

**Orthographic camera** — mandatory for flat world-space grid.

`CameraFitter : MonoBehaviour` subscribes to `LevelLoadedEvent` and recalculates `Camera.orthographicSize`:

```
orthographicSize = (gridHeight * cellSize / 2) + verticalPadding

// Clamp for wide/tablet screens:
requiredHalfWidth = (gridWidth * cellSize / 2) + horizontalPadding
cameraHalfWidth   = orthographicSize * Screen.width / Screen.height
if (requiredHalfWidth > cameraHalfWidth)
    orthographicSize = requiredHalfWidth * Screen.height / Screen.width
```

`verticalPadding` and `horizontalPadding` are tunable constants in `GameConfig` SO.

---

## 9. Data Layers (Config vs Persistence)

| Layer | Type | Who writes | Who reads |
|---|---|---|---|
| Static config | ScriptableObjects | Designer (Editor) | `LevelRepository`, VContainer |
| Runtime save | JSON (`persistentDataPath`) | `SaveService` | `SaveService` |
| Domain state | Pure C# POCOs | Controllers | Controllers, Views via events |

**ScriptableObjects are designed to read-only in runtime builds.**
`SaveData` is a plain C# class: `{ int levelIndex; int[] blocks; }`.  
`LevelRepository` converts `LevelData` SO → plain `LevelState` struct before handing to controllers.

**Centralized config:** One `GameConfigSO` SO holds references to `BlockConfigSO[]`, `BalloonConfigSO`, level list, camera padding, animation timings.

---

## 10. VContainer Scene Scope (!!)

```
GameLifetimeScope
  ├─ GameConfig (SO) 
  ├─ LevelRepository
  ├─ SaveService
  ├─ BoardModel        
  ├─ SwipeValidator
  ├─ GravityResolver
  ├─ MatchFinder
  ├─ BoardController
  ├─ NormalizationController
  ├─ LevelController
  ├─ GameController
  ├─ InputService       (MonoBehaviour)
  ├─ BoardView          (MonoBehaviour)
  ├─ BalloonSpawner     (MonoBehaviour)
  └─ HUDController      (MonoBehaviour)
```

---

## 11. Events (EventBus<T>) (!!)

| Event | Payload | Raised by | Consumed by |
|---|---|---|---|
| `SwipeInputEvent` | `GridPosition From, Direction Dir` | InputService | BoardController |
| `SwapExecutedEvent` | `GridPosition A, B` | BoardController | BoardView |
| `BlocksFellEvent` | `List<DropMove> Drops` | NormalizationController | BoardView |
| `BlocksDestroyedEvent` | `List<HashSet<GridPosition>> Regions` | NormalizationController | BoardView |
| `NormalizationCompleteEvent` | – | NormalizationController | LevelController, SaveService |
| `LevelWonEvent` | – | LevelController | HUDController, LevelController |
| `LevelLoadedEvent` | `LevelState Level, int LevelIndex` | LevelController | BoardView, BoardController, CameraFitter |
| `RestartRequestedEvent` | – | HUDController | GameController |
| `NextLevelRequestedEvent` | – | HUDController | LevelController |

