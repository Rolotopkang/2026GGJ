using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 多玩家用手柄控制（新 Input System）。只读取指定手柄编号的输入。
/// 震动通过 GamepadVibration.Vibrate/Stop 控制。
/// </summary>
public class PlayerMovementMulti : MonoBehaviour
{
    [Header("绑定手柄")]
    [Tooltip("手柄编号（1～4），与按下确认键的手柄对应")]
    [Range(1, 4)]
    public int joystickIndex = 1;

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
    private Animal _animal;

    private void Awake()
    {
        _animal = GetComponent<Animal>();
        _animal.isplayer = true;
        _animal.GetComponent<BehaviorTree>().enabled = false;
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        Gamepad gamepad = GamepadVibration.GetGamepadByIndex(joystickIndex);
        if (gamepad == null)
        {
            _input = Vector2.zero;
            return;
        }

        Vector2 stick = gamepad.leftStick.ReadValue();
        _input.x = stick.x;
        _input.y = stick.y;

        float r2 = gamepad.rightTrigger.ReadValue();
        if (r2 >= r2PressThreshold)
        {
            if (!_r2WasPressed)
            {
                _r2WasPressed = true;
                _animal.Attack();
                GamepadVibration.Vibrate(joystickIndex, 0.6f, 0.6f);
                _vibrateStopTime = Time.time + vibrationDuration;
            }
        }
        else
        {
            _r2WasPressed = false;
        }

        if (gamepad.rightShoulder.wasPressedThisFrame)
        {
            _animal.UseGrassMagic();
        }

        if (gamepad.buttonEast.wasPressedThisFrame)  // B / 圆圈
        {
            if (_animal.DoBehavior1())
            {
                GamepadVibration.Vibrate(joystickIndex, 0.5f, 0.5f);
                _vibrateStopTime = Time.time + vibrationDuration;
            }
        }

        if (gamepad.buttonNorth.wasPressedThisFrame)  // Y / 三角
        {
            if (_animal.DoBehavior2())
            {
                GamepadVibration.Vibrate(joystickIndex, 0.5f, 0.5f);
                _vibrateStopTime = Time.time + vibrationDuration;
            }
        }

        if (_input.sqrMagnitude > 1f)
            _input.Normalize();

        if (_vibrateStopTime > 0f && Time.time >= _vibrateStopTime)
        {
            GamepadVibration.Stop(joystickIndex);
            _vibrateStopTime = -1f;
        }
    }

    private void FixedUpdate()
    {
        if (_rb != null)
            _rb.velocity = _input * _animal.moveSpeed;
    }

    private void OnDisable()
    {
        GamepadVibration.Stop(joystickIndex);
        _vibrateStopTime = -1f;
    }
}
