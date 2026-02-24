using System.Collections.Generic;
using MergeCubes.Core.Grid;

namespace MergeCubes.Game.Board
{
    public class GravityResolver
    {
        public List<DropMove> Resolve(BoardModel model)
        {
            var result = new List<DropMove>();
            // Iterates each column bottom-up
            // For each empty cell, find the lowest non-empty block above it
            // Returns ordered list of DropMove (bottom-most drops first)
            // Returns empty list if nothing to fall

            //E[0,3]0[1,3]E[2,3]
            //E[0,2]0[1,2]E[2,2]
            //O[0,1]0[1,1]E[2,1]
            //E[0,0]E[1,0]E[2,0]

            for (var x = 0; x < model.GetWidth(); x++)
                ResolveRow(model, x, result);

            return result;
        }

        private void ResolveRow(BoardModel board, int col, List<DropMove> drops)
        {
            var writeRow = 0;

            for (var row = 0; row < board.GetHeight(); row++)
            {
                var pos = new GridPosition(col, row);

                if (board.IsEmpty(pos))
                    continue;

                if (row != writeRow)
                {
                    drops.Add(new DropMove(pos, new GridPosition(col, writeRow)));
                }

                writeRow++;
            }
        }
    }

    public readonly struct DropMove
    {
        public readonly GridPosition From;
        public readonly GridPosition To;

        public DropMove(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }
    }
}