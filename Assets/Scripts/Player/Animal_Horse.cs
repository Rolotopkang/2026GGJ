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
    // 马npc提高走路的权重
   
    public MMF_Player ShitMMF;
    [Tooltip("屎飞出的力（Impulse）")]
    public float shitForce = 0.5f;

    public override bool DoBehavior1()
    {
        if (!base.DoBehavior1()) return false;
        ShitMMF?.PlayFeedbacks();

        return true;
    }

    public override bool DoBehavior2()
    {
        if (!base.DoBehavior1()) return false;
        return true;
    }
}
