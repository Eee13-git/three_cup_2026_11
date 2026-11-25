using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Data + position of one reveal rectangle. The fog shader reads the resolved
    /// rectangle every frame, so this component only has to describe it.
    ///
    /// 分光缝会把视窗拆成主、副两块：主块仍然照对面（尺寸缩小），
    /// 副块照自己脚下。两块共用同一个所属侧与尺寸配置。
    ///
    /// 折痕会把主窗口沿一条线镜像翻转，于是能看到折痕另一侧的高度。
    /// </summary>
    [ExecuteAlways]
    public sealed class VisionWindow : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;
        [SerializeField] Transform owner;
        [SerializeField] Vector2 offset = new Vector2(8f, 0f);

        [Header("Split")]
        [SerializeField] bool splitEnabled;

        [Tooltip("副窗口相对角色位置的偏移")]
        [SerializeField] Vector2 splitOffset = new Vector2(0f, 0.2f);

        [Tooltip("分裂后主窗口保留的尺寸比例")]
        [SerializeField, Range(0.2f, 1f)] float primaryScale = 0.65f;

        [Tooltip("副窗口的尺寸比例")]
        [SerializeField, Range(0.2f, 1f)] float secondaryScale = 0.55f;

        [Tooltip("副窗口高度比例")]
        [SerializeField, Range(0.2f, 1f)] float secondaryHeightScale = 0.85f;

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
        public bool SplitEnabled => splitEnabled;
        public bool HasCrease => hasCrease;

        /// <summary>主窗口实际尺寸：分裂时缩小、透镜生效时缩放。</summary>
        public Vector2 PrimarySize => Size * zoomSizeScale * (splitEnabled ? primaryScale : 1f);

        public float ZoomSizeScale => zoomSizeScale;

        public Vector2 SecondarySize =>
            new Vector2(Size.x * secondaryScale, Size.y * secondaryHeightScale);

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
            splitEnabled = false;
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

        /// <summary>分光缝踩下 / 踩回。</summary>
        public void SetSplit(bool value)
        {
            splitEnabled = value;
            Refresh();
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

        /// <summary>副窗口（照自己脚下）的中心。</summary>
        public Vector2 ResolveSecondaryCenter() =>
            owner == null ? (Vector2)transform.position : (Vector2)owner.position + splitOffset;

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

            if (!splitEnabled) return;

            Gizmos.color = new Color(0.55f, 0.85f, 1f, 0.7f);
            Gizmos.DrawWireCube(ResolveSecondaryCenter(), SecondarySize);
        }
    }
}
