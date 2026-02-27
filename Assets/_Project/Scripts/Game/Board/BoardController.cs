using System;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using JetBrains.Annotations;
using MergeCubes.Core.Grid;
using MergeCubes.Events;
using UnityEngine;
using VContainer.Unity;

namespace MergeCubes.Game.Board
{
    /// <summary>
    /// Application layer. Wires SwipeInputEvent → validation → model mutation → normalization trigger. Blocks input during normalization.
    /// </summary>
    [UsedImplicitly]
    public class BoardController : IInitializable, IDisposable
    {
        private readonly SwipeValidator _swipeValidator;
        private readonly NormalizationController _normalizationController;
        private readonly BoardModel _boardModel;

        private EventBinding<SwipeInputEvent> _swipeBinding;

        public BoardController(SwipeValidator swipeValidator, NormalizationController normalizationController,
            BoardModel boardModel)
        {
            _swipeValidator = swipeValidator;
            _normalizationController = normalizationController;
            _boardModel = boardModel;
        }


        public void Initialize()
        {
            _swipeBinding = new EventBinding<SwipeInputEvent>(OnSwipe);
            EventBus<SwipeInputEvent>.Register(_swipeBinding);
        }

        public void Dispose() =>
            EventBus<SwipeInputEvent>.Deregister(_swipeBinding);

        private void OnSwipe(SwipeInputEvent e)
        {
            if (_normalizationController.GetIsNormalizing())
                return;

            if (!_swipeValidator.Validate(e.From, e.Dir, _boardModel))
                return;

            ApplyMove(e.From, e.Dir);
            

            EventBus<SwapExecutedEvent>.Raise(new SwapExecutedEvent(e.From,
                e.From + e.Dir.ToOffset()));
            
            _normalizationController.RunCycleAsync().Forget();
        }

        private void ApplyMove(GridPosition from, Direction dir)
        {
            var to = from + dir.ToOffset();

            if (_boardModel.IsEmpty(to))
                _boardModel.Move(from, to);
            else
                _boardModel.Swap(from, to);
        }
    }
}