using System;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Player;
using Tools;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;


namespace WanderingCubes.BehaviorDesigner
{
    /// <summary>
    /// 在指定 2D 区域内随机生成一个点，写入 SharedVector2。
    /// 用于 2D 漫游：目标位置 = center + [-halfExtents, +halfExtents] 的随机偏移。
    /// </summary>
    [TaskCategory("WanderingCubes")]
    [TaskDescription("在指定 2D 区域内生成随机位置并存储")]
    public class GetRandomPositionInArea2D : Action
    {
        [UnityEngine.Tooltip("区域中心（世界坐标）")]
        public SharedVector2 center = Vector2.zero;
        [UnityEngine.Tooltip("区域半尺寸（X 为水平范围，Y 为垂直范围）")]
        public SharedVector2 halfExtents = new Vector2(5f, 5f);
        [UnityEngine.Tooltip("存储结果的位置（可绑定行为树变量，如 TargetPosition2D）")]
        [RequiredField]
        public SharedVector2 storeResult;

        [Header("区域限制")]
        [UnityEngine.Tooltip("是否使用 GameLoopManager 的 spawnRegionSize 限制区域（忽略 center 和 halfExtents）")]
        public SharedBool useSpawnRegionLimit = false;

        [Header("目标距离限制")]
        [UnityEngine.Tooltip("目标与当前位置的最小距离（0 表示不限制）")]
        public SharedFloat minTargetDistance = 0f;
        [UnityEngine.Tooltip("目标与当前位置的最大距离（0 表示不限制）")]
        public SharedFloat maxTargetDistance = 0f;
        [UnityEngine.Tooltip("最大尝试次数（避免无限循环）")]
        [UnityEngine.Range(1, 100)]
        public SharedInt maxRetryAttempts = 20;

        public EnumTool.AnimalType SelfAnimalType;
        
        // 为每个枚举值提供对应的 Vector2 值
        public Vector2 GetLimitPos(EnumTool.AnimalType area)
        {
            switch (area)
            {
                case EnumTool.AnimalType.Sheep:
                    return new Vector2(2, 3);
                case EnumTool.AnimalType.Dog:
                    return new Vector2(4, 5);
                case EnumTool.AnimalType.House:
                    return new Vector2(5, 6);
                default:
                    return new Vector2(1, 2);
            }
        }

        public override TaskStatus OnUpdate()
        {
            // 调试日志
            Debug.Log($"[GetRandomPositionInArea2D] minTargetDistance={minTargetDistance.Value}, maxTargetDistance={maxTargetDistance.Value}");

            Vector2 c;
            Vector2 he;

            // 如果启用区域限制，使用 GameLoopManager 的配置
            if (useSpawnRegionLimit.Value && GameLoopManager.Inst != null)
            {
                c = GameLoopManager.Inst.spawnRegionCenter;
                he = GameLoopManager.Inst.spawnRegionSize * 0.5f;
            }
            else
            {
                c = center.Value;
                he = halfExtents.Value;
            }

            // 获取当前物体位置（用于距离限制）
            Vector2 currentPos = Vector2.zero;
            Transform selfTransform = GetDefaultGameObject(null)?.transform;
            if (selfTransform != null)
            {
                currentPos = selfTransform.position;
            }

            // 检查是否需要距离限制
            bool needDistanceLimit = minTargetDistance.Value > 0f || maxTargetDistance.Value > 0f;
            Vector2 finalPosition = Vector2.zero;
            bool foundValidPosition = false;

            // 尝试生成符合距离要求的位置
            /*int maxAttempts = needDistanceLimit ? maxRetryAttempts.Value : 1;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 randomOffset = new Vector2(
                    Random.Range(-he.x, he.x),
                    Random.Range(-he.y, he.y)
                );
                
                Vector2 candidate = c + randomOffset;

                // 检查距离限制
                if (needDistanceLimit)
                {
                    float distance = Vector2.Distance(currentPos, candidate);
                    bool withinMin = minTargetDistance.Value <= 0f || distance >= minTargetDistance.Value;
                    bool withinMax = maxTargetDistance.Value <= 0f || distance <= maxTargetDistance.Value;

                    
                    if (withinMin && withinMax)
                    {
                        finalPosition = candidate;
                        foundValidPosition = true;
                        break;
                    }
                }
                else
                {
                    finalPosition = candidate;
                    foundValidPosition = true;
                    break;
                }
            }*/

            var SelfAnimalScript = GetDefaultGameObject(null)?.GetComponent<Animal>();
            if (SelfAnimalScript != null)
            {
                SelfAnimalType = SelfAnimalScript.animalType;
            }
            Vector2 MoveDisLimit = GetLimitPos(SelfAnimalType);
            
            var randompos = GetRandomPointWithDistanceLimit(
                currentPos,
                GameLoopManager.Inst.spawnRegionCenter,
                GameLoopManager.Inst.spawnRegionSize * 0.5f,
                MoveDisLimit.x,MoveDisLimit.y);
            
                /*storeResult.Value = GetRandomPointInRectangleWithMaxDistance(
                    currentPos,
                    GameLoopManager.Inst.spawnRegionCenter,
                    GameLoopManager.Inst.spawnRegionSize * 0.5f,
                    maxTargetDistance.Value
                    );*/

                
            
            var dis = Vector2.Distance(currentPos, randompos);
            storeResult.Value = randompos;
            Debug.Log($"[MoveTowardsTargetUntilReached2D] 目标移动距离: {dis:F2}m");

            /*// 如果没有找到符合条件的位置，使用最后一次的结果
            if (!foundValidPosition && needDistanceLimit)
            {
                Vector2 randomOffset = new Vector2(
                    Random.Range(-he.x, he.x),
                    Random.Range(-he.y, he.y)
                );
                finalPosition = c + randomOffset;
            }*/

            //storeResult.Value = finalPosition;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            center = Vector2.zero;
            halfExtents = new Vector2(5f, 5f);
            storeResult = Vector2.zero;
            useSpawnRegionLimit = false;
            minTargetDistance = 0f;
            maxTargetDistance = 0f;
            maxRetryAttempts = 20;
        }
        
        public Vector2 GetRandomPointInRectangleWithMaxDistance(
            Vector2 center,           // 参考点
            Vector2 rectCenter,       // 矩形中心
            Vector2 rectHalfSize,     // 矩形半尺寸（half extents）
            float maxDistance         // 最大距离
        )
        {
            // 找到参考点到矩形各边的距离
            float toLeft = center.x - (rectCenter.x - rectHalfSize.x);
            float toRight = (rectCenter.x + rectHalfSize.x) - center.x;
            float toBottom = center.y - (rectCenter.y - rectHalfSize.y);
            float toTop = (rectCenter.y + rectHalfSize.y) - center.y;
    
            // 取最小值，确保圆完全在矩形内
            float minDistToEdge = Mathf.Min(toLeft, toRight, toBottom, toTop);
    
            // 使用实际可用的最大距离
            float actualMaxDist = Mathf.Min(maxDistance, minDistToEdge);
    
            // 在圆内随机取点
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float distance = Random.Range(0f, actualMaxDist);
    
            return center + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
        }
        
        Vector2 GetRandomPointWithDistanceLimit(
            Vector2 center,
            Vector2 rectCenter,
            Vector2 rectHalfSize,
            float minDistance,
            float maxDistance,
            int maxAttempts = 100
        )
        {
            float minX = rectCenter.x - rectHalfSize.x;
            float maxX = rectCenter.x + rectHalfSize.x;
            float minY = rectCenter.y - rectHalfSize.y;
            float maxY = rectCenter.y + rectHalfSize.y;

            for (int i = 0; i < maxAttempts; i++)
            {
                // 在矩形内随机取点
                Vector2 candidate = new Vector2(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );
        
                float dist = Vector2.Distance(center, candidate);
        
                // 检查距离是否在范围内
                if (dist >= minDistance && dist <= maxDistance)
                {
                    return candidate;
                }
            }
    
            // 如果没找到，返回范围内最近的点
            return center + (center - rectCenter).normalized * Mathf.Clamp(maxDistance, 0, 
                Vector2.Distance(center, rectCenter));
        }
    }
}
