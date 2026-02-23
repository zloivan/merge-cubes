using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New BalloonConfig", menuName = "MergeCubes/Balloon Config", order = 0)]
    public class BalloonConfigSO : ScriptableObject
    {
        public Sprite[] BalloonSprites;
        public float SpeedMin;
        public float SpeedMax;
        public float AmplitudeMin;
        public float AmplitudeMax;
        public float FrequencyMin;
        public float FrequencyMax;
        public int MaxCount;
    }
}