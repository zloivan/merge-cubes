using JetBrains.Annotations;
using MergeCubes.Config;
using MergeCubes.Game.Blocks;

namespace MergeCubes.Game.Level
{
    [UsedImplicitly]
    public class LevelRepository : ILevelRepository
    {
        private readonly GameConfigSO _gameConfig;

        public LevelRepository(GameConfigSO gameConfig) =>
            _gameConfig = gameConfig;

        public LevelState GetLevelByIndex(int levelIndex)
        {
            var levelData = _gameConfig.Levels[levelIndex];
            var blocks = new BlockType[levelData.Width, levelData.Height];

            for (var row = 0; row < levelData.Height; row++)
            for (var col = 0; col < levelData.Width; col++)
                blocks[col, row] = levelData.InitialBlocks[row * levelData.Width + col];

            return new LevelState(levelData.Width, levelData.Height, blocks, levelIndex);
        }

        public int GetNextIndex(int current) =>
            (current + 1) % _gameConfig.Levels.Length;

        public int Count() =>
            _gameConfig.Levels.Length;
    }
}