using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace WanderingCubes.BehaviorDesigner
{
    /// <summary>
    /// 每帧朝目标位置移动（2D 版本），到达后返回 Success，否则返回 Running。
    /// 使用 Unity 基础逻辑：Transform.position + Vector2.MoveTowards。
    /// </summary>
    [TaskCategory("WanderingCubes")]
    [TaskDescription("朝目标位置移动直到到达（2D）")]
    public class MoveTowardsTargetUntilReached2D : Action
    {
        [UnityEngine.Tooltip("目标位置（可绑定行为树变量 TargetPosition）")]
        [RequiredField]
        public SharedVector2 targetPosition;
        [UnityEngine.Tooltip("移动速度（单位/秒）")]
        public SharedFloat speed = 2f;
        [UnityEngine.Tooltip("视为到达的距离阈值")]
        public SharedFloat arrivalDistance = 0.2f;
        [UnityEngine.Tooltip("要移动的 GameObject，为空则使用挂载行为树的对象")]
        public SharedGameObject targetGameObject;

        [Header("轨迹显示")]
        [UnityEngine.Tooltip("是否显示移动轨迹")]
        public SharedBool showTrail = false;
        [UnityEngine.Tooltip("轨迹线颜色")]
        public UnityEngine.Color trailColor = UnityEngine.Color.green;
        [UnityEngine.Tooltip("轨迹线宽度")]
        [UnityEngine.Range(0.01f, 0.5f)]
        public SharedFloat trailWidth = 0.1f;
        [UnityEngine.Tooltip("轨迹线总长度（实时输出）")]
        public SharedFloat trailLength;
        [UnityEngine.Tooltip("是否显示长度数字标签")]
        public SharedBool showLengthLabel = true;
        [UnityEngine.Tooltip("长度标签字体大小")]
        [UnityEngine.Range(1f, 10f)]
        public SharedFloat labelFontSize = 3f;
        [UnityEngine.Tooltip("长度标签颜色")]
        public UnityEngine.Color labelColor = UnityEngine.Color.white;

        [Header("移动方式")]
        [UnityEngine.Tooltip("是否使用 Rigidbody2D 移动（需要物体有 Rigidbody2D 组件）")]
        public SharedBool useRigidbody2D = false;
        [UnityEngine.Tooltip("Rigidbody2D 移动模式：true=velocity，false=MovePosition（更精确）")]
        public SharedBool useVelocityMode = true;

        private Transform _transform;
        private Rigidbody2D _rb;
        private LineRenderer _lineRenderer;
        private TextMesh _lengthLabel;
        private float _totalTrailLength;
        private Vector3 _lastPosition;

        // 区域限制
        private Vector2 _spawnRegionCenter;
        private Vector2 _spawnRegionSize;
        private bool _hasSpawnRegion;

        public override void OnStart()
        {
            _totalTrailLength = 0;
            GameObject go = GetDefaultGameObject(targetGameObject.Value);
            _transform = go != null ? go.transform : null;

            // 获取 Rigidbody2D 组件
            if (useRigidbody2D.Value && go != null)
            {
                _rb = go.GetComponent<Rigidbody2D>();
                if (_rb == null)
                {
                    _rb = go.AddComponent<Rigidbody2D>();
                    _rb.gravityScale = 0f;
                    _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
            }
            else
            {
                _rb = null;
            }

            // 获取生成区域边界
            if (Player.GameLoopManager.Inst != null)
            {
                _spawnRegionCenter = Player.GameLoopManager.Inst.spawnRegionCenter;
                _spawnRegionSize = Player.GameLoopManager.Inst.spawnRegionSize;
                _hasSpawnRegion = _spawnRegionSize != Vector2.zero;
            }
            else
            {
                _hasSpawnRegion = false;
            }

            // 初始化轨迹线
            if (showTrail.Value && _transform != null)
            {
                SetupLineRenderer(_transform);
            }

            // 初始化长度标签（独立于轨迹线）
            if (showLengthLabel.Value && _transform != null)
            {
                SetupLengthLabel(_transform);
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (_transform == null)
                return TaskStatus.Failure;

            Vector2 current = _transform.position;
            Vector2 target = targetPosition.Value;

            // 在移动前限制目标位置在区域内
            /*if (_hasSpawnRegion)
            {
                target = ClampToSpawnRegion(target);
                // 注意：不修改 targetPosition 共享变量，避免影响行为树中其他节点
            }*/

            float moveDelta = speed.Value * Time.deltaTime;

            if (useRigidbody2D.Value && _rb != null)
            {
                if (useVelocityMode.Value)
                {
                    // 使用 velocity 移动
                    Vector2 direction = (target - current).normalized;
                    _rb.velocity = direction * speed.Value;
                }
                else
                {
                    // 使用 MovePosition 移动（更精确，类似 Transform 移动）
                    Vector2 nextPos = Vector2.MoveTowards(current, target, moveDelta);
                    _rb.MovePosition(nextPos);
                }
            }
            else
            {
                // 使用原来的 Transform 移动
                _transform.position = Vector2.MoveTowards(current, target, moveDelta);
            }

            // 计算移动距离并更新
            float segmentLength = Vector3.Distance(_lastPosition, _transform.position);
            if (segmentLength > 0.001f)
            {
                _totalTrailLength += segmentLength;
                trailLength.Value = _totalTrailLength;
                _lastPosition = _transform.position;
            }

            // 更新轨迹线
            if (showTrail.Value && _lineRenderer != null)
            {
                _lineRenderer.positionCount++;
                int lastIndex = _lineRenderer.positionCount - 1;
                _lineRenderer.SetPosition(lastIndex, _transform.position);
            }

            // 更新长度标签
            if (showLengthLabel.Value && _lengthLabel != null)
            {
                _lengthLabel.text = $"{_totalTrailLength:F2}m";
                _lengthLabel.transform.position = _transform.position + new Vector3(0, 0.5f, 0);
                _lengthLabel.characterSize = 0.1f * labelFontSize.Value;
                _lengthLabel.fontSize = (int)(labelFontSize.Value * 10);
                _lengthLabel.color = labelColor;
            }

            float dist = Vector2.Distance(_transform.position, target);
            
            // 检查是否到达目标
            bool arrived = dist <= arrivalDistance.Value;
            
            // 如果使用 Rigidbody2D，到达时停止移动
            if (useRigidbody2D.Value && _rb != null && arrived)
            {
                _rb.velocity = Vector2.zero;
            }
            
            if (arrived)
            {
                //Debug.Log($"[MoveTowardsTargetUntilReached2D] 到达目标，移动距离: {_totalTrailLength:F2}m");
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }

        private Vector2 ClampToSpawnRegion(Vector2 target)
        {
            float halfW = _spawnRegionSize.x * 0.5f;
            float halfH = _spawnRegionSize.y * 0.5f;

            float minX = _spawnRegionCenter.x - halfW;
            float maxX = _spawnRegionCenter.x + halfW;
            float minY = _spawnRegionCenter.y - halfH;
            float maxY = _spawnRegionCenter.y + halfH;

            return new Vector2(
                Mathf.Clamp(target.x, minX, maxX),
                Mathf.Clamp(target.y, minY, maxY)
            );
        }

        private void SetupLineRenderer(Transform target)
        {
            // 获取或添加LineRenderer组件
            _lineRenderer = target.GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = target.gameObject.AddComponent<LineRenderer>();
            }

            // 设置LineRenderer属性
            _lineRenderer.startWidth = trailWidth.Value;
            _lineRenderer.endWidth = trailWidth.Value;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = trailColor;
            _lineRenderer.endColor = trailColor;
            _lineRenderer.positionCount = 1;
            _lineRenderer.useWorldSpace = true;

            // 初始化轨迹数据
            _totalTrailLength = 0f;
            _lastPosition = target.position;
            _lineRenderer.SetPosition(0, _lastPosition);

            // 创建长度标签
            if (showLengthLabel.Value)
            {
                SetupLengthLabel(target);
            }
        }

        private void SetupLengthLabel(Transform target)
        {
            // 创建或获取 TextMesh 组件
            _lengthLabel = target.GetComponent<TextMesh>();
            if (_lengthLabel == null)
            {
                GameObject labelObj = new GameObject("TrailLengthLabel");
                labelObj.transform.SetParent(target);
                labelObj.transform.localPosition = new Vector3(0, 0.5f, 0);
                _lengthLabel = labelObj.AddComponent<TextMesh>();
            }

            _lengthLabel.text = "0.00m";
            _lengthLabel.characterSize = 0.1f * labelFontSize.Value;
            _lengthLabel.fontSize = (int)(labelFontSize.Value * 10);
            _lengthLabel.color = labelColor;
            _lengthLabel.alignment = TextAlignment.Center;
            _lengthLabel.anchor = TextAnchor.MiddleCenter;
            _lengthLabel.richText = true;

            // 初始化轨迹数据（如果还没有初始化）
            if (_totalTrailLength == 0f && _lastPosition == Vector3.zero)
            {
                _totalTrailLength = 0f;
                _lastPosition = target.position;
            }
        }

        public override void OnReset()
        {
            targetGameObject = null;
            targetPosition = Vector2.zero;
            speed = 2f;
            arrivalDistance = 0.2f;
            showTrail = false;
            trailLength = 0f;
        }
    }
}
