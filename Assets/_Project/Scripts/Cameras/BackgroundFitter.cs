using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Events;
using UnityEngine;
using VContainer;

namespace MergeCubes.Cameras
{
    [DefaultExecutionOrder(-10)]
    public class BackgroundFitter : MonoBehaviour
    {
        [SerializeField] private Transform _background;
        [SerializeField] private float _horizontalMargin = 1.5f;
        [SerializeField] private float _verticalMargin = 1.5f;
        private GameConfigSO _gameConfig;
        private Vector2 _nativeSize;
        private EventBinding<LevelLoadedEvent> _onLevelLoaded;

        private void Awake()
        {
            var sr = _background.GetComponent<SpriteRenderer>();
            _nativeSize = sr.sprite.bounds.size;

            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(OnLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);
        }

        private void OnDestroy() =>
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);

        [Inject]
        public void Construct(GameConfigSO gameConfig) =>
            _gameConfig = gameConfig;

        private void OnLevelLoaded(LevelLoadedEvent e)
        {
            var gridWorldWidth = e.Level.Width * _gameConfig.CellSize;
            var gridWorldHeight = e.Level.Height * _gameConfig.CellSize;

            var targetWidth = gridWorldWidth + 2f * _horizontalMargin;
            var targetHeight = gridWorldHeight + 2f * _verticalMargin;

            var scaleX = targetWidth / _nativeSize.x;
            var scaleY = targetHeight / _nativeSize.y;
            var uniformScale = Mathf.Max(1f, Mathf.Max(scaleX, scaleY));

            _background.localScale = Vector3.one * uniformScale;
        }
    }
}