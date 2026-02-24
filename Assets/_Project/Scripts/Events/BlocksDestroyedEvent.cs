using System.Collections.Generic;
using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Core.Grid;

namespace MergeCubes.Events
{
    public readonly struct BlocksDestroyedEvent : IEvent
    {
        public readonly List<HashSet<GridPosition>> Regions;

        public BlocksDestroyedEvent(List<HashSet<GridPosition>> regions) =>
            Regions = regions;
    }
}