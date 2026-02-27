using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using JetBrains.Annotations;
using MergeCubes.Config;
using MergeCubes.Events;
using MergeCubes.Game.Board;
using MergeCubes.Saving;
using VContainer.Unity;

namespace MergeCubes.Game.Level
{
    /// <summary>
    ///     Manages level lifecycle: load, win detection, advance. Coordinates with SaveService and LevelRepository.
    /// </summary>
    [UsedImplicitly]
    public class LevelController : IInitializable, IDisposable
    {
        private readonly BoardModel _boardModel;
        private readonly GameConfigSO _gameConfig;
        private readonly ILevelRepository _levelRepository;
        private CancellationTokenSource _cts;

        private int _currentLevelIndex;
        private EventBinding<LevelLoadedEvent> _onLevelLoaded;
        private EventBinding<NextLevelRequestedEvent> _onNextLevelRequested;

        private EventBinding<NormalizationCompletedEvent> _onNormalizationCompleted;

        public LevelController(BoardModel boardModel, ILevelRepository levelRepository,
            GameConfigSO gameConfig)
        {
            _boardModel = boardModel;
            _levelRepository = levelRepository;
            _gameConfig = gameConfig;
        }

        public void Dispose()
        {
            EventBus<NormalizationCompletedEvent>.Deregister(_onNormalizationCompleted);
            EventBus<NextLevelRequestedEvent>.Deregister(_onNextLevelRequested);
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);

            _cts?.Cancel();
            _cts = null;
        }

        public void Initialize()
        {
            _onNormalizationCompleted = new EventBinding<NormalizationCompletedEvent>(HandleNormalizationComplete);
            _onNextLevelRequested = new EventBinding<NextLevelRequestedEvent>(HandleNextLevelRequested);
            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(HandleLevelLoaded);

            EventBus<NormalizationCompletedEvent>.Register(_onNormalizationCompleted);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);
            EventBus<NextLevelRequestedEvent>.Register(_onNextLevelRequested);
        }

        private void HandleLevelLoaded(LevelLoadedEvent obj)
        {
            _cts?.Cancel();
            _cts = null;
        }

        private void HandleNormalizationComplete(NormalizationCompletedEvent _) =>
            CheckWin().Forget();

        public void LoadLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;

            var levelState = _levelRepository.GetLevelByIndex(levelIndex);
            _boardModel.Initialize(levelState);

            EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent(levelState, levelIndex));
        }

        public void LoadFromSave(SaveData saveData)
        {
            var blocks = SaveDataConverter.ToBlockGrid(saveData);
            var levelState = new LevelState(saveData.Width, saveData.Height, blocks, saveData.LevelIndex);

            _currentLevelIndex = saveData.LevelIndex;
            _boardModel.Initialize(levelState);

            EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent(levelState, _currentLevelIndex));
        }

        private void HandleNextLevelRequested(NextLevelRequestedEvent _) =>
            LoadLevel(_levelRepository.GetNextIndex(_currentLevelIndex));

        private async UniTask CheckWin()
        {
            if (!_boardModel.IsAllEmpty())
                return;

            EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
            _cts = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(_gameConfig.WinDelay), cancellationToken: _cts.Token);
            LoadLevel(_levelRepository.GetNextIndex(_currentLevelIndex));
        }

        public int GetCurrentLevelIndex() =>
            _currentLevelIndex;
    }
}