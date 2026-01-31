using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace WanderingCubes.BehaviorDesigner
{
    /// <summary>
    /// 每帧朝目标位置移动，到达后返回 Success，否则返回 Running。
    /// 使用 Unity 基础逻辑：Transform.position + Vector3.MoveTowards。
    /// </summary>
    [TaskCategory("WanderingCubes")]
    [TaskDescription("朝目标位置移动直到到达")]
    public class MoveTowardsTargetUntilReached : Action
    {
        [UnityEngine.Tooltip("目标位置（可绑定行为树变量 TargetPosition）")]
        [RequiredField]
        public SharedVector3 targetPosition;
        [UnityEngine.Tooltip("移动速度（单位/秒）")]
        public SharedFloat speed = 2f;
        [UnityEngine.Tooltip("视为到达的距离阈值")]
        public SharedFloat arrivalDistance = 0.2f;
        [UnityEngine.Tooltip("要移动的 GameObject，为空则使用挂载行为树的对象")]
        public SharedGameObject targetGameObject;

        private Transform _transform;

        public override void OnStart()
        {
            GameObject go = GetDefaultGameObject(targetGameObject.Value);
            _transform = go != null ? go.transform : null;
        }

        public override TaskStatus OnUpdate()
        {
            if (_transform == null)
                return TaskStatus.Failure;

            Vector3 current = _transform.position;
            Vector3 target = targetPosition.Value;
            float moveDelta = speed.Value * Time.deltaTime;
            _transform.position = Vector3.MoveTowards(current, target, moveDelta);

            float dist = Vector3.Distance(_transform.position, target);
            return dist <= arrivalDistance.Value ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnReset()
        {
            targetGameObject = null;
            targetPosition = Vector3.zero;
            speed = 2f;
            arrivalDistance = 0.2f;
        }
    }
}
