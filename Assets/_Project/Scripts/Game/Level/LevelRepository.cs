using JetBrains.Annotations;
using MergeCubes.Config;

namespace MergeCubes.Game.Level
{
    [UsedImplicitly]
    public class LevelRepository
    {
        private readonly GameConfigSO _gameConfig;

        public LevelRepository(GameConfigSO gameConfig) =>
            _gameConfig = gameConfig;

        public LevelState GetLevelByIndex(int levelIndex)
        {
            var levelData = _gameConfig.Levels[levelIndex];

            var blocks = new BlockType[levelData.Height, levelData.Width];

            //TODO: Тут собираются ячейки грида, если будут проблемы с порядком то сюда!
            var initial = levelData.InitialBlocks;
            for (var i = 0; i < levelData.Height; i++)
            {
                for (var j = 0; j < levelData.Width; j++)
                {
                    var flatIndex = i * levelData.Width + j;
                    blocks[i, j] = initial != null && flatIndex < initial.Length
                        ? initial[flatIndex]
                        : default;
                }
            }

            return new LevelState(levelData.Width, levelData.Height, blocks, levelIndex);
        }

        public int GetNextIndex(int current) =>
            (current + 1) % _gameConfig.Levels.Length;

        public int Count() =>
            _gameConfig.Levels.Length;
    }
}