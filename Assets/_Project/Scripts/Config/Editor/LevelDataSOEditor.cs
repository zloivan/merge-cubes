using MergeCubes.Config;
using MergeCubes.Game.Blocks;
using UnityEditor;
using UnityEngine;

namespace MergeCubes.Editor
{
    [CustomEditor(typeof(LevelDataSO))]
    public class LevelDataSOEditor : UnityEditor.Editor
    {
        private static readonly Color FireColor = new Color(1f, 0.45f, 0.1f);
        private static readonly Color WaterColor = new Color(0.2f, 0.6f, 1f);
        private static readonly Color NoneColor = new Color(0.2f, 0.2f, 0.2f);

        public override void OnInspectorGUI()
        {
            var so = (LevelDataSO)target;
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            var newWidth = Mathf.Max(1, EditorGUILayout.IntField("Width", so.Width));
            var newHeight = Mathf.Max(1, EditorGUILayout.IntField("Height", so.Height));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(so, "Resize Grid");
                ResizeGrid(so, newWidth, newHeight);
                EditorUtility.SetDirty(so);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid (row 0 = bottom)", EditorStyles.boldLabel);
            DrawGrid(so);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid(LevelDataSO so)
        {
            var cellSize = GUILayout.Width(52);

            // Draw top → bottom visually, but row 0 is bottom in data
            for (var row = so.Height - 1; row >= 0; row--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"row {row}", GUILayout.Width(42));

                for (var col = 0; col < so.Width; col++)
                {
                    var index = row * so.Width + col;
                    var current = so.InitialBlocks[index];

                    GUI.backgroundColor = GetBlockColor(current);

                    if (GUILayout.Button(current.ToString(), cellSize))
                    {
                        Undo.RecordObject(so, "Edit Block");
                        so.InitialBlocks[index] = CycleType(current);
                        EditorUtility.SetDirty(so);
                    }
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }

        private static BlockType CycleType(BlockType current)
        {
            var values = (BlockType[])System.Enum.GetValues(typeof(BlockType));
            var nextIndex = (System.Array.IndexOf(values, current) + 1) % values.Length;
            return values[nextIndex];
        }

        private static Color GetBlockColor(BlockType type) => type switch
        {
            BlockType.None  => NoneColor,
            BlockType.Fire  => FireColor,
            BlockType.Water => WaterColor,
            _               => ColorFromHash(type.ToString())
        };
        
        private static Color ColorFromHash(string name)
        {
            var h = (float)(name.GetHashCode() & 0x7FFFFFFF) / int.MaxValue;
            return Color.HSVToRGB(h, 0.6f, 0.9f);
        }
        
        private static void ResizeGrid(LevelDataSO so, int newWidth, int newHeight)
        {
            var newBlocks = new BlockType[newWidth * newHeight];

            for (var row = 0; row < Mathf.Min(so.Height, newHeight); row++)
            for (var col = 0; col < Mathf.Min(so.Width, newWidth); col++)
                newBlocks[row * newWidth + col] = so.InitialBlocks[row * so.Width + col];

            so.Width = newWidth;
            so.Height = newHeight;
            so.InitialBlocks = newBlocks;
        }
    }
}