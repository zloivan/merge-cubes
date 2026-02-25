using Cysharp.Threading.Tasks;
using DG.Tweening;
using MergeCubes.Config;
using MergeCubes.Core.Grid;
using UnityEngine;

namespace MergeCubes.Game.Blocks
{
    public class BlockView : MonoBehaviour
    {
        private BlockConfigSO _blockConfig;
        private GridPosition _gridPosition;


        public void SelfDestroy() =>
            Destroy(gameObject);

        public void Initialize(BlockConfigSO blockConfig)
        {
            _blockConfig = blockConfig;
        }

        public GridPosition GetGridPosition() =>
            _gridPosition;

        public void SetGridPosition(GridPosition gridPosition) =>
            _gridPosition = gridPosition;

        public async UniTask MoveToAsync(Vector3 bWorldPos, float gameConfigBlockMoveDuration,
            Ease gameConfigBlockMoveEase)
        {
            throw new System.NotImplementedException();
        }

        public async UniTask SelfDestroyAnimatedAsync(float gameConfigBlockDestroyDuration)
        {
            throw new System.NotImplementedException();
        }
    }
}