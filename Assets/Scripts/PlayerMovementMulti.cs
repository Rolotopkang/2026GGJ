using UnityEngine;

/// <summary>
/// 多玩家用手柄控制：只读取指定手柄编号的输入，不与其他玩家串键。
/// 由 PlayerJoinManager 在生成玩家时挂上并设置 joystickIndex。
/// </summary>
public class PlayerMovementMulti : MonoBehaviour
{
    [Header("绑定手柄")]
    [Tooltip("手柄编号（1～4），与按下确认键的手柄对应")]
    [Range(1, 4)]
    public int joystickIndex = 1;

    [Header("移动设置")]
    [Tooltip("移动速度（单位/秒）")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;

    [Tooltip("是否使用 Rigidbody2D 移动")]
    public bool useRigidbody2D = false;

    [Header("R2 扳机与累加器")]
    [Tooltip("按下 R2 时累加器 +1 并振动当前手柄")]
    public int accumulator;

    [Tooltip("R2 视为按下的阈值（0～1）")]
    [Range(0.2f, 0.9f)]
    public float r2PressThreshold = 0.5f;

    [Tooltip("振动时长（秒）")]
    [Range(0.05f, 0.5f)]
    public float vibrationDuration = 0.15f;

    private Rigidbody2D _rb;
    private Vector2 _input;
    private bool _r2WasPressed;
    private float _vibrateStopTime = -1f;

    private void Awake()
    {
        // 不在这里缓存轴名：joystickIndex 是生成后由 PlayerJoinManager 设置的，
        // Awake 执行时还是预制体默认值 1，会导致玩家 2/3/4 也读 J1 的轴。
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
        _input.x = Input.GetAxisRaw("Horizontal_J" + joystickIndex);
        _input.y = Input.GetAxisRaw("Vertical_J" + joystickIndex);

        if (_input.sqrMagnitude > 1f)
            _input.Normalize();

        if (!useRigidbody2D)
            transform.position += (Vector3)(_input * moveSpeed * Time.deltaTime);

        // R2 扳机：仅用 axis 9（主）和 axis 5（Alt），按下时累加器 +1。用绝对值避免负值漏判；不用 4/10 以免松开时误触发。
        float r2Main = Input.GetAxis("TriggerR2_J" + joystickIndex);
        float r2Alt = Input.GetAxis("TriggerR2_J" + joystickIndex + "_Alt");
        float r2Abs = Mathf.Max(Mathf.Abs(r2Main), Mathf.Abs(r2Alt));
        if (r2Abs >= r2PressThreshold)
        {
            if (!_r2WasPressed)
            {
                _r2WasPressed = true;
                accumulator += 1;
                GamepadVibration.Vibrate(joystickIndex, 0.6f, 0.6f);
                _vibrateStopTime = Time.time + vibrationDuration;
            }
        }
        else
        {
            _r2WasPressed = false;
        }

        if (_vibrateStopTime > 0f && Time.time >= _vibrateStopTime)
        {
            GamepadVibration.Stop(joystickIndex);
            _vibrateStopTime = -1f;
        }
    }

    private void FixedUpdate()
    {
        if (useRigidbody2D && _rb != null)
            _rb.velocity = _input * moveSpeed;
    }

    private void OnDisable()
    {
        GamepadVibration.Stop(joystickIndex);
        _vibrateStopTime = -1f;
    }
}
