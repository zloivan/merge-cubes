using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MergeCubes.Config;
using MergeCubes.Core.Grid;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MergeCubes.Game.Blocks
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlockView : MonoBehaviour
    {
        private const string IDLE_STATE_KEY = "Idle";
        private const string IDLE_ANIMATION_KEY = "anim_BlockIdle";
        private const string DESTROY_ANIMATION_KEY = "anim_BlockDestroy";
        private static readonly int DestroyTriggerHash = Animator.StringToHash("Destroy");

        private Animator _animator;
        private int _boardWidth;
        private BoxCollider2D _boxCollider2D;


        private GridPosition _gridPosition;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void OnDestroy() =>
            transform.DOKill();

        public void Initialize(BlockConfigSO blockConfig, int boardWidth)
        {
            _spriteRenderer.sprite = blockConfig.Sprite;
            _boardWidth = boardWidth;
            _animator.Play(IDLE_STATE_KEY, 0, Random.Range(0f, 1f));

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

        public void SetGridPosition(GridPosition gridPosition)
        {
            _gridPosition = gridPosition;
            _spriteRenderer.sortingOrder = gridPosition.Z * _boardWidth + gridPosition.X;
        }

        public async UniTask MoveToAsync(Vector3 position, float duration,
            Ease ease) =>
            await transform.DOMove(position, duration).SetEase(ease).ToUniTask();

        public async UniTask SelfDestroyAnimatedAsync(float duration)
        {
            SetInteractable(false);
            _animator.SetTrigger(DestroyTriggerHash);
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            SelfDestroy();
        }

        private void SetInteractable(bool isEnabled) =>
            _boxCollider2D.enabled = isEnabled;
    }
}