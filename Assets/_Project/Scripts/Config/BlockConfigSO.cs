using IKhom.SoundSystem.Runtime.data;
using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Block Config", menuName = "MergeCubes/Block Config", order = 0)]
    public class BlockConfigSO : ScriptableObject
    {
        public Sprite Sprite;
        public AnimationClip IdleClip;
        public AnimationClip DestroyClip;
        public SoundData DestroySound;
    }
}