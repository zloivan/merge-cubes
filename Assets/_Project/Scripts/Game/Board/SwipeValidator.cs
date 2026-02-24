using System;
using MergeCubes.Core.Grid;

namespace MergeCubes.Game.Board
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class SwipeValidator
    {
        public bool Validate(GridPosition from, Direction direction, BoardModel model)
        {
            // Upward swipe to empty cell returns false
            // Upward swipe to occupied cell returns true
            // Out-of-bounds target returns false in all directions
            // Swipe from empty cell returns false
            
            var to = from + direction.ToOffset();
            
            if (model.IsEmpty(from))
                return false;

            if (direction == Direction.Up && model.IsEmpty(to))
                return false;

            if (!model.IsInBounds(to))
                return false;

            return true;
        }
    }

    public static class DirectionExtensions
    {
        public static GridPosition ToOffset(this Direction dir)
        {
            return dir switch
            {
                Direction.Up => new GridPosition(0, 1),
                Direction.Down => new GridPosition(0, -1),
                Direction.Left => new GridPosition(-1, 0),
                Direction.Right => new GridPosition(1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }
    }
}