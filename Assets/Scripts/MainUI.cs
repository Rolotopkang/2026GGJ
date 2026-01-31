using System;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainUI : MonoBehaviour
{
    public GameObject PlayerJoinUI;
    public GameObject Main;

    private bool isStart = false;

    private void Start()
    {
        if (PlayerJoinUI != null) PlayerJoinUI.SetActive(false);
        if (Main != null) Main.SetActive(true);
    }

    private void Update()
    {
        if (AnyGamepadButtonPressed() && GameLoopManager.Inst != null && 
            GameLoopManager.Inst.currentGameState == GameLoopManager.GameState.Logo
            &&!isStart)
        {
            isStart = true;
            TransitionController.Inst.PlayBlackTransition(0.3f,() =>
                {
                    PlayerJoinUI.SetActive(true);
                    Main.SetActive(false);
                }, () =>
            {
                GameLoopManager.Inst.currentGameState = GameLoopManager.GameState.PickPlayer;
            }
            );
        }
    }

    private static bool AnyGamepadButtonPressed()
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad g = Gamepad.all[i];
            if (g == null) continue;
            if (g.buttonSouth.wasPressedThisFrame || g.buttonNorth.wasPressedThisFrame ||
                g.buttonEast.wasPressedThisFrame || g.buttonWest.wasPressedThisFrame ||
                g.leftShoulder.wasPressedThisFrame || g.rightShoulder.wasPressedThisFrame ||
                g.leftStickButton.wasPressedThisFrame || g.rightStickButton.wasPressedThisFrame ||
                g.startButton.wasPressedThisFrame || g.selectButton.wasPressedThisFrame)
                return true;
        }
        return false;
    }
}
