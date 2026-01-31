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

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Start Game", GUILayout.Height(28f)))
        {
            var manager = (GameLoopManager)target;
            if (Application.isPlaying)
                manager.TestStartGame();
        }
        if (GUILayout.Button("Generate Animals (测试)", GUILayout.Height(28f)))
        {
            var manager = (GameLoopManager)target;
            if (Application.isPlaying)
                manager.GenerateAnimals();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("仅可在 Play 模式下测试", MessageType.Info);
    }
}
