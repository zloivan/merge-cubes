using UnityEngine;

namespace MergeCubes.Config
{
    [CreateAssetMenu(fileName = "New BalloonConfig", menuName = "MergeCubes/Balloon Config", order = 0)]
    public class BalloonConfigSO : ScriptableObject
    {
        public BalloonTypeConfigSO[] Types;
        public float BottomMargin;
        public int MaxCount;
    }
}