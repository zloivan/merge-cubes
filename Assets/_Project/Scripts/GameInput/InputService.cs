using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Core.Grid;
using MergeCubes.Events;
using MergeCubes.Game.Blocks;
using MergeCubes.Game.Board;
using UnityEngine;
using VContainer;

namespace MergeCubes.GameInput
{
    /// <summary>
    /// Detects swipe gesture on board. Translates screen input → SwipeInputEvent. No knowledge of game rules.
    /// </summary>
    public class InputService : MonoBehaviour
    {
        private NormalizationController _normalizationController;
        private GameConfigSO _gameConfig;
        private Camera _camera;

        [SerializeField] private bool _isDebug;

        private Vector2 _dragStart;
        private GridPosition _fromPosition;
        private bool _isDragging;
        private Vector2 _debugCurrentPos;

        [Inject]
        public void Construct(NormalizationController normalizationController, GameConfigSO gameConfig)
        {
            _normalizationController = normalizationController;
            _gameConfig = gameConfig;
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_normalizationController.GetIsNormalizing())
            {
                return;
            }

            if (Input.touchCount > 0)
            {
                HandleTouch(Input.GetTouch(0));
            }
            else
            {
                HandleMouse();
            }
        }

        private void HandleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    PointerDown(touch.position);
                    break;
                case TouchPhase.Ended when _isDragging:
                    PointerUp(touch.position);
                    break;
                case TouchPhase.Canceled:
                    _isDragging = false;
                    break;
            }
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointerDown(Input.mousePosition);
            }
            else
            {
                if (Input.GetMouseButtonUp(0) && _isDragging)
                {
                    PointerUp(Input.mousePosition);
                }
            }
        }

        private void PointerDown(Vector2 screenPosition)
        {
            var worldPos = ScreenToWorld2D(screenPosition);

            var hit = Physics2D.OverlapPoint(worldPos, _gameConfig.BlockLayer);

            if (!hit || !hit.TryGetComponent<BlockView>(out var block))
                return;

            _fromPosition = block.GetGridPosition();
            _dragStart = screenPosition;
            _isDragging = true;
        }

        private void PointerUp(Vector2 screenPosition)
        {
            _isDragging = false;

            var delta = screenPosition - _dragStart;

            if (delta.magnitude < _gameConfig.MinSwipeDistance)
                return;

            var dir = ToDirection(delta);

            EventBus<SwipeInputEvent>.Raise(new SwipeInputEvent(_fromPosition, dir));
        }

        private Vector2 ScreenToWorld2D(Vector2 screenPosition)
        {
            var worldPos = _camera.ScreenToWorldPoint(screenPosition);
            return new Vector2(worldPos.x, worldPos.y);
        }

        private Direction ToDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? Direction.Right : Direction.Left;
            }

            return delta.y > 0 ? Direction.Up : Direction.Down;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_isDebug)
                return;

            Gizmos.color = _isDragging ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(new Vector3(_dragStart.x, _dragStart.y, 0f), 0.15f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(_debugCurrentPos.x, _debugCurrentPos.y, 0f), 0.08f);

            if (_isDragging)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(
                    new Vector3(_dragStart.x, _dragStart.y, 0f),
                    new Vector3(_debugCurrentPos.x, _debugCurrentPos.y, 0f)
                );
            }

            UnityEditor.Handles.Label(
                new Vector3(_debugCurrentPos.x, _debugCurrentPos.y + 0.3f, 0f),
                _isDragging
                    ? $"DRAGGING  from: {_fromPosition}"
                    : "IDLE"
            );
        }
#endif
    }
}