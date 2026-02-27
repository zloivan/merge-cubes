using System;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using JetBrains.Annotations;
using MergeCubes.Config;
using MergeCubes.Events;
using MergeCubes.Game.Board;
using MergeCubes.Game.Level;
using MergeCubes.Saving;
using UnityEngine;
using VContainer.Unity;

namespace MergeCubes.Bootstrap
{
    /// <summary>
    /// Manages level lifecycle: load, win detection, advance. Coordinates with SaveService and LevelRepository.
    /// </summary>
    [UsedImplicitly]
    public class LevelController : IInitializable, IDisposable
    {
        private readonly BoardModel _boardModel;
        private readonly ILevelRepository _levelRepository;
        private readonly GameConfigSO _gameConfig;

        private int _currentLevelIndex;

        private EventBinding<NormalizationCompletedEvent> _onNormalizationCompleted;
        private EventBinding<NextLevelRequestedEvent> _onNextLevelRequested;


        public LevelController(BoardModel boardModel, ILevelRepository levelRepository,
            GameConfigSO gameConfig)
        {
            _boardModel = boardModel;
            _levelRepository = levelRepository;
            _gameConfig = gameConfig;
        }

        public void Initialize()
        {
            _onNormalizationCompleted = new EventBinding<NormalizationCompletedEvent>(HandleNormalizationComplete);
            _onNextLevelRequested = new EventBinding<NextLevelRequestedEvent>(HandleNextLevelRequested);

            EventBus<NormalizationCompletedEvent>.Register(_onNormalizationCompleted);
            EventBus<NextLevelRequestedEvent>.Register(_onNextLevelRequested);
        }

        private void HandleNormalizationComplete(NormalizationCompletedEvent _) =>
            CheckWin().Forget();

        public void Dispose()
        {
            EventBus<NormalizationCompletedEvent>.Deregister(_onNormalizationCompleted);
            EventBus<NextLevelRequestedEvent>.Deregister(_onNextLevelRequested);
        }

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
            await UniTask.Delay(TimeSpan.FromSeconds(_gameConfig.WinDelay));
            LoadLevel(_levelRepository.GetNextIndex(_currentLevelIndex));
        }

        public int GetCurrentLevelIndex() =>
            _currentLevelIndex;
    }
}