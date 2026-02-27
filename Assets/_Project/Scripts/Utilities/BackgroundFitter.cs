using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Events;
using UnityEngine;
using VContainer;

namespace MergeCubes.Utilities
{
    [DefaultExecutionOrder(-10)]
    public class BackgroundFitter : MonoBehaviour
    {
        [SerializeField] private Transform _background;
        [SerializeField] private float _horizontalMargin = 1.5f; // <-- добавить
        [SerializeField] private float _verticalMargin = 1.5f;   // 
        private GameConfigSO _gameConfig;
        private Vector2 _nativeSize;
        private EventBinding<LevelLoadedEvent> _onLevelLoaded;

        [Inject]
        public void Construct(GameConfigSO gameConfig) =>
            _gameConfig = gameConfig;

        private void Awake()
        {
            var sr = _background.GetComponent<SpriteRenderer>();
            // Native size at scale 1,1,1
            _nativeSize = sr.sprite.bounds.size;

            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(OnLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);
        }

        private void OnDestroy() =>
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);

        private void OnLevelLoaded(LevelLoadedEvent e)
        {
            var gridWorldWidth = e.Level.Width * _gameConfig.CellSize;
            var gridWorldHeight = e.Level.Height * _gameConfig.CellSize;

            // Required background size to fit grid + equal margins on each side
            var targetWidth  = gridWorldWidth  + 2f * _horizontalMargin;
            var targetHeight = gridWorldHeight + 2f * _verticalMargin;

            // Proportional scale — take the larger axis so background always covers both
            var scaleX = targetWidth / _nativeSize.x;
            var scaleY = targetHeight / _nativeSize.y;
            var uniformScale = Mathf.Max(scaleX, scaleY);

            _background.localScale = Vector3.one * uniformScale;
        }
    }
}