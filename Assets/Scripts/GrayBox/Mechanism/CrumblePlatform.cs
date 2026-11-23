using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 未干的颜料块：这一笔刚上色还没干透。站上去一会儿就塌成一摊半透明的水痕，
    /// 过几秒才重新凝住。塌陷期间碰撞体失效——所以它既是路也是陷阱。
    /// 视觉上只做透明度变化：塌陷即变半透明，不提水痕、不压扁。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class CrumblePlatform : MonoBehaviour
    {
        enum State { Idle, Fading, Faded, Restoring }

        [Header("Timing")]
        [Tooltip("站上去多久开始塌")]
        [SerializeField] float standDelay = 0.6f;

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

        [Header("Detection")]
        [Tooltip("检测玩家是否踩在上面的范围")]
        [SerializeField] Vector2 standCheckSize = new Vector2(0.85f, 0.3f);

        [Tooltip("恢复前检查这块位置上方有没有人，有则推迟，避免把人顶起来")]
        [SerializeField] Vector2 riderCheckSize = new Vector2(1.2f, 0.8f);

        State state = State.Idle;
        Collider2D body;
        float timer;
        float retryAt;

        public MechanismKind Kind => MechanismKind.CrumblePlatform;

        /// <summary>是否处于踩不了的状态。</summary>
        public bool IsDown => state == State.Fading || state == State.Faded;

        void Awake()
        {
            body = GetComponent<Collider2D>();
            if (blockRenderer == null) blockRenderer = GetComponentInChildren<SpriteRenderer>();

            ApplyAlpha(1f);
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;
        }

        void UpdateIdle()
        {
            // 没人踩就不计时，绝不自己塌
            if (!PlayerOnTop())
            {
                timer = 0f;
                return;
            }

            timer += Time.deltaTime;
            if (timer < standDelay) return;

            EnterFading();
        }

        /// <summary>
        /// 不用 OnCollisionStay2D：玩家的速度每帧被 PlayerMotor2D 覆写，
        /// 站在薄平台上时碰撞回调会时有时无。直接查头顶有没有人更可靠。
        /// 也不能用 playerMask 直接 OverlapBox：遮罩覆盖所有层时会命中自己的碰撞体，
        /// 结果没人踩也会自己塌——所以这里显式找玩家组件。
        /// </summary>
        bool PlayerOnTop()
        {
            if (body == null) return false;

            Bounds bounds = body.bounds;
            Vector2 center = new Vector2(bounds.center.x, bounds.max.y + 0.1f);
            var hits = Physics2D.OverlapBoxAll(center, standCheckSize, 0f);

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.GetComponentInParent<PlayerMotor2D>() != null) return true;
            }

            return false;
        }

        void Update()
        {
            switch (state)
            {
                case State.Idle:
                    UpdateIdle();
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

        void EnterFading()
        {
            state = State.Fading;
            timer = 0f;

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
