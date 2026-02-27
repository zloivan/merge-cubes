using MergeCubes.Config;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace MergeCubes.Game.Balloons
{
    public class BalloonsSpawner : MonoBehaviour
    {
        [SerializeField] private BalloonView _balloonPrefab;

        private int _activeCount;
        private BalloonConfigSO _balloonConfig;
        private Camera _camera;
        private float _spriteHalfH;
        private float _spriteHalfW;

        private void Start()
        {
            var bounds = _balloonPrefab.GetComponent<SpriteRenderer>().bounds;

            _spriteHalfW = bounds.extents.x;
            _spriteHalfH = bounds.extents.y;

            for (var i = 0; i < _balloonConfig.MaxCount; i++)
                SpawnBalloon();
        }

        [Inject]
        public void Construct(GameConfigSO balloonConfig, Camera cam)
        {
            _balloonConfig = balloonConfig.BalloonConfig;
            _camera = cam;
        }

        private void SpawnBalloon()
        {
            //distance to top border of screen from center
            var halfH = _camera.orthographicSize;
            //distance to right border of screen from center
            var halfW = halfH * ((float)Screen.width / Screen.height);

            var type = _balloonConfig.Types[Random.Range(0, _balloonConfig.Types.Length)];

            var amplitude = Random.Range(type.AmplitudeMin, type.AmplitudeMax);
            var frequency = Random.Range(type.FrequencyMin, type.FrequencyMax);
            var speed = Random.Range(type.SpeedMin, type.SpeedMax);

            //add margin to top position based on balloon size, so that it didn't go out of top screen
            var verticalMargin = _spriteHalfH + amplitude;
            var baseY = Random.Range(
                -halfH + verticalMargin + _balloonConfig.BottomMargin,
                halfH - verticalMargin);

            var fromLeft = Random.value > 0.5f;
            //hide balloon spawn point behind left or right border
            var startX = fromLeft ? -halfW - _spriteHalfW : halfW + _spriteHalfW;
            var direction = fromLeft ? 1f : -1f;

            var balloon = Instantiate(
                _balloonPrefab,
                new Vector3(startX, baseY, 0.1f),
                Quaternion.identity,
                transform);

            balloon.Initialize(type, direction, speed, amplitude, frequency, baseY, halfW, _spriteHalfW,
                OnBalloonExited);
        }

        private void OnBalloonExited(BalloonView balloon)
        {
            balloon.SelfDestroy();
            SpawnBalloon();
        }
    }
}