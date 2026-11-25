using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 照面灯：踩下去点亮对面半屏对应的一块区域。
    /// 点亮一次后本关内常亮，不再熄灭——所以它是"开启视野"而不是"切换视野"。
    /// 多盏可以同时亮着，但 VisionFog 最多显示 4 个洞。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class BeaconSwitch : MonoBehaviour, IVisionHole
    {
        [Tooltip("点亮区域的尺寸（世界单位）")]
        [SerializeField] Vector2 targetSize = new Vector2(3.4f, 2.2f);

        [Tooltip("把灯的位置关于中缝镜像后作为目标位置")]
        [SerializeField] bool mirrorAcrossCenter = true;

        [Tooltip("关闭镜像时使用的手动目标位置")]
        [SerializeField] Vector2 targetCenter;

        [Tooltip("在目标位置上的微调")]
        [SerializeField] Vector2 targetOffset;

        [Header("Visual")]
        [SerializeField] SpriteRenderer lampVisual;
        [SerializeField] SpriteRenderer glow;
        [SerializeField] SpriteRenderer localGlow;
        [SerializeField] Color idleColor = new Color(0.62f, 0.60f, 0.55f, 1f);
        [SerializeField] Color litColor = new Color(1f, 0.85f, 0.52f, 1f);

        bool lit;

        public MechanismKind Kind => MechanismKind.Beacon;
        public bool Lit => lit;

        public bool HoleActive => lit;

        public Vector2 HoleCenter =>
            mirrorAcrossCenter
                ? new Vector2(-transform.position.x, transform.position.y) + targetOffset
                : targetCenter + targetOffset;

        public Vector2 HoleHalfSize => targetSize * 0.5f;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void Awake()
        {
            if (lampVisual == null) lampVisual = GetComponent<SpriteRenderer>();
        }

        void OnEnable() => VisionHoleRegistry.Register(this);

        void OnDisable() => VisionHoleRegistry.Unregister(this);

        void OnTriggerEnter2D(Collider2D other)
        {
            if (lit) return;
            if (other.GetComponentInParent<PlayerMotor2D>() == null) return;

            lit = true;
        }

        void Update()
        {
            if (lampVisual != null) lampVisual.color = lit ? litColor : idleColor;

            if (glow != null)
            {
                glow.enabled = lit;
                if (lit) glow.transform.position = HoleCenter;
            }

            if (localGlow != null) localGlow.enabled = lit;
        }

        public void Configure(Vector2 size, bool mirror, SpriteRenderer visual, SpriteRenderer glowVisual, SpriteRenderer localGlowVisual)
        {
            targetSize = size;
            mirrorAcrossCenter = mirror;
            lampVisual = visual;
            glow = glowVisual;
            localGlow = localGlowVisual;
        }
    }
}
