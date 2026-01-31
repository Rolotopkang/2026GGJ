using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// 多手柄加入（新 Input System）：当某个手柄按下确认键（A/B）时生成玩家。
/// </summary>
public class PlayerJoinManager : SingletonMono<PlayerJoinManager>
{
    [Header("玩家预制体")]
    [Tooltip("拖入玩家预制体（需带 PlayerMovementMulti；若有 PlayerMovement 会自动禁用）")]
    public GameObject playerPrefab;

    [Header("确认键")]
    [Tooltip("Xbox B / PS 圆圈")]
    public bool confirmButton1 = true;

    [Tooltip("Xbox A / PS 叉")] public bool confirmButton0 = false;

    private const int MaxPlayers = 4;
    private readonly bool[] _joined = new bool[MaxPlayers];
    

    private void Update()
    {
        // Logo 阶段：已加入玩家重复按确认键 → OnAlreadyJoined
        if (Player.GameLoopManager.Inst != null &&
            Player.GameLoopManager.Inst.currentGameState == Player.GameLoopManager.GameState.Logo &&
            PlayerJoinUI.Inst != null)
        {
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                Gamepad g = Gamepad.all[i];
                if (g == null) continue;
                if (!GamepadVibration.IsGamepadJoined(g)) continue;

                bool confirmDown = (confirmButton0 && g.buttonSouth.wasPressedThisFrame) ||
                                  (confirmButton1 && g.buttonEast.wasPressedThisFrame);
                if (!confirmDown) continue;

                int joystickIndex = GamepadVibration.GetJoystickIndexByGamepad(g);
                if (joystickIndex > 0)
                {
                    PlayerJoinUI.Inst.OnAlreadyJoined(joystickIndex);
                }
                return;
            }
        }

        // 未加入玩家按下确认键 → JoinPlayer
        if (playerPrefab == null) return;
        if (JoinedCount >= MaxPlayers) return;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad g = Gamepad.all[i];
            if (g == null) continue;
            if (GamepadVibration.IsGamepadJoined(g)) continue;

            bool confirmDown = (confirmButton0 && g.buttonSouth.wasPressedThisFrame) ||
                              (confirmButton1 && g.buttonEast.wasPressedThisFrame);
            if (!confirmDown) continue;

            int joystickIndex = GamepadVibration.RegisterJoinedGamepad(g);
            if (joystickIndex == 0) continue;

            JoinPlayer(joystickIndex);
            return;
        }
    }

    private void JoinPlayer(int joystickIndex)
    {
        _joined[joystickIndex - 1] = true;
        Debug.Log($"玩家 {joystickIndex} 已加入（手柄 {joystickIndex}）");
        PlayerJoinUI.Inst.OnPlayerJoin(joystickIndex);
    }

    public int JoinedCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < MaxPlayers; i++)
                if (_joined[i]) n++;
            return n;
        }
    }
}
