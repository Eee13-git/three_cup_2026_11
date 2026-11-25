using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 可开关的方块（补上的笔触）：关掉时碰撞体失效、视觉隐藏，打开时恢复。
    /// 出现时从一端向另一端"画出来"，而不是整块闪现。
    /// 初始状态由 startActive 决定，Switch 只负责在切换时通知它。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ToggleBlock : MonoBehaviour, ISwitchTarget
    {
        [SerializeField] bool startActive = true;
        [SerializeField] bool visibleWhenInactive;
        [SerializeField] Color activeColor = new Color(0.48f, 0.72f, 0.42f, 1f);
        [SerializeField] Color inactiveColor = new Color(0.48f, 0.72f, 0.42f, 0.18f);

        [Header("Grow")]
        [Tooltip("从无到有的时长")]
        [SerializeField] float growSeconds = 0.35f;

        [Tooltip("从本地 -X 端开始长（勾掉则从 +X 端）")]
        [SerializeField] bool growFromLeft = true;

        SpriteRenderer sprite;
        Collider2D body;

        Vector3 baseScale;
        Vector3 basePosition;
        float baseWidth;
        float grow;

        public bool Active { get; private set; }

        public MechanismKind Kind => MechanismKind.ToggleBlock;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            body = GetComponent<Collider2D>();

            baseScale = transform.localScale;
            basePosition = transform.localPosition;
            baseWidth = sprite != null ? sprite.bounds.size.x * Mathf.Abs(baseScale.x) : 1f;

            Active = startActive;
            grow = startActive ? 1f : 0f;
            ApplyGrow();
        }

        public void SetActive(bool active)
        {
            if (Active == active) return;

            Active = active;

            // 打开时从 0 宽开始，下一帧起才逐渐长出来
            if (active)
            {
                grow = 0f;
                ApplyGrow();
            }

            if (body != null) body.enabled = active;
        }

        void Update()
        {
            float target = Active ? 1f : 0f;
            if (Mathf.Approximately(grow, target)) return;

            grow = Mathf.MoveTowards(grow, target, Time.deltaTime / Mathf.Max(growSeconds, 0.01f));
            ApplyGrow();
        }

        void ApplyGrow()
        {
            float shift = baseWidth * (1f - grow) * 0.5f;
            float anchoredX = growFromLeft ? -shift : shift;

            if (grow <= 0.001f)
            {
                transform.localScale = new Vector3(0f, baseScale.y, baseScale.z);
                transform.localPosition = basePosition + new Vector3(anchoredX, 0f, 0f);

                if (sprite != null)
                {
                    sprite.enabled = visibleWhenInactive;
                    sprite.color = inactiveColor;
                }

                if (body != null) body.enabled = false;
                return;
            }

            // 缩放锚点固定在某一端，视觉上像从那里画出来
            transform.localScale = new Vector3(baseScale.x * grow, baseScale.y, baseScale.z);
            transform.localPosition = basePosition + new Vector3(anchoredX, 0f, 0f);

            if (body != null) body.enabled = Active;

            if (sprite != null)
            {
                sprite.enabled = Active || visibleWhenInactive;
                sprite.color = Active ? activeColor : inactiveColor;
            }
        }
    }
}
