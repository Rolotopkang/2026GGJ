using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace WanderingCubes.BehaviorDesigner
{
    [TaskCategory("WanderingCubes")]
    public class UpdateCallMethod : Action
    {
        [Header("组件配置")]
        [UnityEngine.Tooltip("要调用的组件名称（如 Animal, PlayerMovementMulti）")]
        public SharedString componentName;

        [Header("方法配置")]
        [UnityEngine.Tooltip("方法列表（支持多个方法按权重随机执行）")]
        public string methodName = "";

        public override TaskStatus OnUpdate()
        {
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
            else if (compName == "animal_dog")
                component = owner.GetComponent<Animal_Dog>();
            else if (compName == "animal_horse")
                component = owner.GetComponent<Animal_Horse>();
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
            
            // 获取并调用方法
            var method = component.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
            {
                Debug.LogWarning($"[CallMethod] 在 {component.GetType().Name} 上未找到方法: {methodName}");
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
    }
}