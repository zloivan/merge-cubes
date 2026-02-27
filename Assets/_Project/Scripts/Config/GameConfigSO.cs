using System;
using DG.Tweening;
using MergeCubes.Game.Blocks;
using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Game Config", menuName = "MergeCubes/Game Config", order = 0)]
    public class GameConfigSO : ScriptableObject
    {
        public AudioConfigSO AudioConfig;
        public BlockConfigSO[] BlockConfigs;
        public BalloonConfigSO BalloonConfig;
        public LevelDataSO[] Levels;
        public float CellSize;
        public float CameraVerticalPadding;
        public float CameraHorizontalPadding;
        public float BlockFallDuration;
        public float BlockSwapDuration;
        public float BlockDestroyDuration;
        public LayerMask BlockLayer;
        public float MinSwipeDistance;
        public Ease BlockSwapEase;
        public Ease BlockFallEase;
        public float WinDelay = 5f;
        public float BlockDestroyDelay;
        public float BlockFallDelay;
        public float DestroyStaggerDelay = 0.06f;
        public float ShakeForce = 0.3f;
        public float ShakeDelay = 0.15f;

        public BlockConfigSO GetBlockConfig(BlockType blockType) =>
            Array.Find(BlockConfigs, c => c.BlockType == blockType);
    }
}