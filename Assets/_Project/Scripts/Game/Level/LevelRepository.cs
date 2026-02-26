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

            for (var x = 0; x < levelData.Width; x++)
            for (var y = 0; y < levelData.Height; y++)
                blocks[x, y] = levelData.InitialBlocks[y * levelData.Width + x];

            return new LevelState(levelData.Width, levelData.Height, blocks, levelIndex);
        }

        public int GetNextIndex(int current) =>
            (current + 1) % _gameConfig.Levels.Length;

        public int Count() =>
            _gameConfig.Levels.Length;
    }
}