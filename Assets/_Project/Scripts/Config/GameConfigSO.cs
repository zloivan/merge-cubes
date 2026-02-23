using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Game Config", menuName = "MergeCubes/Game Config", order = 0)]
    public class GameConfigSO : ScriptableObject
    {
        public BlockConfigSO[] BlockConfigs;
        public BalloonConfigSO BalloonConfig;
        public LevelDataSO[] Levels;
        public float CellSize;
        public float CameraVerticalPadding;
        public float CameraHorizontalPadding;
        public float BlockFallDuration;
        public float BlockMoveDuration;
        public float BlockDestroyDuration;
    }
}