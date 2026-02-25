using Cinemachine;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Events;
using UnityEngine;
using VContainer;

namespace MergeCubes.Utilities
{
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Transform _cameraFollowTarget;
        private EventBinding<LevelLoadedEvent> _onLevelLoaded;
        private GameConfigSO _config;

        private void Start()
        {
            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(HandleLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);
        }

        private void OnDestroy() =>
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);

        [Inject]
        public void Construct(GameConfigSO config) =>
            _config = config;

        private void HandleLevelLoaded(LevelLoadedEvent e)
        {
            var gridWorldWidth = e.Level.Width * _config.CellSize;
            var gridWorldHeight = e.Level.Height * _config.CellSize;


            var orthoSize = gridWorldHeight / 2f + _config.CameraVerticalPadding;
            var requiredHalfWidth = gridWorldWidth / 2 + _config.CameraHorizontalPadding;

            var cameraHalfWidth = orthoSize * Screen.width / Screen.height;
            if (requiredHalfWidth > cameraHalfWidth)
            {
                orthoSize = requiredHalfWidth * Screen.height / Screen.width;
            }

            _virtualCamera.m_Lens.OrthographicSize = orthoSize;

            var gridCenter = new Vector3(
                (gridWorldWidth - _config.CellSize) / 2,
                (gridWorldHeight - _config.CellSize) / 2,
                0);

            _cameraFollowTarget.position = gridCenter;
        }
    }
}