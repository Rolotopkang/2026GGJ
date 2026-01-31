using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Player
{
    public class GameLoopManager : SingletonMono<GameLoopManager>
    {

        [Header("NPC 生成")]
        [Tooltip("NPC 预制体列表，生成时从中随机选择并尽量保证各类数量均衡")]
        [FormerlySerializedAs("NPCPrefab")]
        public GameObject[] NPCPrefabs;

        [Tooltip("生成数量")]
        [Min(1)]
        public int spawnCount = 10;

        [Tooltip("生成区域中心（世界坐标）")]
        public Vector2 spawnRegionCenter = Vector2.zero;

        [Tooltip("生成区域大小（宽 X 高）")]
        public Vector2 spawnRegionSize = new Vector2(10f, 6f);

        [Tooltip("生成节奏曲线：X=序号归一化(0~1)，Y=本次生成后等待秒数。例：前几个 0.1s，后面 0.05s")]
        public AnimationCurve spawnIntervalCurve = AnimationCurve.EaseInOut(0f, 0.15f, 1f, 0.05f);

        [Tooltip("每个生成物之间的最小距离，0 则不限制")]
        [Min(0f)]
        public float minSpawnDistance = 0.5f;

        [Tooltip("尝试放置时最大随机次数，避免无限循环")]
        [Min(10)]
        public int maxSpawnAttempts = 50;

        [Header("父物体")]
        [Tooltip("生成的所有 NPC 与玩家将挂在此物体下，不填则挂在场景根")]
        public Transform animalRoot;

        [Header("游戏结束条件")]
        [Tooltip("达到此分数时触发游戏结束")]
        [Min(1)]
        public int maxScore = 100;
        [Tooltip("仅剩一名存活玩家时结束游戏。取消勾选可单手柄测试")]
        public bool endWhenOneSurvivor = true;

        [Header("胜利聚焦")]
        [Tooltip("聚焦动画时长（秒）")]
        public float focusDuration = 1.5f;
        [Tooltip("聚焦节奏曲线：X=0~1 进度，Y=插值系数。EaseInOut 为前慢后快再慢")]
        public AnimationCurve focusCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("聚焦完成时，摄像机父父物体相对胜利玩家位置的 XY 偏移（世界坐标）")]
        public Vector2 focusCameraOffset = Vector2.zero;
        [Tooltip("聚焦时的目标 orthographicSize（越小画面越大）")]
        [Min(0.1f)]
        public float focusOrthoSize = 2.5f;
        [Tooltip("聚焦完成后的回调，可在 Inspector 中绑定")]
        public UnityEngine.Events.UnityEvent onFocusComplete;

        public bool isGameStart = false;

        private Coroutine _spawnCoroutine;

        public GameState currentGameState = GameState.Logo;

        public MMF_Player StartBanner;
        
        public GameObject EndBanner;

        public GameObject GameMap;
        
        private readonly List<GameObject> _spawnedPlayers = new List<GameObject>();

        private int[] _playerScores = new int[0];
        private bool[] _playerDead = new bool[0];
        private int _winnerIndex = -1;

        private Camera _mainCamera;
        private Transform _focusTarget;
        private Vector3 _initialFocusTargetPosition;
        private float _initialCamOrthoSize;
        private Coroutine _focusCoroutine;

        /// <summary>
        /// 获取本局获胜玩家的索引（1～4）。若无获胜者返回 -1。
        /// </summary>
        public int WinnerIndex => _winnerIndex;
        
        public enum GameState
        {
            Logo,
            PickPlayer,
            WaitStart,
            Starting,
            WaitEnd,
        }

        private void Start()
        {
            StartBanner.gameObject.SetActive(false);
            GameMap.SetActive(false);
        }

        private void Update()
        {

            if (currentGameState == GameState.PickPlayer && PlayerJoinUI.Inst.IsReadyToStart)
            {
                if (Keyboard.current == null) return;
                if (Keyboard.current[Key.Space].wasPressedThisFrame)
                {
                    TransitionToWaitStart();
                    TransitionController.Inst.PlayBlackTransition(0.3f,
                        () =>
                        {
                            PlayerJoinUI.Inst.transform.gameObject.SetActive(false);
                            GameMap.SetActive(true);
                        }
                    ,OnTransitionDown);
                }
            }

            if (currentGameState == GameState.Starting)
            {
                CheckGameOverConditions();
            }
        }

        public void TestStartGame()
        {
            TransitionToWaitStart();
            TransitionController.Inst.PlayBlackTransition(0.3f,()=> PlayerJoinUI.Inst.transform.gameObject.SetActive(false),OnTransitionDown);
        }

        private void TransitionToWaitStart()
        {
            currentGameState = GameState.WaitStart;
            InitPlayerState();
        }

        private void InitPlayerState()
        {
            int count = PlayerJoinManager.Inst != null ? PlayerJoinManager.Inst.JoinedCount : 0;
            count = Mathf.Clamp(count, 0, 4);
            _playerScores = new int[count];
            _playerDead = new bool[count];
            for (int i = 0; i < count; i++)
            {
                _playerScores[i] = 0;
                _playerDead[i] = false;
            }
            _winnerIndex = -1;
        }

        private void CheckGameOverConditions()
        {
            int count = _playerScores.Length;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                if (_playerScores[i] >= maxScore)
                {
                    _winnerIndex = i + 1;
                    OnGameOver();
                    return;
                }
            }

            if (endWhenOneSurvivor)
            {
                int aliveCount = 0;
                int lastAliveIdx = -1;
                for (int i = 0; i < count; i++)
                {
                    if (!_playerDead[i])
                    {
                        aliveCount++;
                        lastAliveIdx = i;
                    }
                }
                if (aliveCount <= 1)
                {
                    _winnerIndex = lastAliveIdx >= 0 ? lastAliveIdx + 1 : -1;
                    OnGameOver();
                }
            }
        }

        /// <summary>
        /// 给指定玩家加分。playerIndex 为 1～4 的手柄/玩家编号。
        /// </summary>
        public void AddScore(int playerIndex, int amount)
        {
            int idx = playerIndex - 1;
            if (idx < 0 || idx >= _playerScores.Length) return;
            _playerScores[idx] += amount;
        }

        /// <summary>
        /// 将指定玩家设为死亡状态。playerIndex 为 1～4 的手柄/玩家编号。
        /// </summary>
        public void SetPlayerDead(int playerIndex)
        {
            int idx = playerIndex - 1;
            if (idx < 0 || idx >= _playerDead.Length) return;
            _playerDead[idx] = true;
        }

        /// <summary>
        /// 获取玩家分数。playerIndex 为 1～4。无效索引返回 0。
        /// </summary>
        public int GetPlayerScore(int playerIndex)
        {
            int idx = playerIndex - 1;
            if (idx < 0 || idx >= _playerScores.Length) return 0;
            return _playerScores[idx];
        }

        /// <summary>
        /// 获取玩家是否已死亡。playerIndex 为 1～4。无效索引返回 true。
        /// </summary>
        public bool IsPlayerDead(int playerIndex)
        {
            int idx = playerIndex - 1;
            if (idx < 0 || idx >= _playerDead.Length) return true;
            return _playerDead[idx];
        }

        private void OnTransitionDown()
        {
            GenerateAnimals();
        }
        
        public void GenerateAnimals()
        {
            if (NPCPrefabs == null || NPCPrefabs.Length == 0)
            {
                Debug.LogWarning("[GameLoopManager] NPCPrefabs 未设置或为空，无法生成。");
                return;
            }

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
            }

            _spawnCoroutine = StartCoroutine(SpawnAnimalsCoroutine());
        }

        private IEnumerator SpawnAnimalsCoroutine()
        {
            int npcCount = Mathf.Max(1, spawnCount);
            int playerCount = (PlayerJoinManager.Inst != null && NPCPrefabs != null && NPCPrefabs.Length > 0) ? PlayerJoinManager.Inst.JoinedCount : 0;
            int totalCount = npcCount + playerCount;

            float halfW = spawnRegionSize.x * 0.5f;
            float halfH = spawnRegionSize.y * 0.5f;
            float minDistSq = minSpawnDistance * minSpawnDistance;
            var spawnedPositions = new List<Vector2>(totalCount);

            // 各预制体已生成数量，用于均衡分布
            int[] spawnCountPerPrefab = new int[NPCPrefabs.Length];

            // 混合顺序：NPC 与玩家槽位，然后打乱
            var schedule = new List<(bool isPlayer, int joystickIndex)>(totalCount);
            for (int i = 0; i < npcCount; i++)
                schedule.Add((false, 0));
            for (int j = 1; j <= playerCount; j++)
                schedule.Add((true, j));

            for (int i = schedule.Count - 1; i > 0; i--)
            {
                int k = Random.Range(0, i + 1);
                (schedule[i], schedule[k]) = (schedule[k], schedule[i]);
            }

            for (int i = 0; i < schedule.Count; i++)
            {
                float t = schedule.Count > 1 ? (float)i / (schedule.Count - 1) : 0f;
                float delay = spawnIntervalCurve != null
                    ? Mathf.Max(0f, spawnIntervalCurve.Evaluate(t))
                    : 0.1f;

                if (i > 0)
                    yield return new WaitForSeconds(delay);

                Vector2 pos2 = TryGetNonOverlapPosition(spawnedPositions, halfW, halfH, minDistSq);
                Vector3 pos = new Vector3(pos2.x, pos2.y, 0f);
                spawnedPositions.Add(pos2);

                GameObject prefab = GetNPCPrefabWithBalancedDistribution(spawnCountPerPrefab);
                if (prefab == null) continue;

                if (schedule[i].isPlayer)
                {
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity, animalRoot);
                    go.name = "Player_" + schedule[i].joystickIndex;
                    var multi = go.GetComponent<PlayerMovementMulti>();
                    if (multi == null)
                        multi = go.AddComponent<PlayerMovementMulti>();
                    multi.joystickIndex = schedule[i].joystickIndex;
                    _spawnedPlayers.Add(go);
                }
                else
                {
                    Instantiate(prefab, pos, Quaternion.identity, animalRoot);
                }
            }
            _spawnCoroutine = null;
            ShowStartingUI();
        }

        public void ShowStartingUI()
        {
            StartBanner.gameObject.SetActive(true);
        }

        public void OnShowStartingUIEnd()
        {
            StartBanner.gameObject.SetActive(false);
            currentGameState = GameState.Starting;
        }

        /// <summary>
        /// 从 NPCPrefabs 中选取一个预制体，优先选已生成数量较少的类型以保持均衡。
        /// 会更新 spawnCountPerPrefab，调用方需传入同一个数组。
        /// </summary>
        private GameObject GetNPCPrefabWithBalancedDistribution(int[] spawnCountPerPrefab)
        {
            if (NPCPrefabs == null || NPCPrefabs.Length == 0 || spawnCountPerPrefab == null || spawnCountPerPrefab.Length != NPCPrefabs.Length)
                return null;

            int minCount = int.MaxValue;
            var minIndices = new List<int>();

            for (int i = 0; i < NPCPrefabs.Length; i++)
            {
                if (NPCPrefabs[i] == null) continue;
                int c = spawnCountPerPrefab[i];
                if (c < minCount)
                {
                    minCount = c;
                    minIndices.Clear();
                    minIndices.Add(i);
                }
                else if (c == minCount)
                {
                    minIndices.Add(i);
                }
            }

            if (minIndices.Count == 0) return null;

            int idx = minIndices[Random.Range(0, minIndices.Count)];
            spawnCountPerPrefab[idx]++;
            return NPCPrefabs[idx];
        }

        private Vector2 TryGetNonOverlapPosition(List<Vector2> existing, float halfW, float halfH, float minDistSq)
        {
            if (minDistSq <= 0f || existing.Count == 0)
            {
                return new Vector2(
                    spawnRegionCenter.x + Random.Range(-halfW, halfW),
                    spawnRegionCenter.y + Random.Range(-halfH, halfH));
            }

            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                float x = spawnRegionCenter.x + Random.Range(-halfW, halfW);
                float y = spawnRegionCenter.y + Random.Range(-halfH, halfH);
                Vector2 cand = new Vector2(x, y);

                bool ok = true;
                for (int j = 0; j < existing.Count; j++)
                {
                    float dx = cand.x - existing[j].x;
                    float dy = cand.y - existing[j].y;
                    if (dx * dx + dy * dy < minDistSq)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) return cand;
            }

            return new Vector2(
                spawnRegionCenter.x + Random.Range(-halfW, halfW),
                spawnRegionCenter.y + Random.Range(-halfH, halfH));
        }

        private void OnGameOver()
        {
            if (currentGameState == GameState.WaitEnd) return;
            currentGameState = GameState.WaitEnd;
            Debug.Log("GameOver, Winner is "+ _winnerIndex);
            StartFocusOnWinner();

        }

        private void StartFocusOnWinner()
        {
            if (_winnerIndex < 1) return;

            Transform winnerTransform = GetWinnerTransform();
            if (winnerTransform == null) return;

            _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            _focusTarget = GetCameraFocusTarget(_mainCamera.transform);
            _initialFocusTargetPosition = _focusTarget.position;
            _initialCamOrthoSize = _mainCamera.orthographicSize;

            if (_focusCoroutine != null)
                StopCoroutine(_focusCoroutine);
            _focusCoroutine = StartCoroutine(FocusOnWinnerCoroutine(winnerTransform));
        }

        private Transform GetCameraFocusTarget(Transform camTransform)
        {
            if (camTransform.parent != null && camTransform.parent.parent != null)
                return camTransform.parent.parent;
            if (camTransform.parent != null)
                return camTransform.parent;
            return camTransform;
        }

        private Transform GetWinnerTransform()
        {
            foreach (GameObject go in _spawnedPlayers)
            {
                if (go == null) continue;
                var multi = go.GetComponent<PlayerMovementMulti>();
                if (multi != null && multi.joystickIndex == _winnerIndex)
                    return go.transform;
            }
            return null;
        }

        private IEnumerator FocusOnWinnerCoroutine(Transform winner)
        {
            float elapsed = 0f;
            Vector3 startPos = _focusTarget.position;
            float startSize = _mainCamera.orthographicSize;
            float startZ = startPos.z;
            Vector2 targetPos2 = new Vector2(winner.position.x + focusCameraOffset.x, winner.position.y + focusCameraOffset.y);
            Vector3 targetPos = new Vector3(targetPos2.x, targetPos2.y, startZ);

            AnimationCurve curve = focusCurve != null ? focusCurve : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            float duration = Mathf.Max(0.01f, focusDuration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eval = curve.Evaluate(t);

                _focusTarget.position = Vector3.LerpUnclamped(startPos, targetPos, eval);
                _mainCamera.orthographicSize = Mathf.LerpUnclamped(startSize, focusOrthoSize, eval);

                yield return null;
            }

            _focusTarget.position = targetPos;
            _mainCamera.orthographicSize = focusOrthoSize;
            _focusCoroutine = null;

            onFocusComplete?.Invoke();
        }

        /// <summary>
        /// 恢复摄像头到聚焦前的原位与 orthographicSize。
        /// </summary>
        public void RestoreCamera()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            if (_focusCoroutine != null)
            {
                StopCoroutine(_focusCoroutine);
                _focusCoroutine = null;
            }

            if (_focusTarget != null)
                _focusTarget.position = _initialFocusTargetPosition;
            _mainCamera.orthographicSize = _initialCamOrthoSize;
        }

        public void OpenEndBanner()
        {
            EndBanner.SetActive(true);
            EndBanner.transform.GetChild(0).GetChild(_winnerIndex-1).GetComponent<MMF_Player>().PlayFeedbacks();
        }

        public void ReturnPickPlayer()
        {
            TransitionController.Inst.PlayBlackTransition(0.3f,
                () =>
                {
                    RestoreCamera();
                    if (EndBanner != null)
                    {
                        EndBanner.transform.GetChild(0).GetChild(_winnerIndex-1).GetComponent<MMF_Player>().RestoreInitialValues();
                        EndBanner.SetActive(false);
                    }
                    ClearSpawnedObjects();
                    GamepadVibration.ClearJoined();
                    if (PlayerJoinManager.Inst != null) PlayerJoinManager.Inst.ResetJoinedState();
                    if (PlayerJoinUI.Inst != null)
                    {
                        PlayerJoinUI.Inst.ResetToPickState();
                        PlayerJoinUI.Inst.gameObject.SetActive(true);
                    }
                    GameMap.SetActive(false);
                },
                () =>
                {
                    currentGameState = GameState.PickPlayer;
                });
        }

        private void ClearSpawnedObjects()
        {
            _spawnedPlayers.Clear();
            if (animalRoot == null) return;
            for (int i = animalRoot.childCount - 1; i >= 0; i--)
                Destroy(animalRoot.GetChild(i).gameObject);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center = new Vector3(spawnRegionCenter.x, spawnRegionCenter.y, 0f);
            Vector3 size = new Vector3(spawnRegionSize.x, spawnRegionSize.y, 0.01f);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.6f);
            Gizmos.DrawWireCube(center, size);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.15f);
            Gizmos.DrawCube(center, size);
        }
#endif
    }
}
