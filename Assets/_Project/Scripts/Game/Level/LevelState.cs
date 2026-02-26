using MergeCubes.Game.Blocks;

namespace MergeCubes.Game.Level
{
    public readonly struct LevelState
    {
        public readonly int Width;
        public readonly int Height;
        public readonly BlockType[,] Blocks;
        public readonly int LevelIndex;

        public LevelState(int width, int height, BlockType[,] blocks, int levelIndex)
        {
            Width = width;
            Height = height;
            Blocks = blocks;
            LevelIndex = levelIndex;
        }
    }
}