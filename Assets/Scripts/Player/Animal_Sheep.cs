using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class Animal_Sheep : Animal
{
    public Transform shitPoint;
    public GameObject shitPrefab;
    public MMF_Player sheepMMF;
    [Tooltip("屎飞出的力（Impulse）")]
    public float shitForce = 0.5f;

    public override bool DoBehavior1()
    {
        if (!base.DoBehavior1()) return false;
        sheepMMF?.PlayFeedbacks();

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

        GameObject shit = Instantiate(shitPrefab, spawnPos, Quaternion.identity);
        var rb = shit.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(-awayDir * shitForce, ForceMode2D.Impulse);

        return true;
    }

    public override bool DoBehavior2()
    {
        return base.DoBehavior2();
    }
}
