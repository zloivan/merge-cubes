namespace MergeCubes.Game.Level
{
    public interface ILevelRepository
    {
        LevelState GetLevelByIndex(int levelIndex);
        int GetNextIndex(int current);
        int Count();
    }
}