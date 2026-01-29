using UnityEngine;
using UnityEditor;

public static class CreatePlayerMenuItem
{
    [MenuItem("GameObject/2D Object/玩家方形 (带移动脚本)", false, 10)]
    public static void CreatePlayerSquare()
    {
        // 创建白色方形贴图
        int size = 64;
        var tex = new Texture2D(size, size);
        var fill = Color.white;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, fill);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));

        var go = new GameObject("Player");
        Undo.RegisterCreatedObjectUndo(go, "Create Player");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;

        go.AddComponent<PlayerMovement>();

        // 放到场景视图中心附近
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one;

        Selection.activeGameObject = go;
    }
}
