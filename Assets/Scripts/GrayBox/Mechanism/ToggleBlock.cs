using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 可开关的方块：关掉时碰撞体失效、视觉隐藏，打开时恢复。
    /// 初始状态由 startActive 决定，Switch 只负责在切换时通知它。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ToggleBlock : MonoBehaviour, ISwitchTarget
    {
        [SerializeField] bool startActive = true;
        [SerializeField] bool visibleWhenInactive;
        [SerializeField] Color activeColor = new Color(0.48f, 0.72f, 0.42f, 1f);
        [SerializeField] Color inactiveColor = new Color(0.48f, 0.72f, 0.42f, 0.18f);

        SpriteRenderer sprite;
        Collider2D body;

        public bool Active { get; private set; }

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            body = GetComponent<Collider2D>();
            Apply(startActive);
        }

        public void SetActive(bool active) => Apply(active);

        void Apply(bool active)
        {
            Active = active;

            if (body != null) body.enabled = active;

            if (sprite == null) return;

            sprite.enabled = active || visibleWhenInactive;
            sprite.color = active ? activeColor : inactiveColor;
        }
    }
}
