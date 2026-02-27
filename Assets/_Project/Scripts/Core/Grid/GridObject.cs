using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MergeCubes.Core.Grid
{
    public class GridObject
    {
        private readonly GridPosition _gridPosition;
        private readonly List<GameObject> _unitList;

        public GridObject(GridPosition gridPosition)
        {
            _gridPosition = gridPosition;
            _unitList = new List<GameObject>(0);
        }

        public override string ToString() =>
            $"{_gridPosition}\n" + string.Join("\n", _unitList.Select(e => e.ToString()));

        public void Add(GameObject unit) =>
            _unitList.Add(unit);

        public List<GameObject> GetList() =>
            _unitList;

        public void Remove(GameObject unit)
        {
            if (!_unitList.Contains(unit))
                return;

            _unitList.Remove(unit);
        }

        public void ClearObjectList() =>
            _unitList.Clear();

        public bool HasAny() =>
            _unitList.Count > 0;

        public GameObject Get() =>
            HasAny() ? _unitList[0] : null;

        public void Set(GameObject unit)
        {
            _unitList.Clear();
            _unitList[0] = unit;
        }
    }
}