using System;

namespace MergeCubes.Saving
{
    [Serializable]
    public class SaveData
    {
        public int LevelIndex;
        public int Width;
        public int Height;
        public int[] Blocks;
        public int SaveVersion;
    }
}