using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Core.Grid;
using MergeCubes.Events;
using MergeCubes.Game.Blocks;
using UnityEngine;
using VContainer;

namespace MergeCubes.Game.Board
{
    /// <summary>
    ///     Presentation layer for the board. Manages BlockView lifecycle. Maps GridPosition ↔ world position. Signals
    ///     animation completion to NormalizationController.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private Transform _blockViewContainer;
        [SerializeField] private BlockView _blockViewPrefab;
        [SerializeField] private Transform _gridAnchor;

        private readonly Dictionary<GridPosition, BlockView> _viewsByGridPos = new();
        private BoardModel _boardModel;
        private GameConfigSO _gameConfig;
        private GridSystemVertical<GridObject> _gridSystem;
        private NormalizationController _normalizationController;
        private EventBinding<BlockMovedEvent> _onBlockMoved;
        private EventBinding<BlocksDestroyedEvent> _onBlocksDestroyed;
        private EventBinding<BlocksFellEvent> _onBlocksFell;


        private EventBinding<LevelLoadedEvent> _onLevelLoaded;
        private EventBinding<SwapExecutedEvent> _onSwapExecuted;

        private void Awake()
        {
            if (!_blockViewContainer)
                _blockViewContainer = transform;

            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(HandleLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);

            _onSwapExecuted = new EventBinding<SwapExecutedEvent>(HandleSwapExecuted);
            EventBus<SwapExecutedEvent>.Register(_onSwapExecuted);

            _onBlocksFell = new EventBinding<BlocksFellEvent>(HandleBlocksFell);
            EventBus<BlocksFellEvent>.Register(_onBlocksFell);

            _onBlocksDestroyed = new EventBinding<BlocksDestroyedEvent>(HandleBlocksDestroyed);
            EventBus<BlocksDestroyedEvent>.Register(_onBlocksDestroyed);

            _onBlockMoved = new EventBinding<BlockMovedEvent>(HandleBlockMoved);
            EventBus<BlockMovedEvent>.Register(_onBlockMoved);
        }

        private void OnDestroy()
        {
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);
            EventBus<SwapExecutedEvent>.Deregister(_onSwapExecuted);
            EventBus<BlocksFellEvent>.Deregister(_onBlocksFell);
            EventBus<BlocksDestroyedEvent>.Deregister(_onBlocksDestroyed);
            EventBus<BlockMovedEvent>.Deregister(_onBlockMoved);
        }

        [Inject]
        public void Construct(BoardModel boardModel, NormalizationController normalizationController,
            GameConfigSO gameConfig)
        {
            _boardModel = boardModel;
            _normalizationController = normalizationController;
            _gameConfig = gameConfig;
        }

        private void HandleLevelLoaded(LevelLoadedEvent eventArgs)
        {
            _gridSystem = new GridSystemVertical<GridObject>(
                _boardModel.GetWidth(), _boardModel.GetHeight(), (_, _) => null, _gameConfig.CellSize, GetGridOrigin());

            _blockViewContainer.localPosition = Vector3.zero;

            SpawnAllBlocks();
        }

        private Vector3 GetGridOrigin()
        {
            var gridWorldWidth = _boardModel.GetWidth() * _gameConfig.CellSize;
            var anchorPos = _gridAnchor ? _gridAnchor.position : Vector3.zero;

            return new Vector3(
                anchorPos.x - (gridWorldWidth - _gameConfig.CellSize) / 2f,
                anchorPos.y,
                0f);
        }

        private void SpawnAllBlocks()
        {
            foreach (var spawnedBlockView in _viewsByGridPos)
                spawnedBlockView.Value.SelfDestroy();

            _viewsByGridPos.Clear();

            for (var x = 0; x < _boardModel.GetWidth(); x++)
            for (var y = 0; y < _boardModel.GetHeight(); y++)
            {
                var pos = new GridPosition(x, y);
                if (_boardModel.IsEmpty(pos)) continue;

                SpawnBlock(pos);
            }
        }

        private BlockView SpawnBlock(GridPosition pos)
        {
            var worldPos = _gridSystem.GetWorldPosition(pos);
            var blockView = Instantiate(_blockViewPrefab, worldPos, Quaternion.identity, _blockViewContainer);

            var blockConfig = _gameConfig.GetBlockConfig(_boardModel.GetBlockType(pos));
            blockView.Initialize(blockConfig, _boardModel.GetWidth());
            blockView.SetGridPosition(pos);

            _viewsByGridPos[pos] = blockView;

            return blockView;
        }

        private void HandleSwapExecuted(SwapExecutedEvent e)
        {
            //Check if there are views spawned
            //convert swap grid pos to world pos
            //move both blocks to destination world position
            //update dictionary and grid position of views

            if (!_viewsByGridPos.TryGetValue(e.A, out var a)
                || !_viewsByGridPos.TryGetValue(e.B, out var b))
            {
                Debug.Log("No Views spawned for the swap");
                return;
            }

            var aWorldPos = _gridSystem.GetWorldPosition(e.A);
            var bWorldPos = _gridSystem.GetWorldPosition(e.B);

            a.MoveToAsync(bWorldPos, _gameConfig.BlockSwapDuration, _gameConfig.BlockSwapEase).Forget();
            b.MoveToAsync(aWorldPos, _gameConfig.BlockSwapDuration, _gameConfig.BlockSwapEase).Forget();

            _viewsByGridPos[e.A] = b;
            _viewsByGridPos[e.B] = a;
            b.SetGridPosition(e.A);
            a.SetGridPosition(e.B);
        }

        private void HandleBlockMoved(BlockMovedEvent e)
        {
            if (!_viewsByGridPos.Remove(e.From, out var view))
                return;

            var worldTo = _gridSystem.GetWorldPosition(e.To);
            _viewsByGridPos[e.To] = view;
            view.SetGridPosition(e.To);
            view.MoveToAsync(worldTo, _gameConfig.BlockSwapDuration, _gameConfig.BlockSwapEase).Forget();
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
            var destroyTasks = new List<UniTask>();

            foreach (var region in eventArgs.Regions)
            {
                var center = GetRegionCenter(region);
                var sorted = region
                    .OrderBy(p => ManhattanDistance(p, center))
                    .ToList();

                for (var i = 0; i < sorted.Count; i++)
                {
                    if (!_viewsByGridPos.Remove(sorted[i], out var view))
                        continue;

                    var delay = i * _gameConfig.DestroyStaggerDelay;
                    destroyTasks.Add(DestroyWithDelayAsync(view, delay));
                }
            }

            await UniTask.WhenAll(destroyTasks);
            _normalizationController.NotifyDestroyCompleted();
        }

        private async UniTask DestroyWithDelayAsync(BlockView view, float delay)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            await view.SelfDestroyAnimatedAsync(_gameConfig.BlockDestroyDuration);
        }

        private GridPosition GetRegionCenter(HashSet<GridPosition> region) =>
            new(
                Mathf.RoundToInt((float)region.Average(p => p.X)),
                Mathf.RoundToInt((float)region.Average(p => p.Z))
            );

        private int ManhattanDistance(GridPosition a, GridPosition b) =>
            Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Z - b.Z);
    }
}