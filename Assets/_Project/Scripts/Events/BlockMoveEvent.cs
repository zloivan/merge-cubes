using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Core.Grid;

namespace MergeCubes.Events
{
    public readonly struct BlockMovedEvent : IEvent
    {
        public readonly GridPosition From;
        public readonly GridPosition To;

        public BlockMovedEvent(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }
    }
}