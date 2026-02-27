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
        [SerializeField] private BlockView watterBlockView;
        [SerializeField] private BlockView fireBlockView;

        [FormerlySerializedAs("_blockConfigSO")] [SerializeField]
        private BlockConfigSO _watterConfig;

        [FormerlySerializedAs("_blockConfigSO1")] [SerializeField]
        private BlockConfigSO _fireConfig;

        [SerializeField] private GameConfigSO _gameConfigSO;
        private ILevelRepository _repository;


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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                watterBlockView.Initialize(_watterConfig, 3);
                fireBlockView.Initialize(_fireConfig, 3);
            }

            if (Input.GetKeyDown(KeyCode.W))
                watterBlockView.SelfDestroyAnimatedAsync(_gameConfigSO.BlockDestroyDuration).Forget();
            if (Input.GetKeyDown(KeyCode.F))
                fireBlockView.SelfDestroyAnimatedAsync(_gameConfigSO.BlockDestroyDuration).Forget();

            if (Input.GetKeyDown(KeyCode.S)) EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
            if (Input.GetKeyDown(KeyCode.L)) EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent());
        }

        [Inject]
        public void Construct(ILevelRepository repository)
        {
            _repository = repository;
        }

        [ContextMenu("Trigget Level Complete")]
        private void TriggerLevelWon()
        {
            EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
        }
    }
}