using System;
using System.Collections.Generic;
using System.Linq;
using MergeCubes.Core.Grid;

namespace MergeCubes.Game.Board
{
    public class MatchFinder
    {
        private const int MIN_LINE = 3;
        private readonly Direction[] All = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        public List<HashSet<GridPosition>> FindRegions(BoardModel board)
        {
            // BFS flood-fill from each unvisited non-empty cell → build connected region of same BlockType
            // For each region: check if it contains ≥1 horizontal run of 3+ cells OR ≥1 vertical run of 3+ cells in the same column
            // If qualifies → add full region to result (not just the line — the entire connected area)
            // All qualifying regions collected before any destruction
            
            var result = new List<HashSet<GridPosition>>();
            var visited = new HashSet<GridPosition>();


            for (var x = 0; x < board.GetWidth(); x++)
            {
                for (var y = 0; y < board.GetHeight(); y++)
                {
                    var pos = new GridPosition(x, y);

                    if (visited.Contains(pos) || board.IsEmpty(pos))
                    {
                        continue;
                    }

                    var region = FloodFill(pos, board, visited);

                    if (HasQualifingLine(region))
                    {
                        result.Add(region);
                    }
                }
            }


            return result;
        }

        private HashSet<GridPosition> FloodFill(GridPosition startPos, BoardModel board, HashSet<GridPosition> visited)
        {
            var type = board.GetBlockType(startPos);
            var region = new HashSet<GridPosition>();
            var queue = new Queue<GridPosition>();


            Enqueue(startPos);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                region.Add(cur);

                foreach (var direction in All)
                {
                    var next = cur + direction.ToOffset();
                    if (board.IsInBounds(next) && !visited.Contains(next) && board.GetBlockType(next) == type)
                    {
                        Enqueue(next);
                    }
                }
            }

            return region;

            void Enqueue(GridPosition p)
            {
                visited.Add(p);
                queue.Enqueue(p);
            }
        }

        private bool HasQualifingLine(HashSet<GridPosition> region) =>
            HasLine(region.GroupBy(p => p.Z), p => p.X)
            || HasLine(region.GroupBy(p => p.X), p => p.Z);

        private bool HasLine(IEnumerable<IGrouping<int, GridPosition>> groups, Func<GridPosition, int> axis) =>
            groups.Any(g => MaxConsecutiveRun(g.Select(axis)) >= MIN_LINE);

        private int MaxConsecutiveRun(IEnumerable<int> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            var max = 1;
            var run = 1;


            for (var i = 0; i < sorted.Count; i++)
            {
                run = sorted[i] == sorted[i - 1] + 1 ? run + 1 : 1;
                if (run > max)
                {
                    run = max;
                }
            }

            return max;
        }
    }
}