using UnityEngine;
using UnityEditor;
using Player;

[CustomEditor(typeof(GameLoopManager))]
public class GameLoopManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("Generate Animals (测试)", GUILayout.Height(28f)))
        {
            var manager = (GameLoopManager)target;
            if (Application.isPlaying)
            {
                manager.GenerateAnimals();
            }
            else
            {
                Debug.LogWarning("[GameLoopManager] 仅可在运行时测试，请先进入 Play 模式。");
            }
        }
    }
}
