using System;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using JetBrains.Annotations;
using MergeCubes.Core.Grid;
using MergeCubes.Events;
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

        private void OnSwipe(SwipeInputEvent eventArts)
        {
            if (_normalizationController.GetIsNormalizing())
                return;

            if (_swipeValidator.Validate(eventArts.From, eventArts.Dir, _boardModel))
                return;

            ApplyMove(eventArts.From, eventArts.Dir);

            EventBus<SwapExecutedEvent>.Raise(new SwapExecutedEvent(eventArts.From,
                eventArts.From + eventArts.Dir.ToOffset()));

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