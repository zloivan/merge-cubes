using System.Collections.Generic;
using UnityEngine;

namespace MergeCubes.Core.Grid
{
    public class BoardGrid : MonoBehaviour
    {
        private const float SLOT_SIZE = 2f;
        [SerializeField] private int _width;
        [SerializeField] private int _height;

        [SerializeField] private GameObject _gridDebug;
        [SerializeField] private bool _enableDebug;


        private GridSystemBase<GridObject> _gridSystem;

        private void Awake()
        {
            _gridSystem = new GridSystemVertical<GridObject>(
                _width,
                _height,
                (_, pos) => new GridObject(pos),
                SLOT_SIZE);

            if (_enableDebug)
                _gridSystem.CreateDebugObjects(_gridDebug.transform, transform);
        }

        public int GetWidth() =>
            _gridSystem.GetWidth();

        public int GetHeight() =>
            _gridSystem.GetHeight();

        public Vector3 GetWorldPosition(GridPosition targetGridPosition) =>
            _gridSystem.GetWorldPosition(targetGridPosition);

        public bool IsValidGridPosition(GridPosition gridPosition) =>
            _gridSystem.IsValidGridPosition(gridPosition);

        public GridPosition GetGridPosition(Vector3 worldPointerPosition) =>
            _gridSystem.GetGridPosition(worldPointerPosition);

        public List<GridPosition> GetAllGridPositions()
        {
            var result = new List<GridPosition>();
            for (var x = 0; x < _gridSystem.GetWidth(); x++)
            {
                for (var z = 0; z < _gridSystem.GetHeight(); z++)
                {
                    result.Add(new GridPosition(x, z));
                }
            }

            return result;
        }

        public bool IsSlotEmpty(GridPosition gridPos) =>
            _gridSystem.GetGridObject(gridPos).Get() == null;

        public List<GridPosition> GetNeighbors(GridPosition target) =>
            _gridSystem.GetNeighbors(target);
        
    }
}