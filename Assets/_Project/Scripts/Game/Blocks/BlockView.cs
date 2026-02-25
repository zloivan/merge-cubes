using MergeCubes.Core.Grid;
using UnityEngine;

namespace MergeCubes.Game.Blocks
{
    public class BlockView : MonoBehaviour
    {
        public GridPosition GetGridPosition() =>
            new GridPosition(1,1);
    }
}