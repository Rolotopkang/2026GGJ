using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;
using System.Collections.Generic;

/// <summary>
/// 手柄振动（新 Input System）。通过 IDualMotorRumble.SetMotorSpeeds 控制震动，
/// 兼容 Xbox（Gamepad）与 PS4/PS5（DualShock4GamepadHID / DualSenseGamepadHID）。joystickIndex 1～4 为加入顺序。
/// </summary>
public static class GamepadVibration
{
    private static readonly List<Gamepad> _joinedGamepads = new List<Gamepad>();

    public static void Vibrate(int joystickIndex, float leftMotor = 0.5f, float rightMotor = 0.5f)
    {
        var gamepad = GetGamepadByIndex(joystickIndex);
        if (gamepad == null) return;

        float l = Mathf.Clamp01(leftMotor);
        float r = Mathf.Clamp01(rightMotor);

        if (gamepad is IDualMotorRumble rumble)
        {
            rumble.SetMotorSpeeds(l, r);
        }
        else
        {
            gamepad.SetMotorSpeeds(l, r);
        }
    }

    public static void Stop(int joystickIndex)
    {
        var gamepad = GetGamepadByIndex(joystickIndex);
        if (gamepad == null) return;

        if (gamepad is IDualMotorRumble rumble)
        {
            rumble.SetMotorSpeeds(0f, 0f);
        }
        else
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    public static void StopAll()
    {
        for (int i = 0; i < _joinedGamepads.Count; i++)
        {
            if (_joinedGamepads[i] == null) continue;
            if (_joinedGamepads[i] is IDualMotorRumble rumble)
                rumble.SetMotorSpeeds(0f, 0f);
            else
                _joinedGamepads[i].SetMotorSpeeds(0f, 0f);
        }
    }

    /// <summary>
    /// 清空所有已加入手柄，用于完全重置选人状态。
    /// </summary>
    public static void ClearJoined()
    {
        StopAll();
        _joinedGamepads.Clear();
    }

    public static Gamepad GetGamepadByIndex(int joystickIndex)
    {
        if (joystickIndex < 1 || joystickIndex > 4) return null;
        int idx = joystickIndex - 1;
        if (idx >= _joinedGamepads.Count) return null;
        var g = _joinedGamepads[idx];
        return g;
    }

    public static int RegisterJoinedGamepad(Gamepad gamepad)
    {
        if (gamepad == null || _joinedGamepads.Count >= 4) return 0;
        int id = gamepad.deviceId;
        for (int i = 0; i < _joinedGamepads.Count; i++)
        {
            if (_joinedGamepads[i] != null && _joinedGamepads[i].deviceId == id)
                return 0;
        }
        _joinedGamepads.Add(gamepad);
        return _joinedGamepads.Count;
    }

    public static bool IsGamepadJoined(Gamepad gamepad)
    {
        if (gamepad == null) return false;
        int id = gamepad.deviceId;
        for (int i = 0; i < _joinedGamepads.Count; i++)
        {
            if (_joinedGamepads[i] != null && _joinedGamepads[i].deviceId == id)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取已加入手柄对应的 joystickIndex（1～4），未找到返回 0。
    /// </summary>
    public static int GetJoystickIndexByGamepad(Gamepad gamepad)
    {
        if (gamepad == null) return 0;
        int id = gamepad.deviceId;
        for (int i = 0; i < _joinedGamepads.Count; i++)
        {
            if (_joinedGamepads[i] != null && _joinedGamepads[i].deviceId == id)
                return i + 1;
        }
        return 0;
    }
}
