using System.Collections;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Instantiates one level prefab, drops the two characters on its spawn markers,
    /// rewires the win condition and moves on to the next level when it is cleared.
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

        [Header("Flow")]
        [SerializeField] float advanceDelay = 1.2f;

        [Header("Prototype")]
        [SerializeField] bool enableHotkeys = true;
        [SerializeField] int startIndex;

        GameObject instance;
        LevelConfig current;
        int currentIndex = -1;
        Coroutine advance;

        public LevelConfig Current => current;
        public GameObject Instance => instance;
        public int CurrentIndex => currentIndex;

        void Start()
        {
            if (winChecker != null) winChecker.Won += HandleWon;
            if (catalog != null && catalog.Count > 0) LoadIndex(startIndex);
        }

        void OnDestroy()
        {
            if (winChecker != null) winChecker.Won -= HandleWon;
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

            if (Input.GetKeyDown(KeyCode.R)) LoadIndex(currentIndex);
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

        public void Load(LevelConfig config) => LoadInternal(config, IndexOf(config));

        public void LoadIndex(int index)
        {
            if (catalog == null) return;

            LoadInternal(catalog.Get(index), index);
        }

        void LoadInternal(LevelConfig config, int index)
        {
            if (config == null || config.LevelPrefab == null) return;

            if (advance != null)
            {
                StopCoroutine(advance);
                advance = null;
            }

            Unload();

            instance = Instantiate(config.LevelPrefab, levelRoot != null ? levelRoot : null);
            instance.name = config.LevelId;
            current = config;
            currentIndex = index;

            PlacePlayer(leftPlayer, "Spawn_Left");
            PlacePlayer(rightPlayer, "Spawn_Right");
            WirePortals();
        }

        public void Unload()
        {
            if (instance != null) Destroy(instance);
            instance = null;
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
            player.ResetSafePoint(target);
        }

        void WirePortals()
        {
            if (winChecker == null || instance == null) return;

            var left = FindDeep(instance.transform, "Portal_Left");
            var right = FindDeep(instance.transform, "Portal_Right");

            winChecker.Configure(
                left != null ? left.GetComponent<Portal>() : null,
                right != null ? right.GetComponent<Portal>() : null);
            winChecker.ResetState();
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
