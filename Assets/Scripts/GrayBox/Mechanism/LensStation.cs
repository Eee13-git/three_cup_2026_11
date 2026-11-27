using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 透镜（露珠）：画里草叶上凝着的一滴水。踩一下让透光口放大，
    /// 再踩缩成一小块，第三次恢复。
    /// 透光口的总面积有限——自己放大时，对面的窗口会同时缩小；
    /// 自己缩小时，对面反而放大。所以用之前要先想清楚谁更需要视野。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class LensStation : MonoBehaviour
    {
        enum Mode { Normal, ZoomIn, ZoomOut }

        [SerializeField] PlayerSide side = PlayerSide.Left;

        [Header("Zoom")]
        [Tooltip("放大时的视窗尺寸倍率与偏移倍率")]
        [SerializeField] Vector2 zoomInScale = new Vector2(1.35f, 1f);

        [Tooltip("缩小时的视窗尺寸倍率与偏移倍率")]
        [SerializeField] Vector2 zoomOutScale = new Vector2(0.7f, 1f);

        [Header("Visual")]
        [SerializeField] SpriteRenderer visual;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color zoomInColor = new Color(0.70f, 0.86f, 1f, 1f);
        [SerializeField] Color zoomOutColor = new Color(0.78f, 1f, 0.84f, 1f);

        Mode mode;
        VisionWindow window;

        public MechanismKind Kind => MechanismKind.Lens;
        public PlayerSide Side => side;

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

            mode = mode == Mode.Normal ? Mode.ZoomIn
                 : mode == Mode.ZoomIn ? Mode.ZoomOut
                 : Mode.Normal;

            Apply();
        }

        void Apply()
        {
            ApplyZoom(window, mode);

            // 一边放大，另一边就缩小
            var other = OtherWindow();
            ApplyZoom(other, mode == Mode.ZoomIn ? Mode.ZoomOut
                              : mode == Mode.ZoomOut ? Mode.ZoomIn
                              : Mode.Normal);

            if (visual == null) return;

            visual.color = mode == Mode.ZoomIn ? zoomInColor
                         : mode == Mode.ZoomOut ? zoomOutColor
                         : normalColor;
        }

        void ApplyZoom(VisionWindow target, Mode value)
        {
            if (target == null) return;

            switch (value)
            {
                case Mode.ZoomIn:
                    target.SetZoom(zoomInScale.x, zoomInScale.y);
                    break;
                case Mode.ZoomOut:
                    target.SetZoom(zoomOutScale.x, zoomOutScale.y);
                    break;
                default:
                    target.SetZoom(1f, 1f);
                    break;
            }
        }

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

        VisionWindow OtherWindow()
        {
            foreach (var candidate in FindObjectsOfType<VisionWindow>())
            {
                if (candidate.Side == side) continue;
                return candidate;
            }

            return null;
        }

        void OnDisable()
        {
            if (window != null) window.SetZoom(1f, 1f);

            var other = OtherWindow();
            if (other != null) other.SetZoom(1f, 1f);
        }

        public void Configure(PlayerSide newSide, SpriteRenderer newVisual)
        {
            side = newSide;
            visual = newVisual;
        }
    }
}
