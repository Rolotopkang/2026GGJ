using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class Animal_Horse : Animal
{
    [Header("踢击")]
    [Tooltip("踢击范围中心偏移（相对自身，正为右）")]
    public float kickRangeOffset = 0.6f;
    [Tooltip("踢击范围半径")]
    public float kickRangeRadius = 0.5f;
    [Tooltip("施加给 Animal 的目标速度（会根据质量缩放冲量）")]
    public float kickSpeedAnimal = 3f;
    [Tooltip("施加给 Shit 的目标速度（通常设小一些避免屎飞太远）")]
    public float kickSpeedShit = 2f;
    [Tooltip("力方向的随机角度偏差（度），±此值")]
    [Range(0f, 45f)]
    public float kickAngleRandom = 15f;
    [Tooltip("踢到 Animal 时的加分（shit 不计分）")]
    public int kickAnimalScore = 3;

    [Header("行走加分")]
    [Tooltip("每走多少米加一次分")]
    public float distancePerScore = 10f;
    [Tooltip("行走达标时加的分数")]
    public int walkScoreAmount = 1;

    private static readonly int KickId = Animator.StringToHash("Kick");
    private float _accumulatedWalkDistance;
    private static int? _shitLayer;

    private static int ShitLayer
    {
        get
        {
            if (!_shitLayer.HasValue)
                _shitLayer = LayerMask.NameToLayer("Shit");
            return _shitLayer.Value;
        }
    }

    public override bool DoBehavior1()
    {
        if (!base.DoBehavior1()) return false;
        if (_animator != null)
            _animator.SetTrigger(KickId);
        KickHitCheck();
        return true;
    }

    /// <summary>
    /// 动画事件可调用。检测前方范围内的 Animal 和 Shit，施加远离马的力。
    /// 力按质量缩放：冲量 = 目标速度 × 质量，保证轻重物体都能被推动。
    /// </summary>
    public override void KickHitCheck()
    {
        if (_isDead || !CanUseAbilities()) return;

        float facing = (animalSprite != null && animalSprite.flipX) ? -1f : 1f;
        Vector2 center = (Vector2)transform.position + Vector2.right * (facing * kickRangeOffset);

        var hits = Physics2D.OverlapCircleAll(center, kickRangeRadius);
        foreach (var col in hits)
        {
            if (col == null || col.gameObject == gameObject) continue;

            var rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 toTarget = (Vector2)col.transform.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude < 0.0001f) continue;

            Vector2 baseDir = toTarget.normalized;
            float angleDeg = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
            float randomOffset = Random.Range(-kickAngleRandom, kickAngleRandom);
            float finalAngle = angleDeg + randomOffset;
            Vector2 forceDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            bool isShit = col.gameObject.layer == ShitLayer || col.CompareTag("Shit");
            float kickSpeed = isShit ? kickSpeedShit : kickSpeedAnimal;
            float impulse = kickSpeed * rb.mass;
            rb.AddForce(forceDir * impulse, ForceMode2D.Impulse);

            if (!isShit && isplayer && kickAnimalScore != 0 && GameLoopManager.Inst != null)
            {
                var multi = GetComponent<PlayerMovementMulti>();
                if (multi != null)
                    GameLoopManager.Inst.AddScore(multi.joystickIndex, kickAnimalScore);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isplayer || _isDead || !CanMove() || _rb == null || distancePerScore <= 0f) return;

        _accumulatedWalkDistance += _rb.velocity.magnitude * Time.fixedDeltaTime;
        while (_accumulatedWalkDistance >= distancePerScore && GameLoopManager.Inst != null)
        {
            _accumulatedWalkDistance -= distancePerScore;
            var multi = GetComponent<PlayerMovementMulti>();
            if (multi != null)
                GameLoopManager.Inst.AddScore(multi.joystickIndex, walkScoreAmount);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float facing = (animalSprite != null && animalSprite.flipX) ? -1f : 1f;
        Vector3 center = transform.position + Vector3.right * (facing * kickRangeOffset);
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(center, kickRangeRadius);
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.15f);
        Gizmos.DrawSphere(center, kickRangeRadius);
    }
#endif
}
