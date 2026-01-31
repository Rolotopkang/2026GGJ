using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class Animal_Dog : Animal
{
    // TODO: 
    // 1. 自动捡使
    // 2. 叫（特效）
    
    public GameObject shitPrefab;
    public MMF_Player ShitMMF;
    public int BarkScore = 2;
    public int PickShitScore = 5;

    public override bool DoBehavior1()
    {
        //叫
        if (!base.DoBehavior1()) return false;
        ShitMMF?.PlayFeedbacks();
        if (isplayer)
        {
            GameLoopManager.Inst.AddScore(GetComponent<PlayerMovementMulti>().joystickIndex,BarkScore);
        }

        return true;
    }

    public override bool DoBehavior2()
    {
        /*GameObject OverlapShit = CheckForPrefabInRange(shitPrefab.name, 3.0f);
        if (OverlapShit != null)
        {
            // 检测到目标，执行行为
            //消除使，加分
            Destroy(OverlapShit);
            //Debug.Log($"捡到屎了");
            
            return true;
        }
        else
        {
            // 没有检测到目标，不执行
            Debug.Log("未检测到目标，不执行行为");
            return false;
        }*/

        return true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shit"))
        {
            var shitins = other.gameObject;
            if (isplayer)
            {
                GameLoopManager.Inst.AddScore(GetComponent<PlayerMovementMulti>().joystickIndex,PickShitScore);
            }
            Destroy(shitins);
        }
    }

    // 检测周围范围内的指定预制体
    public GameObject CheckForPrefabInRange(string prefabName, float range)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
    
        foreach (var collider in colliders)
        {
            // 检查物体名称或标签是否包含目标名称
            if (collider.gameObject.name.Contains(prefabName) || 
                collider.CompareTag(prefabName))
            {
                return collider.gameObject;
            }
        }
        return null;
    }
}
