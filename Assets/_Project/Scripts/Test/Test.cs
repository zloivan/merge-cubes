using System;
using MergeCubes.Core.Grid;
using UnityEngine;

namespace MergeCubes
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private BoardGrid _boardGrid;


        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(_boardGrid.GetGridPosition(PointerToWorld.GetPointerPositionInWorld()));
            }
        }
    }
}