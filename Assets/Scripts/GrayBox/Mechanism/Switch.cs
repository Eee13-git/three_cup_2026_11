using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 踩上去（或碰到）就驱动一组 ISwitchTarget 的开关。
    /// 目标可以放在另一半屏，这就是「跨屏开关」——踩下后对面那侧长出路来。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Switch : MonoBehaviour
    {
        [Tooltip("挂任意实现了 ISwitchTarget 的组件，可以跨屏引用")]
        [SerializeField] MonoBehaviour[] targets;

        [Tooltip("按下后保持，不再响应离开")]
        [SerializeField] bool oneShot = true;

        [Tooltip("可反复触发时，离开是否复原")]
        [SerializeField] bool revertOnExit;

        [SerializeField] Color idleColor = new Color(0.75f, 0.62f, 0.30f, 1f);
        [SerializeField] Color pressedColor = new Color(0.35f, 0.90f, 0.60f, 1f);

        SpriteRenderer sprite;
        int occupants;
        bool pressed;

        public bool Pressed => pressed;

        void Awake() => sprite = GetComponentInChildren<SpriteRenderer>();

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMotor2D>() == null) return;

            occupants++;
            SetPressed(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMotor2D>() == null) return;

            occupants = Mathf.Max(0, occupants - 1);
            if (oneShot || !revertOnExit || occupants > 0) return;

            SetPressed(false);
        }

        void SetPressed(bool value)
        {
            // 一次性开关按下后不再改变任何目标
            if (oneShot && pressed) return;
            if (pressed == value) return;

            pressed = value;

            if (sprite != null) sprite.color = pressed ? pressedColor : idleColor;

            if (targets == null) return;

            foreach (var target in targets)
            {
                if (target is ISwitchTarget switchTarget) switchTarget.SetActive(pressed);
            }
        }

        public void Configure(MonoBehaviour[] newTargets, bool isOneShot, bool revert)
        {
            targets = newTargets;
            oneShot = isOneShot;
            revertOnExit = revert;
        }
    }
}
