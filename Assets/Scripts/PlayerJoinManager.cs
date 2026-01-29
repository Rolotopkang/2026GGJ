using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 多手柄加入：当某个手柄按下确认键（PS4 圆圈 / Xbox B，或 A/X）时，
/// 为该手柄生成一个玩家方块，且仅该手柄控制该方块。
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
    [Tooltip("PS4 圆圈 / Xbox B")]
    public bool confirmButton1 = true;
    [Tooltip("PS4 叉 / Xbox A（也可当确认）")]
    public bool confirmButton0 = true;

    private const int MaxPlayers = 4;
    private readonly bool[] _joined = new bool[MaxPlayers];
    private readonly List<GameObject> _spawnedPlayers = new List<GameObject>();

    private void Update()
    {
        if (playerPrefab == null) return;

        for (int j = 1; j <= MaxPlayers; j++)
        {
            if (_joined[j - 1]) continue;

            if (GetConfirmDown(j))
            {
                JoinPlayer(j);
            }
        }
    }

    private bool GetConfirmDown(int joystickNumber)
    {
        int base0 = (int)KeyCode.Joystick1Button0 + (joystickNumber - 1) * 20;
        int base1 = (int)KeyCode.Joystick1Button1 + (joystickNumber - 1) * 20;
        if (confirmButton0 && Input.GetKeyDown((KeyCode)base0)) return true;
        if (confirmButton1 && Input.GetKeyDown((KeyCode)base1)) return true;
        return false;
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

        var single = go.GetComponent<PlayerMovement>();
        if (single != null)
            single.enabled = false;

        _spawnedPlayers.Add(go);
        Debug.Log($"玩家 {joystickIndex} 已加入（手柄 {joystickIndex}）");
    }

    /// <summary>当前已加入的玩家数量。</summary>
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

    /// <summary>已生成的所有玩家物体。</summary>
    public IReadOnlyList<GameObject> SpawnedPlayers => _spawnedPlayers;
}
