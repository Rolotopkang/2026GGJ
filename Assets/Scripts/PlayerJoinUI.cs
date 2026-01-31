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
}
