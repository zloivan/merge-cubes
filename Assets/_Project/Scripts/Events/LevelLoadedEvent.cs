using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Game.Level;

namespace MergeCubes.Events
{
    public readonly struct LevelLoadedEvent : IEvent
    {
        public readonly LevelState Level;

        public LevelLoadedEvent(LevelState level) =>
            Level = level;
    }
}