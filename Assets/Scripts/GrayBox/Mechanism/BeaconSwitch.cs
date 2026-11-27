using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 照面灯：踩下去点亮对面半屏对应的一块区域。
    /// 点亮一次后本关内常亮，不再熄灭——所以它是"开启视野"而不是"切换视野"。
    /// 多盏可以同时亮着，VisionFog 的洞槽位够 6 盏灯同时生效。
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

        [Tooltip("手动摆在对面半屏的光斑预制体实例。绑了它之后，灯只负责开关这个光斑，不再自动把子物体挪到对面，位置和大小都由摆放决定")]
        [SerializeField] GameObject remoteGlow;

        [Tooltip("未绑定 remoteGlow 时的旧做法：把子物体光斑自动挪到镜像位置")]
        [SerializeField] SpriteRenderer glow;

        [SerializeField] SpriteRenderer localGlow;
        [SerializeField] Color idleColor = new Color(0.62f, 0.60f, 0.55f, 1f);
        [SerializeField] Color litColor = new Color(1f, 0.85f, 0.52f, 1f);

        bool lit;

        public MechanismKind Kind => MechanismKind.Beacon;
        public bool Lit => lit;

        public bool HoleActive => lit;

        public Vector2 HoleCenter
        {
            get
            {
                // 绑定了手动摆放的光斑：视野就开在光斑当前位置
                if (remoteGlow != null) return (Vector2)remoteGlow.transform.position + targetOffset;

                return mirrorAcrossCenter
                    ? new Vector2(-transform.position.x, transform.position.y) + targetOffset
                    : targetCenter + targetOffset;
            }
        }

        public Vector2 HoleHalfSize
        {
            get
            {
                // 同步使用光斑的实际尺寸（含缩放），保证亮的地方就是能看见的地方
                if (remoteGlow != null)
                {
                    var renderer = remoteGlow.GetComponentInChildren<SpriteRenderer>();
                    if (renderer != null) return (Vector2)renderer.bounds.size * 0.5f;
                }

                return targetSize * 0.5f;
            }
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void Awake()
        {
            if (lampVisual == null) lampVisual = GetComponent<SpriteRenderer>();

            // 手动摆放的光斑默认关掉，点亮时才出现
            if (remoteGlow != null) remoteGlow.SetActive(false);
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

            if (remoteGlow != null)
            {
                // 光斑由场景里手动摆放，灯只负责开关；旧的子物体光斑要显式关掉
                if (glow != null) glow.enabled = false;
                if (remoteGlow.activeSelf != lit) remoteGlow.SetActive(lit);
            }
            else if (glow != null)
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

        public void SetRemoteGlow(GameObject glowObject)
        {
            remoteGlow = glowObject;
            if (remoteGlow != null) remoteGlow.SetActive(false);
        }
    }
}
