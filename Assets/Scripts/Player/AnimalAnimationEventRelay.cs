using UnityEngine;

/// <summary>
/// 放在 Animator 所在的 GameObject 上，用于将动画事件转发给父级或根级挂载的 Animal。
/// 当 Animator 和 Animal 不在同一 GameObject 时，在 Animation Event 中选择本脚本的 HitCheck 等方法。
/// </summary>
public class AnimalAnimationEventRelay : MonoBehaviour
{
    private Animal _animal;

    void Awake()
    {
        _animal = GetComponentInParent<Animal>();
    }

    public void HitCheck() => _animal?.HitCheck();
    public void Attack() => _animal?.Attack();
}
