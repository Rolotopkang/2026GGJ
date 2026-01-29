using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 创建玩家预制体：方形精灵 + PlayerMovement 脚本，并设置好初始值。
/// 菜单：Assets -> Create -> 玩家预制体
/// </summary>
public static class CreatePlayerPrefab
{
    private const string PrefabPath = "Assets/Art/Player/Player.prefab";
    private const string SpriteAssetPath = "Assets/Art/Player/PlayerSquare.asset";

    [MenuItem("Assets/Create/玩家预制体", false, 0)]
    public static void CreatePrefab()
    {
        EnsureFolderExists();
        Sprite sprite = GetOrCreatePlayerSprite();
        if (sprite == null)
        {
            Debug.LogError("创建玩家方形贴图失败。");
            return;
        }

        GameObject go = BuildPlayerObject(sprite);
        if (go == null)
        {
            Debug.LogError("创建玩家物体失败。");
            return;
        }

        // 临时放入当前场景才能保存为预制体
        var scene = EditorSceneManager.GetActiveScene();
        bool createdNew = false;
        if (!scene.isLoaded)
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene = EditorSceneManager.GetActiveScene();
            createdNew = true;
        }
        go.transform.SetParent(null);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);

        GameObject prefabRoot = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);

        if (createdNew)
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        if (prefabRoot != null)
        {
            Selection.activeObject = prefabRoot;
            EditorGUIUtility.PingObject(prefabRoot);
            Debug.Log($"玩家预制体已创建：{PrefabPath}");
        }
        else
            Debug.LogError($"保存预制体失败：{PrefabPath}");
    }

    private static void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art/Player"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
                AssetDatabase.CreateFolder("Assets", "Art");
            AssetDatabase.CreateFolder("Assets/Art", "Player");
            AssetDatabase.Refresh();
        }
    }

    private static Sprite GetOrCreatePlayerSprite()
    {
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteAssetPath);
        if (existing != null)
            return existing;

        Object texObj = AssetDatabase.LoadAssetAtPath<Object>(SpriteAssetPath);
        if (texObj != null)
        {
            string path = AssetDatabase.GetAssetPath(texObj);
            Object[] sub = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < sub.Length; i++)
            {
                if (sub[i] is Sprite s)
                    return s;
            }
        }

        int size = 64;
        var tex = new Texture2D(size, size);
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        AssetDatabase.CreateAsset(tex, SpriteAssetPath);
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        sprite.name = "PlayerSquare";
        AssetDatabase.AddObjectToAsset(sprite, SpriteAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return sprite;
    }

    private static GameObject BuildPlayerObject(Sprite sprite)
    {
        var go = new GameObject("Player");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = 0;

        var movement = go.AddComponent<PlayerMovement>();
        movement.moveSpeed = 5f;
        movement.useRigidbody2D = false;

        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go;
    }
}
