using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New BalloonConfig", menuName = "MergeCubes/Balloon Config", order = 0)]
    public class BalloonConfigSO : ScriptableObject
    {
        [Header("Types")]
        [Tooltip("Each entry is a distinct balloon variant with its own movement ranges")]
        public BalloonTypeConfigSO[] Types;

        [Header("Spawning")]
        [Tooltip("Maximum simultaneous balloons on screen")]
        [Range(1, 10)] public int MaxCount = 3;
        [Tooltip("Minimum Y offset from screen bottom so balloons don't spawn below HUD")]
        [Min(0f)] public float BottomMargin = 1f;
    }
}