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
    private Animator _animator;

    public GameObject GrassPrefab;
    public int GrassNum = 1;

    public void Start()
    {
        //播放动画
        //
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

    public virtual void DoBehavior1()
    {
        if (isplayer)
        {
            Debug.Log(name+"行为1");
        }
    }
    
    public virtual void DoBehavior2()
    {
        if (isplayer)
        {
            Debug.Log(name+"行为2");
        }
    }
    
    


    private bool GetGameStatue() => GameLoopManager.Inst.isGameStart;
}
