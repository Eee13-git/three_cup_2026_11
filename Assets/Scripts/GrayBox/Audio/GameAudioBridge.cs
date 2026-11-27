using System.Collections.Generic;
using ThreeCup.GrayBox;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeCup.Audio
{
    /// <summary>
    /// 把场景里的游戏事件接到 GameAudio：
    /// 跳跃/落地（按 Grounded 变化判断）、死亡、清理污渍、开关/压板、存档点、通关、失败、UI 点击。
    /// 每个场景自动创建一个，不跨场景保留。
    /// </summary>
    public sealed class GameAudioBridge : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Ensure();
        }

        // 菜单流程会连续切场景（Start → Select → 关卡），第一个场景里建好的实例跟着旧场景一起被销毁，
        // 所以每加载一个场景都要确认还有一个实例在跑，否则进关后没有任何音效事件。
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Ensure();

        static void Ensure()
        {
            if (FindObjectOfType<GameAudioBridge>() != null) return;

            var go = new GameObject("GameAudioBridge");
            go.AddComponent<GameAudioBridge>();
        }

        sealed class PlayerState
        {
            public bool Grounded;
            public float Warmup;
            public float LastFallSpeed;
            public Vector2 Respawn;
        }

        LevelSession session;
        readonly Dictionary<PlayerMotor2D, PlayerState> players = new Dictionary<PlayerMotor2D, PlayerState>();
        readonly HashSet<LevelGoal> cleanedGoals = new HashSet<LevelGoal>();
        readonly Dictionary<Switch, bool> switchPressed = new Dictionary<Switch, bool>();
        readonly Dictionary<TandemPlate, bool> platePressed = new Dictionary<TandemPlate, bool>();

        bool winPlayed;
        float rescan;

        void Start()
        {
            HookButtons();
            BindSession();
        }

        void Update()
        {
            BindSession();
            PollPlayers();
            PollGoals();
            PollMechanisms();
        }

        void BindSession()
        {
            if (session != null) return;

            session = FindObjectOfType<LevelSession>();
            if (session == null) return;

            session.PlayerDied += OnPlayerDied;
            session.FailedEvent += OnFailed;
            winPlayed = false;
            cleanedGoals.Clear();
        }

        void OnDestroy()
        {
            if (session == null) return;

            session.PlayerDied -= OnPlayerDied;
            session.FailedEvent -= OnFailed;
        }

        void OnPlayerDied(PlayerSide side, DeathCause cause) => GameAudio.Play(Sfx.Death, 0.9f);

        void OnFailed() => GameAudio.Play(Sfx.Fail, 0.9f);

        void PollPlayers()
        {
            // 进关/重开后角色实例会换，定期重扫一次
            rescan -= Time.deltaTime;
            if (rescan <= 0f)
            {
                rescan = 0.5f;

                foreach (var player in FindObjectsOfType<PlayerMotor2D>())
                {
                    if (players.ContainsKey(player)) continue;

                    players[player] = new PlayerState
                    {
                        Grounded = player.Grounded,
                        Warmup = 0.5f,
                        Respawn = player.RespawnPoint,
                    };
                }

                var stale = new List<PlayerMotor2D>();
                foreach (var pair in players)
                    if (pair.Key == null) stale.Add(pair.Key);
                foreach (var key in stale) players.Remove(key);
            }

            foreach (var pair in players)
            {
                var player = pair.Key;
                var state = pair.Value;
                if (player == null) continue;

                state.Warmup -= Time.deltaTime;

                bool grounded = player.Grounded;
                var rb = player.GetComponent<Rigidbody2D>();
                float vy = rb != null ? rb.velocity.y : 0f;

                // 落地音只在真正有下落速度时响，并按落速缩放音量（轻轻一跳不发声）
                if (!state.Grounded && grounded && state.Warmup <= 0f && state.LastFallSpeed > 5f)
                {
                    float strength = Mathf.InverseLerp(5f, 13f, state.LastFallSpeed);
                    GameAudio.Play(Sfx.Land, Mathf.Lerp(0.06f, 0.18f, strength), Random.Range(0.82f, 0.92f));
                }

                if (state.Grounded && !grounded && vy > 1f)
                    GameAudio.Play(Sfx.Jump, 0.5f, Random.Range(0.95f, 1.05f));

                // 只记录下落中的速度：落地那一帧马达会把 vy 清零，不能在这里把落速覆盖成 0
                if (vy < -0.1f) state.LastFallSpeed = -vy;
                state.Grounded = grounded;

                var respawn = player.RespawnPoint;
                if ((respawn - state.Respawn).sqrMagnitude > 0.0001f)
                {
                    GameAudio.Play(Sfx.Checkpoint, 0.6f);
                    state.Respawn = respawn;
                }
            }
        }

        void PollGoals()
        {
            foreach (var goal in FindObjectsOfType<LevelGoal>())
            {
                if (!goal.Cleaned || cleanedGoals.Contains(goal)) continue;

                cleanedGoals.Add(goal);
                GameAudio.Play(Sfx.Clean, 0.9f);
            }

            if (winPlayed || session == null || session.Failed) return;

            var goals = FindObjectsOfType<LevelGoal>();
            if (goals.Length < 2) return;

            foreach (var goal in goals)
                if (!goal.Cleaned) return;

            winPlayed = true;
            GameAudio.Play(Sfx.Win, 0.9f);
        }

        void PollMechanisms()
        {
            foreach (var sw in FindObjectsOfType<Switch>())
            {
                bool pressed = sw.Pressed;
                switchPressed.TryGetValue(sw, out bool was);
                if (pressed && !was) GameAudio.Play(Sfx.Switch, 0.7f, Random.Range(0.97f, 1.03f));
                switchPressed[sw] = pressed;
            }

            foreach (var plate in FindObjectsOfType<TandemPlate>())
            {
                bool pressed = plate.Pressed;
                platePressed.TryGetValue(plate, out bool was);
                if (pressed && !was) GameAudio.Play(Sfx.Switch, 0.55f, Random.Range(0.95f, 1.0f));
                platePressed[plate] = pressed;
            }
        }

        void HookButtons()
        {
            foreach (var button in FindObjectsOfType<Button>(true))
            {
                button.onClick.AddListener(() => GameAudio.Play(Sfx.UiClick, 0.5f));
            }
        }
    }
}
