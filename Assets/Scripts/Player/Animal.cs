using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Tools;
using Unity.Mathematics;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public bool isplayer;
    public EnumTool.AnimalType animalType;
    public SpriteRenderer animalSprite;
    private Animator _animator;

    public GameObject GrassPrefab;
    public int GrassNum = 1;

    [Header("移动")]
    [Tooltip("移动速度（单位/秒）")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;

    [Header("行为冷却")]
    [Tooltip("行为1冷却时长（秒），0 则无冷却")]
    public float behavior1CD = 0f;
    [Tooltip("行为2冷却时长（秒），0 则无冷却")]
    public float behavior2CD = 0f;

    private float _lastBehavior1Time = float.MinValue;
    private float _lastBehavior2Time = float.MinValue;
    private Rigidbody2D _rb;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        UpdateFacingFromVelocity();
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
    
    //ondeath
    public virtual void Death()
    {
        
    }
    //attack
    public virtual void Attack()
    {
        if (isplayer)
        {
            Debug.Log(name+"攻击");
        }
    }
    
    public virtual void UseGrassMagic()
    {
        if (isplayer && GrassNum>=1)
        {
            Debug.Log(name+"烟雾弹");
            GrassNum--;
            Instantiate(GrassPrefab, transform.position, quaternion.identity);
        }
    }

    public virtual bool DoBehavior1()
    {
        //if (!isplayer) return false;
        if (behavior1CD > 0f && Time.time - _lastBehavior1Time < behavior1CD) return false;
        _lastBehavior1Time = Time.time;
        Debug.Log(name+"行为1");
        return true;
    }
    
    public virtual bool DoBehavior2()
    {
        if (!isplayer) return false;
        if (behavior2CD > 0f && Time.time - _lastBehavior2Time < behavior2CD) return false;
        _lastBehavior2Time = Time.time;
        Debug.Log(name+"行为2");
        return true;
    }

    private bool GetGameStatue() => GameLoopManager.Inst.isGameStart;
}
