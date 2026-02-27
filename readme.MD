# Merge Cubes — Test Assignment

> Открыть сцену: `Assets/_Project/Scenes/Main.unity` → Play

Мобильная match-3 головоломка для Android. Игрок свайпает блоки двух типов (огонь и вода) по сетке, формируя связные области из 3+ одинаковых блоков — они уничтожаются одновременно. Цель — очистить поле. После победы уровень меняется автоматически, уровни зациклены.

---

## Что реализовано по ТЗ

- [x] Свайп блоков в 4 направления, правило «вверх только на обмен»
- [x] Нормализация: гравитация → BFS flood-fill матч (линия ≥ 3) → уничтожение, цикл до стабильного состояния
- [x] Все крайние случаи матча из ТЗ (мульти-регионы уничтожаются одновременно)
- [x] Input заблокирован для падающих/уничтожаемых блоков, остальные доступны
- [x] 3 уровня (бесконечный цикл), автопереход после последней анимации уничтожения
- [x] Кнопки Restart и Next — работают во время нормализации без крашей
- [x] Idle и destroy анимации блоков
- [x] Шары на фоне: ≤3 на сцене, spawn из-за края, sinusоидная траектория, случайные скорость/высота/направление
- [x] Сохранение: уровень + полное состояние поля, восстановление при следующем запуске
- [x] IL2CPP Android, Portrait, адаптация под 16:9 и 4:3 без искажений

---

## Сверх ТЗ (бонусы)

- **Camera shake** — на уничтожение блоков (Cinemachine Impulse)
- **Звук** — swap, destroy (batch), win jingle, фоновая музыка
- **BackgroundFitter** — фон всегда полностью заполняет экран на любом соотношении сторон
- **Кастомный Level Editor** — визуальный редактор `LevelDataSO` прямо в инспекторе, cell-grid с цветовой индикацией типов блоков
- **Save corruption guard** — валидация версии, размеров и длины массива; при любом несоответствии тихий fallback на уровень 1 без краша
- **Async save** — запись сейва в `UniTask.RunOnThreadPool`, не блокирует main thread
- **Оптимизация** — Sprite Atlas, Camera Solid Color, отключён bloom; `Application.targetFrameRate = 60` (не греем девайс); 0 GC alloc в Update-путях; итог: 7 draw calls, 8 событий в Frame Debugger на кадр

---

## Архитектура

```
┌─────────────────────────────────────────────┐
│  Presentation (MonoBehaviours / Views)       │
│  BlockView, BoardView, BalloonSpawner, HUD   │
├─────────────────────────────────────────────┤
│  Application (Controllers / UseCases)        │
│  GameController, BoardController,            │
│  NormalizationController, LevelController    │
├─────────────────────────────────────────────┤
│  Domain (Pure C# — no Unity deps)            │
│  BoardModel, MatchFinder, GravityResolver,   │
│  SwipeValidator                              │
├─────────────────────────────────────────────┤
│  Infrastructure                              │
│  SaveService, LevelRepository, InputService  │
├─────────────────────────────────────────────┤
│  Config / Data (ScriptableObjects)           │
│  GameConfigSO, BlockConfigSO, LevelDataSO,   │
│  BalloonConfigSO                             │
└─────────────────────────────────────────────┘
```

DI: VContainer (`GameLifetimeScope`) | Events: `EventBus<T>` (cross-system) | Async: UniTask | Tweens: DOTween

Все настраиваемые параметры (длительности анимаций, размер ячейки, порог свайпа, camera shake force) — в `GameConfigSO`. Магических чисел в коде нет.

---

## Добавить новый уровень (без кода)

1. `Assets/_Project/Configs/Levels` → ПКМ → **Create → MergeCubes → Level Data**
2. Выставить `Width` / `Height`, заполнить сетку в инспекторе
3. Добавить ассет в `GameConfigSO.Levels[]`

Уровень сразу участвует в бесконечном цикле.

---

## Добавить новый тип блока (минимум кода)

1. Добавить значение в `BlockType` enum
2. `Assets/_Project/Configs/Blocks` → **Create → MergeCubes → Block Config** → задать `BlockType`, спрайт, `IdleClip`, `DestroyClip`
3. Добавить новый `BlockConfigSO` в `GameConfigSO.BlockConfigs[]`

Prefab создавать не нужно — все блоки используют один универсальный префаб `BlockView`; визуал и анимации подменяются через `AnimatorOverrideController` при спавне.

---

## Сборка APK

**Unity:** 2022.3.62f1  
**Platform:** Android, IL2CPP, .NET Standard 2.1, Portrait, Min API 25, Target API Automatic

1. **File → Build Settings** → выбрать Android
2. **Player Settings** → убедиться: IL2CPP, Portrait locked
3. **Build** → выбрать папку → готово

При необходимости добавить `Assets/link.xml` если IL2CPP стрипает нужные типы.
