using System.Collections.Generic;
using IKhom.EventBusSystem.Runtime.abstractions;
using MergeCubes.Game.Board;

namespace MergeCubes.Events
{
    public readonly struct BlocksFellEvent : IEvent

    {
        public readonly List<DropMove> Drops;

        public BlocksFellEvent(List<DropMove> drops) =>
            Drops = drops;
    }
}