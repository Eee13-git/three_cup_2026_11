using System.Collections;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Instantiates one level prefab, drops the two characters on its spawn markers,
    /// rewires the win condition, counts deaths and moves on when the level is cleared.
    /// Levels never reference each other; only the catalog knows the order.
    /// </summary>
    public sealed class LevelSession : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] LevelCatalog catalog;
        [SerializeField] PlayerMotor2D leftPlayer;
        [SerializeField] PlayerMotor2D rightPlayer;
        [SerializeField] WinChecker winChecker;
        [SerializeField] Transform levelRoot;
        [SerializeField] VisionWindow leftWindow;
        [SerializeField] VisionWindow rightWindow;

        [Header("Rules")]
        [SerializeField] int livesPerLevel = 3;
        [SerializeField] float advanceDelay = 1.2f;

        [Tooltip("死亡后锁操作并显示弹窗的时长")]
        [SerializeField] float deathPopupSeconds = 2f;

        [Header("Prototype")]
        [SerializeField] bool enableHotkeys = true;
        [SerializeField] int startIndex;

        public event System.Action FailedEvent;
        public event System.Action<int> LivesChanged;
        public event System.Action<PlayerSide, DeathCause> PlayerDied;

        GameObject instance;
        LevelConfig current;
        int currentIndex = -1;
        int deaths;
        Coroutine advance;
        Coroutine deathPause;

        public LevelConfig Current => current;
        public GameObject Instance => instance;
        public int CurrentIndex => currentIndex;
        public bool Failed { get; private set; }
        public int LivesPerLevel => livesPerLevel;
        public int LivesRemaining => Mathf.Max(0, livesPerLevel - deaths);

        /// <summary>死亡弹窗是否正在显示（这期间操作被锁住）。</summary>
        public bool DeathPopupVisible { get; private set; }
        public PlayerSide DeathPopupSide { get; private set; }

        void Start()
        {
            if (winChecker != null) winChecker.Won += HandleWon;
            if (leftPlayer != null) leftPlayer.Died += HandleDeath;
            if (rightPlayer != null) rightPlayer.Died += HandleDeath;

            if (catalog != null && catalog.Count > 0) LoadIndex(startIndex);
        }

        void OnDestroy()
        {
            if (winChecker != null) winChecker.Won -= HandleWon;
            if (leftPlayer != null) leftPlayer.Died -= HandleDeath;
            if (rightPlayer != null) rightPlayer.Died -= HandleDeath;
        }

        void Update()
        {
            if (!enableHotkeys || catalog == null) return;

            for (int i = 0; i < 9 && i < catalog.Count; i++)
            {
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                {
                    LoadIndex(i);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.R)) RetryLevel();
        }

        public void RetryLevel() => LoadIndex(currentIndex);

        public void Load(LevelConfig config) => LoadInternal(config, IndexOf(config));

        public void LoadIndex(int index)
        {
            if (catalog == null) return;

            LoadInternal(catalog.Get(index), index);
        }

        void LoadInternal(LevelConfig config, int index)
        {
            if (config == null || config.LevelPrefab == null) return;

            StopPendingRoutines();

            Unload();

            instance = Instantiate(config.LevelPrefab, levelRoot != null ? levelRoot : null);
            instance.name = config.LevelId;
            current = config;
            currentIndex = index;

            deaths = 0;
            Failed = false;
            DeathPopupVisible = false;
            SetPlayersFrozen(false);

            PlacePlayer(leftPlayer, "Spawn_Left");
            PlacePlayer(rightPlayer, "Spawn_Right");
            ApplyWindows(config);
            WireGoals();

            LivesChanged?.Invoke(LivesRemaining);
        }

        public void Unload()
        {
            if (instance != null) Destroy(instance);
            instance = null;
        }

        void StopPendingRoutines()
        {
            if (advance != null)
            {
                StopCoroutine(advance);
                advance = null;
            }

            if (deathPause != null)
            {
                StopCoroutine(deathPause);
                deathPause = null;
            }
        }

        void ApplyWindows(LevelConfig config)
        {
            if (leftWindow != null && leftPlayer != null)
            {
                leftWindow.SetSize(config.WindowSize);
                leftWindow.Configure(leftPlayer.transform, config.LeftWindowOffset);
            }

            if (rightWindow != null && rightPlayer != null)
            {
                rightWindow.SetSize(config.WindowSize);
                rightWindow.Configure(rightPlayer.transform, config.RightWindowOffset);
            }
        }

        void PlacePlayer(PlayerMotor2D player, string markerName)
        {
            if (player == null || instance == null) return;

            var marker = FindDeep(instance.transform, markerName);
            if (marker == null)
            {
                Debug.LogWarning($"[GrayBox] 关卡 {instance.name} 缺少出生点标记 {markerName}");
                return;
            }

            Vector2 target = marker.position;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.position = target;
            }

            player.transform.position = target;
            player.ResetRespawn(target);
        }

        void WireGoals()
        {
            if (winChecker == null || instance == null) return;

            var left = FindDeep(instance.transform, "Goal_Left");
            var right = FindDeep(instance.transform, "Goal_Right");

            winChecker.Configure(
                left != null ? left.GetComponent<LevelGoal>() : null,
                right != null ? right.GetComponent<LevelGoal>() : null);
            winChecker.ResetState();
        }

        void HandleDeath(PlayerSide side, DeathCause cause)
        {
            if (Failed) return;

            deaths++;
            Debug.Log($"[GrayBox] {side} 死亡（{cause}）{deaths}/{livesPerLevel}，剩余 {LivesRemaining}");
            LivesChanged?.Invoke(LivesRemaining);
            PlayerDied?.Invoke(side, cause);

            // 死亡后先锁住操作两秒，避免玩家一路按着方向键反复送死
            if (deathPause != null) StopCoroutine(deathPause);
            deathPause = StartCoroutine(DeathPauseRoutine(side));
        }

        IEnumerator DeathPauseRoutine(PlayerSide side)
        {
            DeathPopupSide = side;
            DeathPopupVisible = true;
            SetPlayersFrozen(true);

            yield return new WaitForSeconds(deathPopupSeconds);
            deathPause = null;
            DeathPopupVisible = false;

            if (deaths >= livesPerLevel)
            {
                FailLevel();
                yield break;
            }

            SetPlayersFrozen(false);
        }

        void FailLevel()
        {
            Failed = true;
            SetPlayersFrozen(true);

            // 失败窗口以后接 FailedEvent
            Debug.Log("[GrayBox] 关卡失败：生命耗尽");
            FailedEvent?.Invoke();
        }

        void SetPlayersFrozen(bool value)
        {
            if (leftPlayer != null) leftPlayer.SetFrozen(value);
            if (rightPlayer != null) rightPlayer.SetFrozen(value);
        }

        void HandleWon()
        {
            if (advance != null) return;

            advance = StartCoroutine(AdvanceAfterDelay());
        }

        IEnumerator AdvanceAfterDelay()
        {
            yield return new WaitForSeconds(advanceDelay);
            advance = null;

            int next = currentIndex + 1;
            if (catalog != null && next < catalog.Count)
            {
                LoadIndex(next);
                yield break;
            }

            // 最后一关：以后把胜利窗口挂在这里
            Debug.Log("[GrayBox] 已是最后一关");
        }

        int IndexOf(LevelConfig config)
        {
            if (catalog == null || config == null) return -1;

            for (int i = 0; i < catalog.Count; i++)
                if (catalog.Get(i) == config) return i;

            return -1;
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;

            return null;
        }
    }
}
