using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerJoinUI : SingletonMono<PlayerJoinUI>
{
    public GameObject starthint;
    private int playerNum;
    public Transform JoinHint;
    public Transform joinedPlayer;

    private void Start()
    {
        if (joinedPlayer != null)
        {
            for (int i = 0; i < joinedPlayer.childCount; i++)
                joinedPlayer.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (starthint != null)
            starthint.SetActive(IsReadyToStart);
    }

    public void OnAlreadyJoined(int joystickIndex)
    {
        joinedPlayer.GetChild(joystickIndex-1).gameObject.GetComponent<MMF_Player>().PlayFeedbacks();
    }
    public void OnPlayerJoin(int joystickIndex)
    {
        playerNum++;
        JoinHint.GetChild(joystickIndex-1).gameObject.SetActive(false);
        joinedPlayer.GetChild(joystickIndex-1).gameObject.SetActive(true);
    }

    public void OnPlayerQuit(int joystickIndex)
    {
        playerNum--;
    }

    public bool IsReadyToStart => playerNum >= 2;

    /// <summary>
    /// 重置选人 UI 状态，用于完全重置（返回选人界面时）。
    /// </summary>
    public void ResetToPickState()
    {
        playerNum = 0;
        if (JoinHint != null)
        {
            for (int i = 0; i < JoinHint.childCount; i++)
                JoinHint.GetChild(i).gameObject.SetActive(true);
        }
        if (joinedPlayer != null)
        {
            for (int i = 0; i < joinedPlayer.childCount; i++)
                joinedPlayer.GetChild(i).gameObject.SetActive(false);
        }
    }
}
