using System;
using Cysharp.Threading.Tasks;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Config;
using MergeCubes.Events;
using MergeCubes.Game.Blocks;
using MergeCubes.Game.Level;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace MergeCubes.Test
{
    public class Test : MonoBehaviour
    {
        private LevelRepository _repository;
        [SerializeField] private BlockView watterBlockView;
        [SerializeField] private BlockView fireBlockView;
        [FormerlySerializedAs("_blockConfigSO")] [SerializeField] private BlockConfigSO _watterConfig;
        [FormerlySerializedAs("_blockConfigSO1")] [SerializeField] private BlockConfigSO _fireConfig;
        [SerializeField] private GameConfigSO _gameConfigSO;


        private void Awake()
        {
            EventBus<RestartRequestedEvent>.Register(new EventBinding<RestartRequestedEvent>(() =>
            {
                Debug.Log("Restart Requested");
            }));
            
            EventBus<NextLevelRequestedEvent>.Register(new EventBinding<NextLevelRequestedEvent>(() =>
            {
                Debug.Log("Next level Requested");
            }));
        }

        [Inject]
        public void Construct(LevelRepository repository)
        {
            _repository = repository;
        }

        private void Update()
        {
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                watterBlockView.Initialize(_watterConfig, 3);
                fireBlockView.Initialize(_fireConfig, 3);
            }
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                watterBlockView.SelfDestroyAnimatedAsync(_gameConfigSO.BlockDestroyDuration).Forget();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                fireBlockView.SelfDestroyAnimatedAsync(_gameConfigSO.BlockDestroyDuration).Forget();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent());
            }
        }

        [ContextMenu("Trigget Level Complete")]
        private void TriggerLevelWon()
        {
            EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
        }
    }
}