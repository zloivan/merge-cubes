using Cysharp.Threading.Tasks;
using MergeCubes.Config;
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
        
        [Inject]
        public void Construct(LevelRepository repository)
        {
            _repository = repository;
        }

        private void Update()
        {
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                watterBlockView.Initialize(_watterConfig);
                fireBlockView.Initialize(_fireConfig);
            }
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                watterBlockView.SelfDestroyAnimatedAsync(_gameConfigSO.BlockDestroyDuration).Forget();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                fireBlockView.SelfDestroyAnimatedAsync(_gameConfigSO.BlockDestroyDuration).Forget();
            }
        }
    }
}