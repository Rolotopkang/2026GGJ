using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Player
{
    public class GameLoopManager : SingletonMono<GameLoopManager>
    {

        [Header("NPC 生成")]
        [Tooltip("NPC 预制体")]
        public GameObject NPCPrefab;
        
        [Header("玩家预制体")]
        [Tooltip("拖入玩家预制体（需带 PlayerMovementMulti；若有 PlayerMovement 会自动禁用）")]
        public GameObject playerPrefab;

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

        public bool isGameStart = false;

        private Coroutine _spawnCoroutine;

        public GameState currentGameState = GameState.Logo;
        
        
        private readonly List<GameObject> _spawnedPlayers = new List<GameObject>();
        
        public enum GameState
        {
            Logo,
            WaitStart,
            Starting,
            WaitEnd,
        }

        private void Update()
        {
            if (currentGameState == GameState.Logo)
            {
                if (Keyboard.current == null) return;
                if (Keyboard.current[Key.Space].wasPressedThisFrame)
                {
                    //Start Game
                    currentGameState = GameState.WaitStart;
                    
                    TransitionController.Inst.PlayBlackTransition(0.3f,()=> PlayerJoinUI.Inst.transform.gameObject.SetActive(false),OnTransitionDown);
                }
            }
        }

        private void OnTransitionDown()
        {
            GenerateAnimals();
        }
        
        public void GenerateAnimals()
        {
            if (NPCPrefab == null)
            {
                Debug.LogWarning("[GameLoopManager] NPCPrefab 未设置，无法生成。");
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
            // 玩家预制体：优先用本组件上的，否则用 PlayerJoinManager 的
            GameObject playerPrefabToUse = playerPrefab != null ? playerPrefab : (PlayerJoinManager.Inst != null ? PlayerJoinManager.Inst.playerPrefab : null);
            int playerCount = (PlayerJoinManager.Inst != null && playerPrefabToUse != null) ? PlayerJoinManager.Inst.JoinedCount : 0;
            int totalCount = npcCount + playerCount;

            float halfW = spawnRegionSize.x * 0.5f;
            float halfH = spawnRegionSize.y * 0.5f;
            float minDistSq = minSpawnDistance * minSpawnDistance;
            var spawnedPositions = new List<Vector2>(totalCount);

            // 混合顺序：NPC 与玩家槽位，然后打乱
            var schedule = new List<(bool isPlayer, int joystickIndex)>(totalCount);
            for (int i = 0; i < npcCount; i++)
                schedule.Add((false, 0));
            for (int j = 1; j <= playerCount; j++)
                schedule.Add((true, j));

            for (int i = schedule.Count - 1; i > 0; i--)
            {
                int k = Random.Range(0, i + 1);
                var tmp = schedule[i];
                schedule[i] = schedule[k];
                schedule[k] = tmp;
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

                if (schedule[i].isPlayer)
                {
                    GameObject go = Instantiate(playerPrefabToUse, pos, Quaternion.identity, animalRoot);
                    go.name = "Player_" + schedule[i].joystickIndex;
                    var multi = go.GetComponent<PlayerMovementMulti>();
                    if (multi == null)
                        multi = go.AddComponent<PlayerMovementMulti>();
                    multi.joystickIndex = schedule[i].joystickIndex;
                    _spawnedPlayers.Add(go);
                }
                else
                {
                    Instantiate(NPCPrefab, pos, Quaternion.identity, animalRoot);
                }
            }

            _spawnCoroutine = null;
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

        public void OnGenerateFinished()
        {
            
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
