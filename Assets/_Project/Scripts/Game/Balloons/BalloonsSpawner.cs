using MergeCubes.Config;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace MergeCubes.Game.Balloons
{
    //TODO:
    //Right now balloons are constructed from as ingle config by random,
    //but assets have different size. So blue is smaller it means that it should go behind the orange one
    //and probably move slower. This might be a good idea to cunfigure each type of balloon
    //by individual config. So that in config we could have 2 different types with individual settings
    
    public class BalloonsSpawner : MonoBehaviour
    {
        private BalloonConfigSO _balloonConfig;
        private Camera _camera;

        [SerializeField] private BalloonView _balloonPrefab;

        private int _activeCount;
        private float _spriteHalfH;
        private float _spriteHalfW;

        [Inject]
        public void Construct(GameConfigSO balloonConfig, Camera cam)
        {
            _balloonConfig = balloonConfig.BalloonConfig;
            _camera = cam;
        }

        private void Start()
        {
            var bounds = _balloonPrefab.GetComponent<SpriteRenderer>().bounds;

            _spriteHalfW = bounds.extents.x;
            _spriteHalfH = bounds.extents.y;

            for (var i = 0; i < _balloonConfig.MaxCount; i++)
                SpawnBalloon();
        }

        private void SpawnBalloon()
        {
            //distance to top border of screen from center
            var halfH = _camera.orthographicSize;
            //distance to right border of screen from center
            var halfW = halfH * ((float)Screen.width / Screen.height);

            var amplitude = Random.Range(_balloonConfig.AmplitudeMin, _balloonConfig.AmplitudeMax);
            var frequency = Random.Range(_balloonConfig.FrequencyMin, _balloonConfig.FrequencyMax);
            var speed = Random.Range(_balloonConfig.SpeedMin, _balloonConfig.SpeedMax);
            var sprite = _balloonConfig.BalloonSprites[Random.Range(0, _balloonConfig.BalloonSprites.Length)];

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

            balloon.Initialize(sprite, direction, speed, amplitude, frequency, baseY, halfW, _spriteHalfW,
                OnBalloonExited);
        }

        private void OnBalloonExited(BalloonView balloon)
        {
            balloon.SelfDestroy();
            SpawnBalloon();
        }
    }
}