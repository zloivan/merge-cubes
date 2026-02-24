using IKhom.SoundSystem.Runtime.data;
using UnityEngine;

namespace MergeCubes.Config
{
    public enum BlockType
    {
        None = 0,
        Fire = 1,
        Water = 2,
    }
    
    [CreateAssetMenu(fileName = "New Block Config", menuName = "MergeCubes/Block Config", order = 0)]
    public class BlockConfigSO : ScriptableObject
    {
        public BlockType BlockType;
        public Sprite Sprite;
        public AnimationClip IdleClip;
        public AnimationClip DestroyClip;
        public SoundData DestroySound;
    }
}