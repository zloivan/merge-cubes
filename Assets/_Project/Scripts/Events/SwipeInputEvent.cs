using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Core.Grid;
using MergeCubes.Game.Board;

namespace MergeCubes.Events
{
    
    public readonly struct SwipeInputEvent : IEvent
    {
        public readonly GridPosition From;
        public readonly Direction Dir;

        public SwipeInputEvent(GridPosition from, Direction dir)
        {
            From = from;
            Dir = dir;
        }
    }
}