using MergeCubes.Game.Blocks;
using MergeCubes.Game.Board;

namespace MergeCubes.Saving
{
    public static class SaveDataConverter
    {
        public const int CURRENT_VERSION = 1;

        public static SaveData FromBoard(BoardModel board, int levelIndex)
        {
            var snapshot = board.GetSnapshot();
            var width = board.GetWidth();
            var height = board.GetHeight();
            var blocks = new int[width * height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                blocks[y * width + x] = (int)snapshot[x, y];

            return new SaveData
            {
                LevelIndex = levelIndex,
                Width = width,
                Height = height,
                Blocks = blocks,
                SaveVersion = CURRENT_VERSION
            };
        }

        public static BlockType[,] ToBlockGrid(SaveData data)
        {
            var grid = new BlockType[data.Width, data.Height];
            for (var x = 0; x < data.Width; x++)
            for (var y = 0; y < data.Height; y++)
                grid[x, y] = (BlockType)data.Blocks[y * data.Width + x];
            return grid;
        }
    }
}