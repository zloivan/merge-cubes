using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Core.Grid;

namespace MergeCubes.Events
{
    public readonly struct SwapExecutedEvent : IEvent
    {
        public readonly GridPosition From;
        public readonly GridPosition To;

        public SwapExecutedEvent(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }
    }
}