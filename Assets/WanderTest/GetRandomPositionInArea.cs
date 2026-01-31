using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace WanderingCubes.BehaviorDesigner
{
    /// <summary>
    /// 在指定区域内随机一个点，写入 SharedVector3。
    /// 用于漫游：目标位置 = center + [-halfExtents, +halfExtents] 的随机偏移。
    /// </summary>
    [TaskCategory("WanderingCubes")]
    [TaskDescription("在指定盒体区域内生成随机位置并存储")]
    public class GetRandomPositionInArea : Action
    {
        [UnityEngine.Tooltip("区域中心（世界坐标）")]
        public SharedVector3 center = Vector3.zero;
        [UnityEngine.Tooltip("区域半尺寸（X/Z 为水平范围，Y 可设为 0 表示只在平面内随机）")]
        public SharedVector3 halfExtents = new Vector3(5f, 0f, 5f);
        [UnityEngine.Tooltip("存储结果的位置（可绑定行为树变量，如 TargetPosition）")]
        [RequiredField]
        public SharedVector3 storeResult;

        public override TaskStatus OnUpdate()
        {
            Vector3 c = center.Value;
            Vector3 he = halfExtents.Value;
            Vector3 randomOffset = new Vector3(
                Random.Range(-he.x, he.x),
                Random.Range(-he.y, he.y),
                Random.Range(-he.z, he.z)
            );
            storeResult.Value = c + randomOffset;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            center = Vector3.zero;
            halfExtents = new Vector3(5f, 0f, 5f);
            storeResult = Vector3.zero;
        }
    }
}
