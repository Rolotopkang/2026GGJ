using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Player;
using Tools;
using Unity.Mathematics;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public bool isplayer;
    public EnumTool.AnimalType animalType;
    public SpriteRenderer animalSprite;
    public Animator _animator;

    public GameObject GrassPrefab;
    public int GrassNum = 1;

    [Header("移动")]
    [Tooltip("移动速度（单位/秒）")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;

    [Header("攻击判定")]
    [Tooltip("正前方检测中心偏移（相对于自身位置）")]
    public float hitRangeOffset = 0.5f;
    [Tooltip("检测范围半径")]
    public float hitRangeRadius = 0.4f;

    [Header("行为冷却")]
    [Tooltip("攻击冷却时长（秒），0 则无冷却")]
    public float attackCD = 5f;
    [Tooltip("行为1冷却时长（秒），0 则无冷却")]
    public float behavior1CD = 0f;
    [Tooltip("行为2冷却时长（秒），0 则无冷却")]
    public float behavior2CD = 0f;

    public float _lastAttackTime = float.MinValue;
    public float _lastBehavior1Time = float.MinValue;
    public float _lastBehavior2Time = float.MinValue;
    public Rigidbody2D _rb;
    public bool _isDead;

    private static readonly int WalkingId = Animator.StringToHash("Walking");

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void LateUpdate()
    {
        if (_isDead) return;
        UpdateFacingFromVelocity();
        UpdateWalkingState();
    }

    private void FixedUpdate()
    {
        if (!CanMove() && _rb != null)
            _rb.velocity = Vector2.zero;
    }

    public bool CanMove()
    {
        return GameLoopManager.Inst != null &&
               GameLoopManager.Inst.currentGameState == GameLoopManager.GameState.Starting;
    }

    private bool CanUseAbilities()
    {
        if (GameLoopManager.Inst == null) return false;
        var state = GameLoopManager.Inst.currentGameState;
        return state == GameLoopManager.GameState.Starting ||
               state == GameLoopManager.GameState.WaitEnd;
    }

    /// <summary>
    /// 是否正在播放攻击动画（用于移动锁定）。
    /// </summary>
    public bool IsAttacking()
    {
        if (_animator == null) return false;
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName("Attack");
    }

    private void UpdateWalkingState()
    {
        if (_animator == null) return;
        bool moving = _rb != null && _rb.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool(WalkingId, moving);
    }

    /// <summary>
    /// 根据 Rigidbody2D 速度水平翻转 animalSprite。velocity.x &gt; 0 朝右，velocity.x &lt; 0 朝左。
    /// </summary>
    private void UpdateFacingFromVelocity()
    {
        if (animalSprite == null || _rb == null) return;
        float vx = _rb.velocity.x;
        if (vx > 0.01f) animalSprite.flipX = true;
        else if (vx < -0.01f) animalSprite.flipX = false;
    }
    
    public virtual void Death()
    {
        if (_isDead) return;
        _isDead = true;

        // 停止物理移动
        if (_rb != null)
            _rb.velocity = Vector2.zero;

        // 禁用玩家输入并通知 GameLoopManager（若有 PlayerMovementMulti）
        var movement = GetComponent<PlayerMovementMulti>();
        if (movement != null)
        {
            movement.enabled = false;
            if (GameLoopManager.Inst != null)
                GameLoopManager.Inst.SetPlayerDead(movement.joystickIndex);
        }

        // 禁用 NPC AI（若有 BehaviorTree）
        var bt = GetComponent<BehaviorTree>();
        if (bt != null)
            bt.enabled = false;

        // 禁用碰撞体，尸体不再参与碰撞
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (_animator != null)
            _animator.SetTrigger("Death");
    }

    public virtual bool Attack()
    {
        if (!CanUseAbilities()) return false;
        if (attackCD > 0f && Time.time - _lastAttackTime < attackCD) return false;
        _lastAttackTime = Time.time;
        if (_animator != null)
            _animator.SetTrigger("Attack");
        return true;
    }

    /// <summary>
    /// 动画事件可调用。检测正前方范围内的其他 Animal 并触发其 Death()。仅当自身未死亡时有效。
    /// </summary>
    public void HitCheck()
    {
        if (_isDead || !CanMove()) return;

        float facing = (animalSprite != null && animalSprite.flipX) ? -1f : 1f;
        Vector2 center = (Vector2)transform.position + Vector2.right * (facing * hitRangeOffset);

        var hits = Physics2D.OverlapCircleAll(center, hitRangeRadius);
        foreach (var col in hits)
        {
            if (col == null || col.gameObject == gameObject) continue;
            var other = col.GetComponent<Animal>();
            if (other != null && other != this && !other._isDead)
                other.Death();
        }
    }
    
    public virtual void UseGrassMagic()
    {
        if (!CanMove() || !isplayer || GrassNum < 1) return;
        Debug.Log(name+"烟雾弹");
        GrassNum--;
        //TODO
        // Transform parent = (GameLoopManager.Inst != null && GameLoopManager.Inst.animalRoot != null)
        //     ? GameLoopManager.Inst.animalRoot : null;
        // Instantiate(GrassPrefab, transform.position, quaternion.identity, parent);
    }

    public virtual bool DoBehavior1()
    {
        if (!CanUseAbilities()) return false;
        if (behavior1CD > 0f && Time.time - _lastBehavior1Time < behavior1CD) return false;
        _lastBehavior1Time = Time.time;
        Debug.Log(name+"行为1");
        return true;
    }
    
    public virtual bool DoBehavior2()
    {
        if (!CanUseAbilities() || !isplayer) return false;
        if (behavior2CD > 0f && Time.time - _lastBehavior2Time < behavior2CD) return false;
        _lastBehavior2Time = Time.time;
        Debug.Log(name+"行为2");
        return true;
    }

    private bool GetGameStatue() => GameLoopManager.Inst.isGameStart;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float facing = (animalSprite != null && animalSprite.flipX) ? -1f : 1f;
        Vector3 center = transform.position + Vector3.right * (facing * hitRangeOffset);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(center, hitRangeRadius);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.15f);
        Gizmos.DrawSphere(center, hitRangeRadius);
    }
#endif
}
