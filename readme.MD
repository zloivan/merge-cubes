# Merge Cubes — Test Assignment (OpenMyGame)

> Открыть сцену: `Assets/_Project/Scenes/Main.unity` → Play

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

---

## Архитектура за 30 секунд

```
Presentation  →  Application  →  Domain (pure C#)
                                  BoardModel, MatchFinder,
                                  GravityResolver, SwipeValidator

Infrastructure: SaveService, LevelRepository, InputService
Config: ScriptableObjects (GameConfigSO — единая точка настройки)
DI: VContainer (GameLifetimeScope)
Events: EventBus<T> (cross-system), прямые вызовы внутри системы
Async: UniTask | Tweens: DOTween
```

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

**File → Build Settings** → Android → IL2CPP → Build  
Min API 21. При необходимости — `Assets/link.xml`.
