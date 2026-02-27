using System;
using System.Collections.Generic;
using UnityEngine;

namespace MergeCubes.Core.Grid
{
    public class GridSystemVertical<T> : GridSystemBase<T>
    {
        private readonly Vector3 _origin;
        
        public GridSystemVertical(int width, int height, Func<GridSystemBase<T>, GridPosition, T> factory,
            float cellSize = 1f, Vector3 origin = default) : base(width, height, cellSize)
        {
            _origin = origin;
            
            for (var x = 0; x < GetWidth(); x++)
            {
                for (var y = 0; y < GetHeight(); y++)
                {
                    var gridPosition = new GridPosition(x, y);
                    _gridObjects[x, y] = factory(this, gridPosition);
                }
            }
        }

        public override Vector3 GetWorldPosition(GridPosition gridPos) =>
            _origin + new Vector3(gridPos.X, gridPos.Z, 0f) * GetCellSize();

        public override GridPosition GetGridPosition(Vector3 worldPosition) =>
            new(
                Mathf.RoundToInt((worldPosition.x - _origin.x) / GetCellSize()),
                Mathf.RoundToInt((worldPosition.y - _origin.y) / GetCellSize())
            );

        public override List<GridPosition> GetNeighbors(GridPosition pos) =>
            new()
            {
                pos + new GridPosition(-1, 0), //left
                pos + new GridPosition(+1, 0), //right
                pos + new GridPosition(0, +1), //top
                pos + new GridPosition(0, -1), //bot
            };
    }
}