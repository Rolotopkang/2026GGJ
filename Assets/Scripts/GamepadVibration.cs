using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Windows 下手柄振动（XInput）。仅对 Xbox 或虚拟成 XInput 的手柄有效；
/// PS4/PS5 原生 DirectInput 不支持，需用 DS4Windows 等转成 XInput 才能震。
/// 非 Windows 平台调用时忽略。
/// </summary>
public static class GamepadVibration
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_VIBRATION
    {
        public ushort wLeftMotorSpeed;
        public ushort wRightMotorSpeed;
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")]
    private static extern int XInputSetState_4(int dwUserIndex, ref XINPUT_VIBRATION pVibration);

    [DllImport("xinput1_3.dll", EntryPoint = "XInputSetState")]
    private static extern int XInputSetState_3(int dwUserIndex, ref XINPUT_VIBRATION pVibration);

    private const ushort MaxMotor = 65535;
    private static bool _useV3; // xinput1_4 不可用时改用 xinput1_3
#endif

    /// <summary>
    /// 振动指定手柄。joystickIndex 为 1～4（与 Input 里 Joystick 1～4 对应）。
    /// </summary>
    public static void Vibrate(int joystickIndex, float leftMotor = 0.5f, float rightMotor = 0.5f)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (joystickIndex < 1 || joystickIndex > 4) return;
        int userIndex = joystickIndex - 1;
        var v = new XINPUT_VIBRATION
        {
            wLeftMotorSpeed = (ushort)(Mathf.Clamp01(leftMotor) * MaxMotor),
            wRightMotorSpeed = (ushort)(Mathf.Clamp01(rightMotor) * MaxMotor)
        };
        try
        {
            if (_useV3)
                XInputSetState_3(userIndex, ref v);
            else
            {
                XInputSetState_4(userIndex, ref v);
            }
        }
        catch (DllNotFoundException)
        {
            _useV3 = true;
            try { XInputSetState_3(userIndex, ref v); } catch { }
        }
#endif
    }

    /// <summary>
    /// 停止指定手柄振动。
    /// </summary>
    public static void Stop(int joystickIndex)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (joystickIndex < 1 || joystickIndex > 4) return;
        int userIndex = joystickIndex - 1;
        var v = new XINPUT_VIBRATION { wLeftMotorSpeed = 0, wRightMotorSpeed = 0 };
        try
        {
            if (_useV3)
                XInputSetState_3(userIndex, ref v);
            else
                XInputSetState_4(userIndex, ref v);
        }
        catch (DllNotFoundException) { _useV3 = true; }
        catch { }
#endif
    }
}
