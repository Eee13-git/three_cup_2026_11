using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 分光缝：裱纸在这里被撕开一道分叉，透光口跟着一分为二——
    /// 一块仍然照着对面，另一块落在自己脚下。踩一下展开，再踩一下合上。
    /// 代价是两块都比原来小，看不全。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class SplitAperture : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;
        [SerializeField] SpriteRenderer visual;
        [SerializeField] Color idleColor = Color.white;
        [SerializeField] Color activeColor = new Color(0.62f, 0.86f, 1f, 1f);

        VisionWindow window;

        public MechanismKind Kind => MechanismKind.SplitAperture;
        public PlayerSide Side => side;
        public bool Active => window != null && window.SplitEnabled;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void Awake()
        {
            if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var motor = other.GetComponentInParent<PlayerMotor2D>();
            if (motor == null || motor.Side != side) return;

            EnsureWindow();
            if (window == null) return;

            window.SetSplit(!window.SplitEnabled);
            ApplyVisual(window.SplitEnabled);
        }

        void OnDisable() => ApplyVisual(false);

        void EnsureWindow()
        {
            if (window != null) return;

            foreach (var candidate in FindObjectsOfType<VisionWindow>())
            {
                if (candidate.Side != side) continue;

                window = candidate;
                return;
            }
        }

        void ApplyVisual(bool active)
        {
            if (visual == null) return;

            visual.color = active ? activeColor : idleColor;
        }

        public void Configure(PlayerSide newSide, SpriteRenderer newVisual)
        {
            side = newSide;
            visual = newVisual;
        }
    }
}
