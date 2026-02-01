using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace WanderingCubes.BehaviorDesigner
{
    /// <summary>
    /// 方法配置（方法名 + 权重）
    /// </summary>
    [System.Serializable]
    public class MethodConfig
    {
        [UnityEngine.Tooltip("方法名称")]
        public string methodName = "";

        [UnityEngine.Tooltip("执行权重（数值越大执行概率越高）")]
        [Range(1, 100)]
        public int weight = 10;
    }

    /// <summary>
    /// 调用挂载行为树物体的指定组件上的公共方法。
    /// 支持多个方法按权重随机执行。
    /// </summary>
    [TaskCategory("WanderingCubes")]
    [TaskDescription("调用物体上的指定方法（支持按权重随机执行）")]
    public class CallMethod : Action
    {
        [UnityEngine.Tooltip("要调用的组件名称（如 Animal, PlayerMovementMulti）")]
        public SharedString componentName;

        [Header("方法配置")]
        [UnityEngine.Tooltip("方法列表（支持多个方法按权重随机执行）")]
        public List<MethodConfig> methods = new List<MethodConfig>();

        [UnityEngine.Tooltip("是否使用权重随机模式（关闭则按顺序执行第一个方法）")]
        public bool useWeightedRandom = true;
        
        [UnityEngine.Tooltip("要调用的组件名称（如 Animal, PlayerMovementMulti）")]
        public float RandomNumberSet;

        public float RandomNumber;

        public override void OnStart()
        {
            RandomNumber = Random.Range(0, 10.0f);
        }

        public override TaskStatus OnUpdate()
        {
            if (RandomNumberSet <= RandomNumber)
            {
                return TaskStatus.Success;    
            }
            
            GameObject owner = GetDefaultGameObject(null);
            if (owner == null)
            {
                Debug.LogWarning("[CallMethod] 未找到行为树挂载的物体");
                return TaskStatus.Failure;
            }

            string compName = componentName.Value;
            if (string.IsNullOrEmpty(compName))
            {
                Debug.LogWarning("[CallMethod] 组件名称为空");
                return TaskStatus.Failure;
            }

            // 查找组件
            Component component = null;
            compName = compName.ToLower();

            if (compName == "animal")
                component = owner.GetComponent<Animal>();
            else if (compName == "rigidbody2d" || compName == "rigidbody")
                component = owner.GetComponent<Rigidbody2D>();
            else if (compName == "transform")
                component = owner.GetComponent<Transform>();
            else if (compName == "animal_sheep")
                component = owner.GetComponent<Animal_Sheep>();
            else
            {
                var components = owner.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c.GetType().Name.ToLower().Contains(compName))
                    {
                        component = c;
                        break;
                    }
                }
            }

            if (component == null)
            {
                Debug.LogWarning($"[CallMethod] 未在 {owner.name} 上找到组件: {componentName.Value}");
                return TaskStatus.Failure;
            }

            // 选择要执行的方法
            string targetMethodName = "";
            
            if (methods.Count > 0)
            {
                if (useWeightedRandom && methods.Count > 1)
                {
                    // 加权随机选择
                    int totalWeight = 0;
                    foreach (var m in methods)
                    {
                        totalWeight += m.weight;
                    }

                    int randomValue = Random.Range(0, totalWeight);
                    int currentWeight = 0;

                    for (int i = 0; i < methods.Count; i++)
                    {
                        currentWeight += methods[i].weight;
                        if (randomValue < currentWeight)
                        {
                            targetMethodName = methods[i].methodName;
                            break;
                        }
                    }
                }
                else
                {
                    // 按顺序执行第一个
                    targetMethodName = methods[0].methodName;
                }
            }
            else
            {
                Debug.LogWarning("[CallMethod] 方法列表为空");
                return TaskStatus.Failure;
            }

            if (string.IsNullOrEmpty(targetMethodName))
            {
                Debug.LogWarning("[CallMethod] 未选择到有效的方法");
                return TaskStatus.Failure;
            }

            // 获取并调用方法
            var method = component.GetType().GetMethod(targetMethodName,
                BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
            {
                Debug.LogWarning($"[CallMethod] 在 {component.GetType().Name} 上未找到方法: {targetMethodName}");
                return TaskStatus.Failure;
            }

            try
            {
                method.Invoke(component, null);
                return TaskStatus.Success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CallMethod] 调用失败: {e.Message}");
                return TaskStatus.Failure;
            }
        }

        public override void OnReset()
        {
            componentName = "";
            methods.Clear();
            useWeightedRandom = true;
        }
    }
}
