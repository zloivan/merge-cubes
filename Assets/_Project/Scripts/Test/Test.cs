using MergeCubes.Core.Grid;
using MergeCubes.Game;
using UnityEngine;
using VContainer;

namespace MergeCubes
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private BoardGrid _boardGrid;
        private LevelRepository _repository;
        
        [Inject]
        public void Construct(LevelRepository repository)
        {
            _repository = repository;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(_boardGrid.GetGridPosition(PointerToWorld.GetPointerPositionInWorld()));
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log(_repository);
            }
        }
    }
}