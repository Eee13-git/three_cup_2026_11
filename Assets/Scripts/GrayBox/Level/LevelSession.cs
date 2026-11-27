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

        [Tooltip("教学关目录；选关点教学关时按 levelId 在这里找")]
        [SerializeField] LevelCatalog tutorialCatalog;

        [SerializeField] PlayerMotor2D leftPlayer;
        [SerializeField] PlayerMotor2D rightPlayer;
        [SerializeField] WinChecker winChecker;
        [SerializeField] SceneDissolve sceneDissolve;
        [SerializeField] FootprintSystem footprintSystem;
        [SerializeField] Transform levelRoot;
        [SerializeField] VisionWindow leftWindow;
        [SerializeField] VisionWindow rightWindow;
        [SerializeField] VisionFog visionFog;
        [SerializeField] TutorialHintUI tutorialHint;

        [Header("Rules")]
        [SerializeField] int livesPerLevel = 3;
        [SerializeField] float advanceDelay = 1f;

        [Tooltip("死亡后锁操作并显示弹窗的时长")]
        [SerializeField] float deathPopupSeconds = 2f;

        [Header("Effects")]
        [Tooltip("死亡时在死亡位置播放的水痕")]
        [SerializeField] GameObject waterStainPrefab;

        [Header("Prototype")]
        [SerializeField] bool enableHotkeys = true;
        [SerializeField] int startIndex;

        public event System.Action FailedEvent;
        public event System.Action<int> LivesChanged;
        public event System.Action<PlayerSide, DeathCause> PlayerDied;

        GameObject instance;
        LevelConfig current;
        LevelCatalog activeCatalog;
        int currentIndex = -1;
        int deaths;
        Coroutine advance;
        Coroutine deathPause;
        Coroutine reveal;
        LevelGoal cachedLeftGoal;
        LevelGoal cachedRightGoal;

        public LevelConfig Current => current;
        public GameObject Instance => instance;
        public int CurrentIndex => currentIndex;
        public bool Failed { get; private set; }
        public int LivesPerLevel => livesPerLevel;
        public int LivesRemaining => Mathf.Max(0, livesPerLevel - deaths);

        /// <summary>死亡弹窗是否正在显示（这期间操作被锁住）。</summary>
        public bool DeathPopupVisible { get; private set; }
        public PlayerSide DeathPopupSide { get; private set; }

        /// <summary>进关的"画出来"动画是否还在播。期间不该显示角色高光。</summary>
        public bool IsRevealing => reveal != null;

        void Start()
        {
            if (winChecker != null) winChecker.Won += HandleWon;
            if (leftPlayer != null) leftPlayer.Died += HandleDeath;
            if (rightPlayer != null) rightPlayer.Died += HandleDeath;

            if (catalog != null && catalog.Count > 0)
            {
                // 选关界面写入的 levelId 优先；未选择则回退到场景里配置的 startIndex
                var selected = FindById(GameFlow.SelectedLevelId);
                if (selected != null) Load(selected);
                else LoadIndex(startIndex);
            }
        }

        void OnDestroy()
        {
            if (winChecker != null) winChecker.Won -= HandleWon;
            if (leftPlayer != null) leftPlayer.Died -= HandleDeath;
            if (rightPlayer != null) rightPlayer.Died -= HandleDeath;
        }

        void Update()
        {
            // 站在污渍上按 F 清除自己那一处
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (cachedLeftGoal != null) cachedLeftGoal.TryClean();
                if (cachedRightGoal != null) cachedRightGoal.TryClean();
            }

            if (!enableHotkeys || catalog == null) return;

            for (int i = 0; i < 9 && i < catalog.Count; i++)
            {
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                {
                    LoadIndex(i);
                    return;
                }
            }

            // 调试跳转：Tab 下一关、Shift+Tab 上一关、End 直接到最后一关
            if (Input.GetKeyDown(KeyCode.Tab) && catalog.Count > 0)
            {
                bool back = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                int from = Mathf.Max(0, currentIndex);
                int next = (from + (back ? -1 : 1) + catalog.Count) % catalog.Count;
                LoadIndex(next);
                return;
            }

            if (Input.GetKeyDown(KeyCode.End) && catalog.Count > 0)
            {
                LoadIndex(catalog.Count - 1);
                return;
            }

            if (Input.GetKeyDown(KeyCode.R)) RetryLevel();
        }

        public void RetryLevel() => LoadInternal(current, currentIndex, activeCatalog);

        public void Load(LevelConfig config)
        {
            var source = FindSource(config, out int index);
            LoadInternal(config, index, source);
        }

        public void LoadIndex(int index)
        {
            if (catalog == null) return;

            LoadInternal(catalog.Get(index), index, catalog);
        }

        LevelCatalog FindSource(LevelConfig config, out int index)
        {
            index = IndexOf(catalog, config);
            if (index >= 0) return catalog;

            index = IndexOf(tutorialCatalog, config);
            if (index >= 0) return tutorialCatalog;

            index = -1;
            return catalog;
        }

        void LoadInternal(LevelConfig config, int index, LevelCatalog source)
        {
            if (config == null || config.LevelPrefab == null) return;

            StopPendingRoutines();

            Unload();

            activeCatalog = source != null ? source : catalog;

            // 上一关擦掉的常驻对象（中央墙、背景）要在这里还原
            if (sceneDissolve != null) sceneDissolve.ResetVisuals();

            // 上一关留下的水渍要清掉
            if (footprintSystem != null) footprintSystem.ClearAll();

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

            // 遮蔽层从进关第一帧就盖住，显现动画只在它下面进行
            if (visionFog != null) visionFog.ResetFog();
            if (reveal != null) StopCoroutine(reveal);
            reveal = StartCoroutine(RevealSequence());

            if (tutorialHint != null) tutorialHint.Show(config, index + 1);

            LivesChanged?.Invoke(LivesRemaining);
        }

        IEnumerator RevealSequence()
        {
            SetPlayersFrozen(true);

            if (sceneDissolve != null) yield return sceneDissolve.RevealRoutine();

            if (!Failed) SetPlayersFrozen(false);

            reveal = null;
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

            if (reveal != null)
            {
                StopCoroutine(reveal);
                reveal = null;
            }
        }

        void ApplyWindows(LevelConfig config)
        {
            // 压纸钉按 side 找窗口，所以这里必须带上所属侧
            if (leftWindow != null && leftPlayer != null)
            {
                leftWindow.SetSize(config.WindowSize);
                leftWindow.Configure(
                    leftPlayer.transform,
                    config.LeftWindowOffset,
                    PlayerSide.Left,
                    config.LeftWindowInverted,
                    config.WindowMirrorAxisX,
                    config.LeftWindowInvertedY,
                    config.WindowMirrorAxisY);
            }

            if (rightWindow != null && rightPlayer != null)
            {
                rightWindow.SetSize(config.WindowSize);
                rightWindow.Configure(
                    rightPlayer.transform,
                    config.RightWindowOffset,
                    PlayerSide.Right,
                    config.RightWindowInverted,
                    config.WindowMirrorAxisX,
                    config.RightWindowInvertedY,
                    config.WindowMirrorAxisY);
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

            cachedLeftGoal = left != null ? left.GetComponent<LevelGoal>() : null;
            cachedRightGoal = right != null ? right.GetComponent<LevelGoal>() : null;
        }

        void HandleDeath(PlayerSide side, DeathCause cause, Vector2 deathPosition)
        {
            if (Failed) return;

            SpawnEffect(waterStainPrefab, deathPosition);

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

        void SpawnEffect(GameObject prefab, Vector2 position)
        {
            if (prefab == null) return;

            Instantiate(prefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);
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
            // 两处污渍都清完：遮蔽层用同样的扩散效果整层擦掉
            if (visionFog != null) yield return visionFog.DissolveRoutine();

            // 再用同一套扩散把画面上其余东西一起擦掉，只留干净的画布
            if (sceneDissolve != null) yield return sceneDissolve.DissolveRoutine();

            yield return new WaitForSeconds(advanceDelay);
            advance = null;

            // 通关当前关，解锁选关界面里的下一关
            if (current != null) ThreeCup.UI.LevelProgress.UnlockNext(current.LevelId);

            int next = currentIndex + 1;
            if (activeCatalog != null && next < activeCatalog.Count)
            {
                LoadInternal(activeCatalog.Get(next), next, activeCatalog);
                yield break;
            }

            // 教学目录走完，接着进正式目录第一关
            if (activeCatalog == tutorialCatalog && catalog != null && catalog.Count > 0)
            {
                LoadInternal(catalog.Get(0), 0, catalog);
                yield break;
            }

            // 最后一关：以后把胜利窗口挂在这里
            Debug.Log("[GrayBox] 已是最后一关");
        }

        static int IndexOf(LevelCatalog source, LevelConfig config)
        {
            if (source == null || config == null) return -1;

            for (int i = 0; i < source.Count; i++)
                if (source.Get(i) == config) return i;

            return -1;
        }

        LevelConfig FindById(string levelId)
        {
            if (string.IsNullOrEmpty(levelId)) return null;

            var found = FindIn(catalog, levelId);
            if (found != null) return found;

            return FindIn(tutorialCatalog, levelId);
        }

        static LevelConfig FindIn(LevelCatalog source, string levelId)
        {
            if (source == null) return null;

            for (int i = 0; i < source.Count; i++)
            {
                var config = source.Get(i);
                if (config != null && config.LevelId == levelId) return config;
            }

            return null;
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;

            return null;
        }
    }
}
