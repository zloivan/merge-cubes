using JetBrains.Annotations;
using MergeCubes.Core.Grid;
using MergeCubes.Game.Blocks;
using MergeCubes.Game.Level;

namespace MergeCubes.Game.Board
{
    [UsedImplicitly]
    public class BoardModel
    {
        private BlockType[,] _gridArray;
        private int _width;
        private int _height;

        public void Initialize(LevelState levelState)
        {
            _width = levelState.Width;
            _height = levelState.Height;
            _gridArray = levelState.Blocks;
        }

        public int GetWidth() =>
            _width;

        public int GetHeight() =>
            _height;

        public BlockType GetBlockType(GridPosition pos) =>
            _gridArray[pos.X, pos.Z];

        public bool IsEmpty(GridPosition pos) =>
            _gridArray[pos.X, pos.Z] == BlockType.None;

        public bool IsInBounds(GridPosition pos) =>
            pos.X >= 0 && pos.X < _width && pos.Z >= 0 && pos.Z < _height;

        public bool IsAllEmpty()
        {
            foreach (var blockType in _gridArray)
            {
                if (blockType != BlockType.None)
                {
                    return false;
                }
            }

            return true;
        }

        public void Swap(GridPosition a, GridPosition b) =>
            (_gridArray[a.X, a.Z], _gridArray[b.X, b.Z]) = (_gridArray[b.X, b.Z], _gridArray[a.X, a.Z]);

        public void Move(GridPosition from, GridPosition to)
        {
            _gridArray[to.X, to.Z] = _gridArray[from.X, from.Z];
            _gridArray[from.X, from.Z] = BlockType.None;
        }

        public void Remove(GridPosition from) =>
            _gridArray[from.X, from.Z] = BlockType.None;

        public BlockType[,] GetSnapshot()
        {
            var copy = new BlockType[_width, _height];
            System.Array.Copy(_gridArray, copy, _gridArray.Length);
            return copy;
        }
    }
}