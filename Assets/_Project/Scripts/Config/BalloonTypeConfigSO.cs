using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New Balloon Config", menuName = "MergeCubes/Balloon Type Config", order = 0)]
    public class BalloonTypeConfigSO : ScriptableObject
    {
        [Header("Visuals")]
        public Sprite Sprite;
        public int SortingOrder;

        [Header("Movement")]
        [Tooltip("Horizontal speed range (units/sec)")]
        [Min(0f)] public float SpeedMin     = 1f;
        [Min(0f)] public float SpeedMax     = 3f;

        [Tooltip("Vertical sine-wave amplitude range (world units)")]
        [Min(0f)] public float AmplitudeMin = 0.2f;
        [Min(0f)] public float AmplitudeMax = 0.8f;

        [Tooltip("Sine-wave frequency range (radians/sec)")]
        [Min(0f)] public float FrequencyMin = 0.5f;
        [Min(0f)] public float FrequencyMax = 2f;
    }
}