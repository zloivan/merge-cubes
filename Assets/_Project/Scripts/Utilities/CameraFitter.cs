using Cinemachine;
using UnityEngine;

namespace MergeCubes.Utilities
{
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private SpriteRenderer _background;

        private void Start()
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