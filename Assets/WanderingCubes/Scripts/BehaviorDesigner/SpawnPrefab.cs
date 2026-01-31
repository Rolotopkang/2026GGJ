using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace WanderingCubes.BehaviorDesigner
{
    /// <summary>
    /// 在行为主体的附近生成一个指定的预制体。
    /// 生成位置 = 主体当前位置 + 随机偏移。
    /// </summary>
    [TaskCategory("WanderingCubes")]
    [TaskDescription("在附近生成指定预制体")]
    public class SpawnPrefab : Action
    {
        [UnityEngine.Tooltip("要生成的预制体")]
        public GameObject prefab;
        [UnityEngine.Tooltip("生成偏移范围（相对于当前位置的随机偏移）")]
        public Vector2 spawnOffsetRange = new Vector2(2f, 2f);
        [UnityEngine.Tooltip("生成后是否自动激活")]
        public bool setActive = true;
        [UnityEngine.Tooltip("生成的父物体（为空则不设置父物体）")]
        public SharedGameObject parent;

        private Transform _transform;

        public override void OnStart()
        {
            _transform = GetDefaultGameObject(gameObject).transform;
        }

        public override TaskStatus OnUpdate()
        {
            if (_transform == null)
            {
                return TaskStatus.Failure;
            }

            if (prefab == null)
            {
                Debug.LogWarning("SpawnPrefab: 未指定预制体！");
                return TaskStatus.Failure;
            }

            // 计算生成位置
            Vector3 spawnPos = _transform.position + new Vector3(
                Random.Range(-spawnOffsetRange.x, spawnOffsetRange.x),
                Random.Range(-spawnOffsetRange.y, spawnOffsetRange.y),
                0f
            );

            // 生成预制体
            GameObject spawnedObj = UnityEngine.Object.Instantiate(prefab, spawnPos, Quaternion.identity);

            // 设置父物体
            if (parent.Value != null)
            {
                spawnedObj.transform.SetParent(parent.Value.transform);
            }

            // 设置激活状态
            if (setActive && !spawnedObj.activeSelf)
            {
                spawnedObj.SetActive(true);
            }

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            prefab = null;
            spawnOffsetRange = new Vector2(2f, 2f);
            setActive = true;
            parent = null;
        }
    }
}
