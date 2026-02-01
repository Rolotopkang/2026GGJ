using UnityEngine;
using UnityEngine.UI;
using Player;

/// <summary>
/// 挂载在 HUD 上，将 1～4P 分数槽中 bar 的 FillAmount 实时绑定到 GameLoopManager 的 _playerScores / maxScore。
/// 仅显示已连接玩家对应的分数槽。
/// </summary>
public class HUDScoreBars : MonoBehaviour
{
    [Tooltip("1～4P 分数条 Image（需设为 Filled 类型）。若不指定则尝试按名称查找")]
    [SerializeField] private Image[] scoreBars = new Image[4];

    private static readonly string[] SlotNames = { "1P分数槽", "2P分数槽 ", "3P分数槽", "4P分数槽" };
    private Transform[] _slotTransforms;

    private void Awake()
    {
        if (scoreBars == null || scoreBars.Length < 4) scoreBars = new Image[4];
        _slotTransforms = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            var slot = transform.Find(SlotNames[i]);
            _slotTransforms[i] = slot;
            if (scoreBars[i] != null) continue;
            if (slot != null)
            {
                var bar = slot.Find("bar");
                if (bar != null)
                    scoreBars[i] = bar.GetComponent<Image>();
            }
        }
    }

    private void OnEnable()
    {
        RefreshSlotVisibility();
    }

    /// <summary>
    /// 根据已加入玩家显示/隐藏对应分数槽。
    /// </summary>
    private void RefreshSlotVisibility()
    {
        if (_slotTransforms == null) return;
        bool useJoinManager = PlayerJoinManager.Inst != null;
        for (int i = 0; i < 4 && i < _slotTransforms.Length; i++)
        {
            if (_slotTransforms[i] == null) continue;
            bool show = useJoinManager && PlayerJoinManager.Inst.IsPlayerJoined(i + 1);
            _slotTransforms[i].gameObject.SetActive(show);
            if (show && GameLoopManager.Inst != null) RefreshDeathState(i);
        }
    }

    private void Update()
    {
        if (GameLoopManager.Inst == null) return;
        int max = Mathf.Max(1, GameLoopManager.Inst.maxScore);
        for (int i = 0; i < 4 && i < scoreBars.Length; i++)
        {
            if (scoreBars[i] != null)
            {
                int score = GameLoopManager.Inst.GetPlayerScore(i + 1);
                scoreBars[i].fillAmount = Mathf.Clamp01((float)score / max);
            }
            RefreshDeathState(i);
        }
    }

    /// <summary>
    /// 根据玩家死亡状态设置 Slot 的第 3、4 个子物体可见性：第 3 个子物体=存活显示，第 4 个子物体=死亡显示。
    /// </summary>
    private void RefreshDeathState(int playerIndex)
    {
        if (_slotTransforms == null || playerIndex < 0 || playerIndex >= _slotTransforms.Length) return;
        var slot = _slotTransforms[playerIndex];
        if (slot == null || !slot.gameObject.activeSelf) return;

        bool isDead = GameLoopManager.Inst.IsPlayerDead(playerIndex + 1);
        int childCount = slot.childCount;
        if (childCount > 2)
            slot.GetChild(2).gameObject.SetActive(!isDead);
        if (childCount > 3)
            slot.GetChild(3).gameObject.SetActive(isDead);
    }
}
