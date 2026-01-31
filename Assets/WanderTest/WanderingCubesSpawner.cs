using UnityEngine;
using BehaviorDesigner.Runtime;

namespace WanderingCubes
{
    /// <summary>
    /// 在场景中生成多个可漫游的方块，每个方块挂载 BehaviorTree，使用指定的 External Behavior Tree。
    /// 将本组件挂在场景中的空物体上，并指定行为树资源和数量即可。
    /// </summary>
    public class WanderingCubesSpawner : MonoBehaviour
    {
        [Header("行为树（需在 Behavior Designer 中创建并保存为 External Behavior Tree）")]
        [Tooltip("漫游逻辑：Repeater(repeatForever) -> Sequence -> GetRandomPositionInArea -> MoveTowardsTargetUntilReached -> Wait")]
        public ExternalBehaviorTree behaviorTree;

        [Header("生成设置")]
        [Tooltip("方块数量")]
        public int cubeCount = 12;
        [Tooltip("生成范围（中心为 Spawner 的 position，半尺寸）")]
        public Vector3 spawnHalfExtents = new Vector3(4f, 0f, 4f);
        [Tooltip("方块统一缩放")]
        public Vector3 cubeScale = Vector3.one;

        [Header("可选：方块材质（URP 下留空则自动用 URP Lit 生成）")]
        public Material cubeMaterial;

        private static Material s_urpFallbackMaterial;

        private void Start()
        {
            if (behaviorTree == null)
            {
                Debug.LogWarning("WanderingCubesSpawner: 未指定 External Behavior Tree，请先在 Inspector 里把行为树资源拖到 Behavior Tree 字段。");
                return;
            }

            Vector3 center = transform.position;
            for (int i = 0; i < cubeCount; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-spawnHalfExtents.x, spawnHalfExtents.x),
                    Random.Range(-spawnHalfExtents.y, spawnHalfExtents.y),
                    Random.Range(-spawnHalfExtents.z, spawnHalfExtents.z)
                );
                CreateWanderCube(pos, i);
            }

            Debug.Log($"WanderingCubesSpawner: 已生成 {cubeCount} 个方块。若仍看不到，请检查：1) 是否在运行 WanderingCubesScene；2) Game 视图是否选中该场景的 Camera；3) 若为 URP，是否已为方块指定/生成了可见材质。");
        }

        private void CreateWanderCube(Vector3 position, int index)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"WanderCube_{index + 1}";
            cube.transform.SetParent(transform);
            cube.transform.position = position;
            cube.transform.localScale = cubeScale;

            // URP 下 CreatePrimitive 的默认材质可能不显示，改为使用指定材质或 URP Lit
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (cubeMaterial != null)
                    renderer.sharedMaterial = cubeMaterial;
                else
                    renderer.sharedMaterial = GetOrCreateURPMaterial();
            }

            var tree = cube.AddComponent<BehaviorTree>();
            tree.ExternalBehavior = behaviorTree;
            tree.EnableBehavior();
        }

        private static Material GetOrCreateURPMaterial()
        {
            if (s_urpFallbackMaterial != null)
                return s_urpFallbackMaterial;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Legacy Shaders/Diffuse");
            if (shader != null)
            {
                s_urpFallbackMaterial = new Material(shader);
                s_urpFallbackMaterial.color = new Color(0.2f, 0.6f, 1f);
            }
            return s_urpFallbackMaterial;
        }
    }
}
