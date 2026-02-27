using System;
using DG.Tweening;
using MergeCubes.Game.Board;
using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Game Config", menuName = "MergeCubes/Game Config", order = 0)]
    public class GameConfigSO : ScriptableObject
    {
        [Header("References")]
        public AudioConfigSO AudioConfig;
        public BlockConfigSO[] BlockConfigs;
        public BalloonConfigSO BalloonConfig;

        [Header("Levels")]
        public LevelDataSO[] Levels;

        [Header("Grid")]
        [Tooltip("World-space size of one cell")]
        [Min(0.1f)] public float CellSize = 1f;

        [Header("Block Animations")]
        [Tooltip("Duration of fall tween (seconds)")]
        [Range(0.05f, 1f)] public float BlockFallDuration = 0.2f;
        [Tooltip("Duration of swap tween (seconds)")]
        [Range(0.05f, 1f)] public float BlockSwapDuration = 0.15f;
        [Tooltip("Duration of destroy animation (seconds)")]
        [Range(0.05f, 1f)] public float BlockDestroyDuration = 0.3f;
        public Ease BlockSwapEase   = Ease.OutBack;
        public Ease BlockFallEase   = Ease.InQuad;

        [Header("Normalization Delays")]
        [Tooltip("Pause before blocks fall event is raised (lets model settle)")]
        [Range(0f, 0.5f)] public float BlockFallDelay = 0.05f;
        [Tooltip("Pause before destroy event is raised")]
        [Range(0f, 0.5f)] public float BlockDestroyDelay = 0.05f;
        [Tooltip("Stagger between individual block destroy animations")]
        [Range(0f, 0.2f)] public float DestroyStaggerDelay = 0.06f;

        [Header("Input")]
        [Tooltip("Minimum screen pixels for a swipe to register")]
        [Range(10f, 200f)] public float MinSwipeDistance = 50f;
        public LayerMask BlockLayer;

        [Header("Win")]
        [Tooltip("Delay before auto-advancing to next level after win")]
        [Range(0.5f, 10f)] public float WinDelay = 5f;

        [Header("Camera Shake")]
        [Range(0f, 2f)]  public float ShakeForce = 0.3f;
        [Range(0f, 1f)]  public float ShakeDelay = 0.15f;

        public BlockConfigSO GetBlockConfig(BlockType blockType) =>
            Array.Find(BlockConfigs, c => c.BlockType == blockType);
    }
}