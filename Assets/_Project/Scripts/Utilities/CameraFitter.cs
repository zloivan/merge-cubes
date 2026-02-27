using System;
using Cinemachine;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Events;
using UnityEngine;

namespace MergeCubes.Utilities
{
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private SpriteRenderer _background;

        private EventBinding<LevelLoadedEvent> _onLevelLoaded;

        private void Awake()
        {
            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(OnLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);
        }

        private void OnDestroy() =>
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);

        private void OnLevelLoaded()
        {
            var bgBounds = _background.bounds;
            var bgHalfHeight = bgBounds.extents.y;
            var bgAspect = bgBounds.size.x / bgBounds.size.y;
            var screenAspect = (float)Screen.width / Screen.height;

            var orthoSize = screenAspect < bgAspect
                ? bgHalfHeight
                : bgBounds.size.x / 2f / screenAspect;

            _virtualCamera.m_Lens.OrthographicSize = orthoSize;
        }
    }
}