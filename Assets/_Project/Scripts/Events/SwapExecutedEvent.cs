using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Core.Grid;

namespace MergeCubes.Events
{
    public readonly struct SwapExecutedEvent : IEvent
    {
        public readonly GridPosition A;
        public readonly GridPosition B;

        public SwapExecutedEvent(GridPosition a, GridPosition b)
        {
            A = a;
            B = b;
        }
    }
}