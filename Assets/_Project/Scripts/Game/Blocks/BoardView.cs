using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MergeCubes.Config;
using MergeCubes.Core.Grid;
using MergeCubes.Events;
using MergeCubes.Game.Board;
using UnityEngine;
using VContainer;
using IKhom.EventBusSystem.Runtime;

namespace MergeCubes.Game.Blocks
{
    /// <summary>
    /// Presentation layer for the board. Manages BlockView lifecycle. Maps GridPosition ↔ world position. Signals animation completion to NormalizationController.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private Transform _blockViewContainer;
        [SerializeField] private BlockView _blockViewPrefab;

        private readonly Dictionary<GridPosition, BlockView> _viewsByGridPos = new();
        private BoardModel _boardModel;
        private NormalizationController _normalizationController;
        private GameConfigSO _gameConfig;
        private GridSystemVertical<GridObject> _gridSystem;


        private EventBinding<LevelLoadedEvent> _onLevelLoaded;
        private EventBinding<SwapExecutedEvent> _onSwapExecuted;
        private EventBinding<BlocksFellEvent> _onBlocksFell;
        private EventBinding<BlocksDestroyedEvent> _onBlocksDestroyed;

        [Inject]
        public void Construct(BoardModel boardModel, NormalizationController normalizationController,
            GameConfigSO gameConfig)
        {
            _boardModel = boardModel;
            _normalizationController = normalizationController;
            _gameConfig = gameConfig;
        }

        private void Awake()
        {
            if (!_blockViewContainer)
                _blockViewContainer = transform;
        }

        private void Start()
        {
            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(HandleLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);

            _onSwapExecuted = new EventBinding<SwapExecutedEvent>(HandleSwapExecuted);
            EventBus<SwapExecutedEvent>.Register(_onSwapExecuted);

            _onBlocksFell = new EventBinding<BlocksFellEvent>(HandleBlocksFell);
            EventBus<BlocksFellEvent>.Register(_onBlocksFell);

            _onBlocksDestroyed = new EventBinding<BlocksDestroyedEvent>(HandleBlocksDestroyed);
            EventBus<BlocksDestroyedEvent>.Register(_onBlocksDestroyed);
        }

        private void OnDestroy()
        {
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);
            EventBus<SwapExecutedEvent>.Deregister(_onSwapExecuted);
            EventBus<BlocksFellEvent>.Deregister(_onBlocksFell);
            EventBus<BlocksDestroyedEvent>.Deregister(_onBlocksDestroyed);
        }

        private void HandleLevelLoaded(LevelLoadedEvent eventArgs)
        {
            _gridSystem = new GridSystemVertical<GridObject>(
                _boardModel.GetWidth(), _boardModel.GetHeight(), (_, _) => null, _gameConfig.CellSize);

            SpawnAllBlocks();
        }

        private void SpawnAllBlocks()
        {
            foreach (var spawnedBlockView in _viewsByGridPos)
                spawnedBlockView.Value.SelfDestroy();

            _viewsByGridPos.Clear();

            for (var x = 0; x < _boardModel.GetWidth(); x++)
            {
                for (var y = 0; y < _boardModel.GetHeight(); y++)
                {
                    var pos = new GridPosition(x, y);
                    if (_boardModel.IsEmpty(pos))
                    {
                        continue;
                    }

                    SpawnBlock(pos);
                }
            }
        }

        private BlockView SpawnBlock(GridPosition pos)
        {
            var worldPos = _gridSystem.GetWorldPosition(pos);
            var blockView = Instantiate(_blockViewPrefab, worldPos, Quaternion.identity, _blockViewContainer);

            var blockConfig = _gameConfig.GetBlockConfig(_boardModel.GetBlockType(pos));
            blockView.Initialize(blockConfig);
            blockView.SetGridPosition(pos);

            _viewsByGridPos[pos] = blockView;

            return blockView;
        }

        private void HandleSwapExecuted(SwapExecutedEvent eventArg)
        {
            //Check if there are views spawned
            //convert swap grid pos to world pos
            //move both blocks to destination world position
            //update dictionary and grid position of views

            if (!_viewsByGridPos.TryGetValue(eventArg.A, out var a)
                || !_viewsByGridPos.TryGetValue(eventArg.B, out var b))
            {
                Debug.Log("No Views spawned for the swap");
                return;
            }

            var aWorldPos = _gridSystem.GetWorldPosition(eventArg.A);
            var bWorldPos = _gridSystem.GetWorldPosition(eventArg.B);

            a.MoveToAsync(bWorldPos, _gameConfig.BlockSwapDuration, _gameConfig.BlockSwapEase).Forget();
            b.MoveToAsync(aWorldPos, _gameConfig.BlockSwapDuration, _gameConfig.BlockSwapEase).Forget();

            _viewsByGridPos[eventArg.A] = b;
            _viewsByGridPos[eventArg.B] = a;
            b.SetGridPosition(eventArg.A);
            a.SetGridPosition(eventArg.B);
        }

        private void HandleBlocksFell(BlocksFellEvent eventArgs) =>
            AnimateFallAsync(eventArgs).Forget();

        private async UniTask AnimateFallAsync(BlocksFellEvent eventArgs)
        {
            //for each fall convert target to world coord
            //check if origin pos has view
            // update dictionary
            // move view to target
            //do for each view and wait for all to complete, then update the normalizer

            var fallTasks = new UniTask[eventArgs.Drops.Count];

            for (var i = 0; i < eventArgs.Drops.Count; i++)
            {
                var drop = eventArgs.Drops[i];
                var worldTo = _gridSystem.GetWorldPosition(drop.To);

                if (!_viewsByGridPos.Remove(drop.From, out var view))
                {
                    Debug.LogWarning("No view spawned for the fall");
                    continue;
                }

                _viewsByGridPos[drop.To] = view;
                view.SetGridPosition(drop.To);

                fallTasks[i] = view.MoveToAsync(worldTo, _gameConfig.BlockFallDuration, _gameConfig.BlockFallEase);
            }

            await UniTask.WhenAll(fallTasks);
            _normalizationController.NotifyFallComplete();
        }

        private void HandleBlocksDestroyed(BlocksDestroyedEvent eventArgs) =>
            AnimateDestroyAsync(eventArgs).Forget();

        private async UniTask AnimateDestroyAsync(BlocksDestroyedEvent eventArgs)
        {
            var allPositions = eventArgs.Regions.SelectMany(r => r).ToList();
            var destroyTasks = new List<UniTask>(allPositions.Count);

            foreach (var gridPosition in allPositions)
            {
                if (!_viewsByGridPos.Remove(gridPosition, out var view))
                {
                    Debug.LogWarning("No view spawned for the destruction");
                    continue;
                }

                destroyTasks.Add(view.SelfDestroyAnimatedAsync(_gameConfig.BlockDestroyDuration));
            }

            await UniTask.WhenAll(destroyTasks);
            _normalizationController.NotifyDestroyCompleted();
        }
    }
}