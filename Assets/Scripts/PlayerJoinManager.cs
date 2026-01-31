using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// 多手柄加入（新 Input System）：当某个手柄按下确认键（A/B）时生成玩家。
/// </summary>
public class PlayerJoinManager : MonoBehaviour
{
    [Header("玩家预制体")]
    [Tooltip("拖入玩家预制体（需带 PlayerMovementMulti；若有 PlayerMovement 会自动禁用）")]
    public GameObject playerPrefab;

    [Header("生成位置")]
    [Tooltip("4 名玩家的生成位置（按手柄 1～4 顺序）")]
    public Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(-2f, 0f, 0f),
        new Vector3(0f, 0f, 0f),
        new Vector3(2f, 0f, 0f),
        new Vector3(4f, 0f, 0f)
    };

    [Header("确认键")]
    [Tooltip("Xbox B / PS 圆圈")]
    public bool confirmButton1 = true;
    [Tooltip("Xbox A / PS 叉")]
    public bool confirmButton0 = true;

    private const int MaxPlayers = 4;
    private readonly bool[] _joined = new bool[MaxPlayers];
    private readonly List<GameObject> _spawnedPlayers = new List<GameObject>();

    private void Update()
    {
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

        Vector3 pos = joystickIndex <= spawnPositions.Length
            ? spawnPositions[joystickIndex - 1]
            : Vector3.right * (joystickIndex - 1) * 2f;

        GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
        go.name = "Player_" + joystickIndex;

        var multi = go.GetComponent<PlayerMovementMulti>();
        if (multi == null)
            multi = go.AddComponent<PlayerMovementMulti>();
        multi.joystickIndex = joystickIndex;
        _spawnedPlayers.Add(go);
        Debug.Log($"玩家 {joystickIndex} 已加入（手柄 {joystickIndex}）");
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

    public IReadOnlyList<GameObject> SpawnedPlayers => _spawnedPlayers;
}
