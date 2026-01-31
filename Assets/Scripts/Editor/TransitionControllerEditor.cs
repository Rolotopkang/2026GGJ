using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransitionController))]
public class TransitionControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("进入 (ToBlack)", GUILayout.Height(28f)))
        {
            TransitionController.Inst.TransitionToBlack();
        }
        if (GUILayout.Button("退出 (FromBlack)", GUILayout.Height(28f)))
        {
            TransitionController.Inst.TransitionFromBlack();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("仅可在 Play 模式下测试", MessageType.Info);
        }
    }
}
