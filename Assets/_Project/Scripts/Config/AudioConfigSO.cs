using IKhom.SoundSystem.Runtime.data;
using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Audio Config", menuName = "MergeCubes/Audio Config", order = 0)]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("Background")]
        public SoundData BackgroundMusic;
        
        [Header("Block")]
        public SoundData[] SwapSounds;
        public SoundData[] MoveSounds;
        public SoundData[] FallSounds;
        public SoundData[] DestroySounds;

        [Header("UI")]
        public SoundData[] ButtonClickSounds;

        [Header("Win")]
        public SoundData[] WinSounds;
    }
}