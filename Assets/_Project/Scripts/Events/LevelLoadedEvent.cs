using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Game.Level;

namespace MergeCubes.Events
{
    public readonly struct LevelLoadedEvent : IEvent
    {
        public readonly LevelState Level;
        public readonly int LevelIndex;

        public LevelLoadedEvent(LevelState level, int levelIndex)
        {
            Level = level;
            LevelIndex = levelIndex;
        }
    }
}