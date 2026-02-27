using MergeCubes.Game.Blocks;
using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Level Data", menuName = "MergeCubes/Level Data", order = 0)]
    public class LevelDataSO : ScriptableObject
    {
        public int Width;
        public int Height;
        public BlockType[] InitialBlocks;

        private void OnValidate()
        {
            if (InitialBlocks.Length != Width * Height)
            {
                Debug.LogWarning("Not all initial blocks are assigned inside config", this);
            }
        }
    }
}