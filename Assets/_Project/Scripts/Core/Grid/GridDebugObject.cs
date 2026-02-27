using UnityEngine;

namespace MergeCubes.Core.Grid
{
    public class GridDebugObject : MonoBehaviour
    {
        [SerializeField] private TextMesh _debugLabel;

        private object _gridObject;

        protected virtual void Update()
        {
            if (_gridObject != null)
                _debugLabel.text = _gridObject.ToString();
        }

        public virtual void SetGridObject(object gridObject) =>
            _gridObject = gridObject;
    }
}