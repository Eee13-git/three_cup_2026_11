using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 未干的颜料块：这一笔刚上色还没干透。
    /// 只要被角色碰到（顶、侧、底任意一面）就开始倒计时，时间一到塌成一摊半透明水痕，
    /// 过几秒才重新凝住。倒计时一旦开始就不会因为离开而重置。
    /// 预警只抖贴图（子物体），不动碰撞体，所以不会影响角色落脚。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class CrumblePlatform : MonoBehaviour
    {
        enum State { Idle, Counting, Fading, Faded, Restoring }

        [Header("Timing")]
        [Tooltip("被碰到后多久开始塌")]
        [SerializeField] float touchDelay = 1f;

        [Tooltip("塌下去（变半透明）的过程时长")]
        [SerializeField] float fadeDuration = 0.15f;

        [Tooltip("半透明状态维持多久")]
        [SerializeField] float fadedDelay = 2.2f;

        [Tooltip("重新凝住的过程时长")]
        [SerializeField] float restoreDuration = 0.3f;

        [Header("Visual")]
        [SerializeField] SpriteRenderer blockRenderer;

        [Tooltip("塌陷后的不透明度")]
        [SerializeField, Range(0f, 1f)] float fadedAlpha = 0.18f;

        [Header("Warning Shake")]
        [Tooltip("倒计时期间贴图抖动的幅度（世界单位），只动贴图不动碰撞体")]
        [SerializeField] float shakeAmplitude = 0.03f;

        [Tooltip("抖动频率（每秒次数）")]
        [SerializeField] float shakeFrequency = 18f;

        [Header("Detection")]
        [Tooltip("触碰检测盒在碰撞体基础上向外扩张多少，保证贴着边缘也算碰到")]
        [SerializeField] float touchPadding = 0.06f;

        [Tooltip("恢复前检查这块位置上方有没有人，有则推迟，避免把人顶起来")]
        [SerializeField] Vector2 riderCheckSize = new Vector2(1.2f, 0.8f);

        State state = State.Idle;
        Collider2D body;
        float timer;
        float retryAt;
        Vector3 blockHome;
        bool hasBlockHome;

        public MechanismKind Kind => MechanismKind.CrumblePlatform;

        /// <summary>是否处于踩不了的状态。</summary>
        public bool IsDown => state == State.Fading || state == State.Faded;

        void Awake()
        {
            body = GetComponent<Collider2D>();
            if (blockRenderer == null) blockRenderer = GetComponentInChildren<SpriteRenderer>();

            if (blockRenderer != null)
            {
                blockHome = blockRenderer.transform.localPosition;
                hasBlockHome = true;
            }

            ApplyAlpha(1f);
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;
        }

        void Update()
        {
            switch (state)
            {
                case State.Idle:
                    if (!PlayerTouching()) break;

                    state = State.Counting;
                    timer = 0f;
                    break;

                case State.Counting:
                    timer += Time.deltaTime;
                    ShakeBlock();
                    if (timer < touchDelay) break;

                    EnterFading();
                    break;

                case State.Fading:
                    timer += Time.deltaTime;
                    if (timer < fadeDuration)
                    {
                        ApplyAlpha(Mathf.Lerp(1f, fadedAlpha, timer / fadeDuration));
                        break;
                    }

                    ApplyAlpha(fadedAlpha);
                    state = State.Faded;
                    timer = 0f;
                    break;

                case State.Faded:
                    timer += Time.deltaTime;
                    if (timer < fadedDelay) break;
                    if (Time.time < retryAt) break;

                    if (BlockedByRider())
                    {
                        retryAt = Time.time + 0.35f;
                        break;
                    }

                    state = State.Restoring;
                    timer = 0f;
                    break;

                case State.Restoring:
                    timer += Time.deltaTime;
                    if (timer < restoreDuration)
                    {
                        ApplyAlpha(Mathf.Lerp(fadedAlpha, 1f, timer / restoreDuration));
                        break;
                    }

                    ApplyAlpha(1f);
                    state = State.Idle;
                    timer = 0f;
                    if (body != null) body.enabled = true;
                    break;
            }
        }

        /// <summary>
        /// 任何一面的接触都算：在碰撞体基础上向外扩 touchPadding 查一次。
        /// 不用 OnCollisionStay2D——玩家速度每帧被覆写，站在薄平台上时回调会时有时无。
        /// 也不能直接用层遮罩：遮罩覆盖所有层时会命中自己的碰撞体，所以只认玩家组件。
        /// </summary>
        bool PlayerTouching()
        {
            if (body == null) return false;

            Bounds bounds = body.bounds;
            Vector2 size = new Vector2(bounds.size.x + touchPadding * 2f, bounds.size.y + touchPadding * 2f);
            var hits = Physics2D.OverlapBoxAll(bounds.center, size, 0f);

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.GetComponentInParent<PlayerMotor2D>() != null) return true;
            }

            return false;
        }

        void ShakeBlock()
        {
            if (blockRenderer == null || !hasBlockHome) return;

            float seed = Time.time * shakeFrequency;
            float x = (Mathf.PerlinNoise(seed, 0.13f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0.37f, seed) - 0.5f) * 2f;

            blockRenderer.transform.localPosition = blockHome + new Vector3(x * shakeAmplitude, y * shakeAmplitude, 0f);
        }

        void ResetShake()
        {
            if (blockRenderer == null || !hasBlockHome) return;
            blockRenderer.transform.localPosition = blockHome;
        }

        void EnterFading()
        {
            state = State.Fading;
            timer = 0f;

            ResetShake();
            if (body != null) body.enabled = false;
        }

        void ApplyAlpha(float alpha)
        {
            if (blockRenderer == null) return;

            var c = blockRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            blockRenderer.color = c;
            blockRenderer.enabled = c.a > 0.01f;
        }

        bool BlockedByRider()
        {
            Vector2 center = (Vector2)transform.position + Vector2.up * 0.1f;
            var hits = Physics2D.OverlapBoxAll(center, riderCheckSize, 0f);

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.GetComponentInParent<PlayerMotor2D>() != null) return true;
            }

            return false;
        }

        public void Configure(SpriteRenderer newBlockRenderer)
        {
            blockRenderer = newBlockRenderer;
        }
    }
}
