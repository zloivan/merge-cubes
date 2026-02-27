using MergeCubes.Game.Board;
using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Block Config", menuName = "MergeCubes/Block Config", order = 0)]
    public class BlockConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public BlockType BlockType;

        [Header("Visuals")]
        public Sprite Sprite;

        [Header("Animations")]
        public AnimationClip IdleClip;
        public AnimationClip DestroyClip;
    }
}