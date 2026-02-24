using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Events;

namespace MergeCubes.Game.Board
{
    /// <summary>
    /// Application layer. Runs gravity→match loop until board is stable. Owns IsNormalizing flag. Coordinates with views via async UniTask awaiting.
    /// </summary>
    public class NormalizationController
    {
        private readonly BoardModel _boardModel;
        private readonly GravityResolver _gravityResolver;
        private readonly MatchFinder _matchFinder;

        private UniTaskCompletionSource _fallCompleted;
        private UniTaskCompletionSource _destroyCompleted;
        private CancellationTokenSource _cts;


        private bool _isNormalizing;

        public NormalizationController(BoardModel boardModel, GravityResolver gravityResolver, MatchFinder matchFinder)
        {
            _boardModel = boardModel;
            _gravityResolver = gravityResolver;
            _matchFinder = matchFinder;
        }

        public async UniTask RunCycleAsync()
        {
            // a. drops = GravityResolver.Resolve(board) → if any: apply to model, raise BlocksFellEvent, await UniTask signal from BoardView that all fall tweens completed
            // b. regions = MatchFinder.FindRegions(board) → if any: apply removes to model, raise BlocksDestroyedEvent, await UniTask signal from BoardView that all destroy anims completed
            // c. If neither drops nor regions → break

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _isNormalizing = true;

            try
            {
                while (true)
                {
                    var dropped = await TryApplyGravityAsync(token);
                    var matched = await TryApplyMatchesAsync(token);

                    if (!dropped && !matched)
                        break;
                }
            }

            finally
            {
                _isNormalizing = false;
                _cts = null;
            }


            EventBus<NormalizationCompletedEvent>.Raise(new NormalizationCompletedEvent());
        }

        public void Cancel()
        {
            _cts.Cancel();
            _cts = null;
            _isNormalizing = false;
        }

        public bool GetIsNormalizing() =>
            _isNormalizing;

        public void NotifyFallComplete() =>
            _fallCompleted?.TrySetResult();

        public void NotifyDestroyCompleted() =>
            _destroyCompleted?.TrySetResult();

        private async UniTask<bool> TryApplyMatchesAsync(CancellationToken token)
        {
            var regions = _matchFinder.FindRegions(_boardModel);

            if (regions.Count == 0)
                return false;

            foreach (var pos in regions.SelectMany(region => region))
            {
                _boardModel.Remove(pos);
            }

            _destroyCompleted = new UniTaskCompletionSource();
            EventBus<BlocksDestroyedEvent>.Raise(new BlocksDestroyedEvent(regions));
            await _destroyCompleted.Task.AttachExternalCancellation(token);

            return true;
        }

        private async UniTask<bool> TryApplyGravityAsync(CancellationToken token)
        {
            var drops = _gravityResolver.Resolve(_boardModel);

            if (drops.Count == 0)
                return false;

            foreach (var dropMove in drops)
            {
                _boardModel.Move(dropMove.From, dropMove.To);
            }

            _fallCompleted = new UniTaskCompletionSource();
            EventBus<BlocksFellEvent>.Raise(new BlocksFellEvent(drops));

            await _fallCompleted.Task.AttachExternalCancellation(token);
            return true;
        }
    }
}