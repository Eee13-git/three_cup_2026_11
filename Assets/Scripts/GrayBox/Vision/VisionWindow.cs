using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 一个透光口的位置与尺寸。遮蔽层 shader 每帧读取它算出的矩形，
    /// 所以这个组件只负责描述"现在该照哪儿"。
    ///
    /// 折痕会把窗口沿一条线镜像翻转，能看到棱另一侧的高度；
    /// 透镜会把窗口整体放大或缩小，同时改变观察距离。
    /// </summary>
    [ExecuteAlways]
    public sealed class VisionWindow : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;
        [SerializeField] Transform owner;
        [SerializeField] Vector2 offset = new Vector2(8f, 0f);

        [Header("Crease")]
        [Tooltip("视窗沿折痕翻过去（或翻回来）的过渡时长")]
        [SerializeField] float creaseFadeSeconds = 0.3f;

        [Header("Zoom")]
        [SerializeField] float zoomSizeScale = 1f;
        [SerializeField] float zoomOffsetScale = 1f;

        public PlayerSide Side => side;
        public Transform Owner => owner;
        public Vector2 Offset => offset;
        public Vector2 Size => new Vector2(transform.localScale.x, transform.localScale.y);
        public bool HasCrease => hasCrease;
        public float ZoomSizeScale => zoomSizeScale;

        /// <summary>主窗口实际尺寸：透镜生效时按比例缩放。</summary>
        public Vector2 PrimarySize => Size * zoomSizeScale;

        bool hasCrease;
        Vector2 creaseA;
        Vector2 creaseB;
        float creaseTarget;
        float creaseWeight;

        void Update()
        {
            float step = Time.deltaTime / Mathf.Max(creaseFadeSeconds, 0.01f);
            creaseWeight = Mathf.MoveTowards(creaseWeight, creaseTarget, step);
        }

        void LateUpdate() => Refresh();

        void OnValidate() => Refresh();

        public void Configure(Transform newOwner, Vector2 newOffset)
        {
            owner = newOwner;
            offset = newOffset;
            hasCrease = false;
            creaseTarget = 0f;
            creaseWeight = 0f;
            zoomSizeScale = 1f;
            zoomOffsetScale = 1f;
            Refresh();
        }

        public void Configure(Transform newOwner, Vector2 newOffset, PlayerSide newSide)
        {
            side = newSide;
            Configure(newOwner, newOffset);
        }

        public void SetSize(Vector2 size)
        {
            if (size.x <= 0f || size.y <= 0f) return;

            transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        /// <summary>
        /// 折痕生效时，主窗口沿这条线镜像翻转。
        /// weight 只是目标值，真正的过渡由 Update 里的 creaseWeight 平滑推进，
        /// 所以进出折痕时视窗是"翻过去"而不是瞬移。
        /// </summary>
        public void SetCrease(Vector2 a, Vector2 b, float weight = 1f)
        {
            hasCrease = true;
            creaseA = a;
            creaseB = b;
            creaseTarget = Mathf.Clamp01(weight);
        }

        /// <summary>离开折痕：只把目标权重归零，线先留着，让视窗平滑翻回来。</summary>
        public void ClearCrease() => creaseTarget = 0f;

        /// <summary>透镜：放大 / 缩小视窗，同时把观察距离一起缩放。</summary>
        public void SetZoom(float sizeScale, float offsetScale)
        {
            zoomSizeScale = Mathf.Max(0.1f, sizeScale);
            zoomOffsetScale = Mathf.Max(0.05f, offsetScale);
            Refresh();
        }

        /// <summary>主窗口（照对面，折痕生效时被镜像）的中心。</summary>
        public Vector2 ResolveCenter()
        {
            Vector2 center = FollowCenter();
            if (!hasCrease || creaseWeight <= 0.0001f) return center;

            return Vector2.Lerp(center, Reflect(center, creaseA, creaseB), creaseWeight);
        }

        Vector2 FollowCenter() =>
            owner == null ? (Vector2)transform.position : (Vector2)owner.position + offset * zoomOffsetScale;

        static Vector2 Reflect(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 direction = b - a;
            float length = direction.magnitude;
            if (length < 0.0001f) return point;

            Vector2 normal = new Vector2(-direction.y, direction.x) / length;
            return point - 2f * Vector2.Dot(point - a, normal) * normal;
        }

        public void Refresh()
        {
            Vector2 target = ResolveCenter();
            if (((Vector2)transform.position - target).sqrMagnitude < 0.000001f) return;

            transform.position = new Vector3(target.x, target.y, 0f);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.92f, 0.55f, 0.6f);
            Gizmos.DrawWireCube(ResolveCenter(), PrimarySize);
        }
    }
}
