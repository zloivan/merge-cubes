using System;
using MergeCubes.Config;
using UnityEngine;

namespace MergeCubes.Game.Balloons
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BalloonView : MonoBehaviour
    {
        private float _amplitude;
        private float _baseY;

        private float _direction;
        private float _frequency;
        private float _halfW;

        private Action<BalloonView> _onExited;
        private float _speed;
        private float _spriteHalfW;
        private SpriteRenderer _spriteRenderer;
        private float _time;

        private void Awake() =>
            _spriteRenderer = GetComponent<SpriteRenderer>();

        private void Update()
        {
            _time += Time.deltaTime;

            var pos = transform.position;
            pos.x += _direction * _speed * Time.deltaTime;
            pos.y = _baseY + _amplitude * Mathf.Sin(_frequency * _time);

            transform.position = pos;

            if (IsOffScreen())
                _onExited?.Invoke(this);
        }

        public void Initialize(
            BalloonTypeConfigSO type,
            float direction,
            float speed,
            float amplitude,
            float frequency,
            float baseY,
            float halfW,
            float spriteHalfW,
            Action<BalloonView> onBalloonExited)
        {
            _spriteRenderer.sprite = type.Sprite;
            _spriteRenderer.sortingOrder = type.SortingOrder;

            _direction = direction;
            _speed = speed;
            _amplitude = amplitude;
            _frequency = frequency;
            _baseY = baseY;
            _halfW = halfW;
            _spriteHalfW = spriteHalfW;
            _onExited = onBalloonExited;
            _time = 0f;
        }

        public void SelfDestroy()
        {
            Destroy(gameObject);
        }

        private bool IsOffScreen()
        {
            var x = transform.position.x;
            //if position of left or right border of balloon is behind distance to screen border
            return (_direction > 0f && x - _spriteHalfW > _halfW)
                   || (_direction < 0f && x + _spriteHalfW < -_halfW);
        }
    }
}