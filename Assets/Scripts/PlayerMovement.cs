using UnityEngine;

/// <summary>
/// 支持键盘 WASD 和手柄左摇杆的 2D 移动控制，运动速度可在 Inspector 中调节。
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动速度（单位/秒）")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;

    [Tooltip("是否使用 Rigidbody2D 移动（勾选则用物理速度，不勾选则直接改位置）")]
    public bool useRigidbody2D = false;

    private Rigidbody2D _rb;
    private Vector2 _input;

    private void Awake()
    {
        if (useRigidbody2D)
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
                _rb.gravityScale = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    private void Update()
    {
        // 键盘 WASD + 手柄左摇杆（Unity 默认把摇杆映射到 Horizontal/Vertical）
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");

        // 归一化，避免斜向移动更快
        if (_input.sqrMagnitude > 1f)
            _input.Normalize();

        if (!useRigidbody2D)
        {
            transform.position += (Vector3)(_input * moveSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (useRigidbody2D && _rb != null)
        {
            _rb.velocity = _input * moveSpeed;
        }
    }
}
