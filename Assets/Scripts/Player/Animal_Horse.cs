using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

public class Animal_Horse : Animal
{
    // TODO :
    // 1. 冲刺撞人（类似于攻击，被撞提供一个力）
    // 2. 走长路 加分
    // 3. 马npc提高走路的权重

    public MMF_Player ShitMMF;

    [Tooltip("屎自动消失时间（秒），0 或负数表示不消失")] public float shitLifetime = 10f;

    [Header("冲刺冲量检测")] [Tooltip("冲刺检测中心偏移（相对于自身位置）")]
    public float dashHitRangeOffset = 0.5f;

    [Tooltip("冲刺检测范围半径")] public float dashHitRangeRadius = 1.0f;
    [Tooltip("冲刺冲量大小")] public float dashImpulseForce = 1f;
    [Tooltip("冲刺时移动速度倍率")] [Range(1f, 5f)] public float dashSpeedMultiplier = 1f;
    [Tooltip("冲刺持续时间（秒）")] public float dashDuration = 0.3f;

    private bool _isDashing = false;
    private Vector2 _dashDirection;

    public override bool DoBehavior1()
    {
        if (!CanMove()) return false;
        if (behavior2CD > 0f && Time.time - _lastBehavior2Time < behavior2CD) return false;
        _lastBehavior2Time = Time.time;

        // 开始冲刺
        StartDash();
        return true;
    }

    private void StartDash()
    {
        if (_isDashing) return;
        _isDashing = true;
        
        float facing = (animalSprite != null && animalSprite.flipX) ? -1f : 1f;
        _dashDirection = new Vector2(facing, 0f).normalized;
        
        if (_animator != null)
            _animator.SetTrigger("Kick"); // 复用攻击动画

        // 施加初始速度
        if (_rb != null)
        {
            //_rb.velocity = _dashDirection * moveSpeed * dashSpeedMultiplier;
        }

        // 开始协程处理冲刺
        StartCoroutine(DashCoroutine());
    }

    private System.Collections.IEnumerator DashCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            // 持续检测碰撞
            CheckDashHit();

            // 保持冲刺速度
            if (_rb != null && !_isDead)
            {
                //_rb.velocity = _dashDirection * moveSpeed * dashSpeedMultiplier;
            }

            yield return null;
        }

        // 冲刺结束
        if (_rb != null && !_isDead)
        {
            _rb.velocity = Vector2.zero;
        }

        _isDashing = false;
    }

    /// <summary>
    /// 冲刺时的碰撞检测，对碰撞到的对象施加冲量
    /// </summary>
    private void CheckDashHit()
    {
        if (_isDead || !CanMove()) return;

        float facing = (animalSprite != null && animalSprite.flipX) ? -1f : 1f;
        Vector2 center = (Vector2)transform.position + Vector2.right * (facing * dashHitRangeOffset);

        var hits = Physics2D.OverlapCircleAll(center, dashHitRangeRadius);
        foreach (var col in hits)
        {
            if (col == null || col.gameObject == gameObject) continue;

            // 对被撞对象施加冲量
            Rigidbody2D otherRb = col.GetComponent<Rigidbody2D>();
            if (otherRb != null && !otherRb.isKinematic)
            {
                // 使用 Impulse 模式（一次性冲量），方向为冲刺方向
                // 数值已经足够大，可以产生明显的瞬时移动效果
                if (col.gameObject.name.Contains("Shit"))
                {
                    dashImpulseForce = 0.01f;
                }

                otherRb.AddForce(_dashDirection * dashImpulseForce, ForceMode2D.Impulse);
            }
        }
    }
}
