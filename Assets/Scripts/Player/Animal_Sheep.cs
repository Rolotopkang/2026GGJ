using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class Animal_Sheep : Animal
{
    public Transform shitPoint;
    public GameObject shitPrefab;
    public MMF_Player ShitMMF;
    public int ShitScore = 4;
    [Tooltip("屎飞出的力（Impulse）")]
    public float shitForce = 0.5f;

    public override bool DoBehavior1()
    {
        if (!base.DoBehavior1()) return false;
        ShitMMF?.PlayFeedbacks();
        Vector3 spawnPos;
        Vector2 awayDir;
        if (animalSprite != null && animalSprite.flipX)
        {
            spawnPos = new Vector3(2f * transform.position.x - shitPoint.position.x, shitPoint.position.y, shitPoint.position.z);
            awayDir = ((Vector2)spawnPos - (Vector2)transform.position).normalized;
        }
        else
        {
            spawnPos = shitPoint.position;
            awayDir = ((Vector2)spawnPos - (Vector2)transform.position).normalized;
        }

        Transform shitParent = (GameLoopManager.Inst != null && GameLoopManager.Inst.animalRoot != null)
            ? GameLoopManager.Inst.animalRoot : null;
        GameObject shit = Instantiate(shitPrefab, spawnPos, Quaternion.identity, shitParent);
        var rb = shit.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(-awayDir * shitForce, ForceMode2D.Impulse);
        if (isplayer)
        {
            GameLoopManager.Inst.AddScore(GetComponent<PlayerMovementMulti>().joystickIndex,ShitScore);
        }

        return true;
    }

    public override bool DoBehavior2()
    {
        if (!base.DoBehavior1()) return false;
        return true;
    }
}
