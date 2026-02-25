using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MergeCubes.Config;
using MergeCubes.Core.Grid;
using UnityEngine;

namespace MergeCubes.Game.Blocks
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlockView : MonoBehaviour
    {
        private const string IDLE_ANIMATION_KEY = "Idle";
        private const string DESTROY_ANIMATION_KEY = "Destroy";
        private static readonly int DestroyAnimationHash = Animator.StringToHash(DESTROY_ANIMATION_KEY);


        private GridPosition _gridPosition;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider2D;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void OnDestroy() =>
            transform.DOKill();

        public void Initialize(BlockConfigSO blockConfig)
        {
            _spriteRenderer.sprite = blockConfig.Sprite;

            var animatorOverrides = new AnimatorOverrideController(_animator.runtimeAnimatorController)
            {
                [IDLE_ANIMATION_KEY] = blockConfig.IdleClip,
                [DESTROY_ANIMATION_KEY] = blockConfig.DestroyClip,
            };

            _animator.runtimeAnimatorController = animatorOverrides;
        }

        public void SelfDestroy() =>
            Destroy(gameObject);

        public GridPosition GetGridPosition() =>
            _gridPosition;

        public void SetGridPosition(GridPosition gridPosition) =>
            _gridPosition = gridPosition;

        public async UniTask MoveToAsync(Vector3 position, float duration,
            Ease ease) =>
            await transform.DOMove(position, duration, true).SetEase(ease).ToUniTask();

        public async UniTask SelfDestroyAnimatedAsync(float duration)
        {
            SetInteractable(false);
            _animator.SetTrigger(DestroyAnimationHash);
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            SelfDestroy();
        }

        private void SetInteractable(bool isEnabled) =>
            _boxCollider2D.enabled = isEnabled;
    }
}