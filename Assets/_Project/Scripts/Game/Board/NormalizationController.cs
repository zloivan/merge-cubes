using Cysharp.Threading.Tasks;

namespace MergeCubes.Game.Board
{
    public class NormalizationController
    {
        private bool _isNormalizing;

        public async UniTask RunCycleAsync()
        {
        } // пока пусто

        public bool GetIsNormalizing() =>
            _isNormalizing;
    }
}