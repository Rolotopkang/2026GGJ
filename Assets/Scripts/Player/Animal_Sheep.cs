using System;
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

    [Header("树丛")]
    [Tooltip("在树丛中站立不动时每秒加分")]
    public int bushScorePerSecond = 1;

    private int _bushCount;
    private float _lastBushScoreTime;
    private static readonly int BushId = Animator.StringToHash("Bush");

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bush"))
        {
            if (_bushCount == 0)
                _lastBushScoreTime = Time.time;
            _bushCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bush"))
            _bushCount = Mathf.Max(0, _bushCount - 1);
    }

    private void Update()
    {
        bool inBush = _bushCount > 0;
        bool standingStill = _rb != null && _rb.velocity.sqrMagnitude <= 0.01f;

        if (_animator != null)
            _animator.SetBool(BushId, inBush && standingStill);

        if (inBush && isplayer && !_isDead && GameLoopManager.Inst != null)
        {
            float now = Time.time;
            if (now - _lastBushScoreTime >= 1f)
            {
                _lastBushScoreTime = now;
                var multi = GetComponent<PlayerMovementMulti>();
                if (multi != null)
                    GameLoopManager.Inst.AddScore(multi.joystickIndex, bushScorePerSecond);
            }
        }
    }

    public override bool DoBehavior2()
    {
        if (!base.DoBehavior1()) return false;
        return true;
    }
}
