using System;
using Cinemachine;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Events;
using UnityEngine;
using VContainer;

namespace MergeCubes.Cameras
{
    public class CameraShakeController : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        private GameConfigSO _gameConfig;
        private EventBinding<BlocksDestroyedEvent> _onBlocksDestroyed;

        private void Awake()
        {
            _onBlocksDestroyed = new EventBinding<BlocksDestroyedEvent>(HandleBlocksDestroyed);
            EventBus<BlocksDestroyedEvent>.Register(_onBlocksDestroyed);
        }

        private void OnDestroy() =>
            EventBus<BlocksDestroyedEvent>.Deregister(_onBlocksDestroyed);

        [Inject]
        public void Construct(GameConfigSO gameConfig) =>
            _gameConfig = gameConfig;

        private void HandleBlocksDestroyed(BlocksDestroyedEvent _) =>
            ShakeDelayedAsync().Forget();

        private async UniTask ShakeDelayedAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_gameConfig.ShakeDelay));
            _impulseSource.GenerateImpulse(_gameConfig.ShakeForce);
        }
    }
}