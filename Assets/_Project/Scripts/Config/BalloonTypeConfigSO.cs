using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Balloon Config", menuName = "MergeCubes/Balloon Type Config", order = 0)]
    public class BalloonTypeConfigSO : ScriptableObject
    {
        public Sprite Sprite;
        public int SortingOrder;
        public float SpeedMin;
        public float SpeedMax;
        public float AmplitudeMin;
        public float AmplitudeMax;
        public float FrequencyMin;
        public float FrequencyMax;
    }
}