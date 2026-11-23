using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Data + position of one reveal rectangle. The fog shader reads the resolved
    /// rectangle every frame, so this component only has to describe it.
    ///
    /// 分光缝会把视窗拆成主、副两块：主块仍然照对面（尺寸缩小），
    /// 副块照自己脚下。两块共用同一个所属侧与尺寸配置。
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

        public PlayerSide Side => side;
        public Transform Owner => owner;
        public Vector2 Offset => offset;
        public Vector2 Size => new Vector2(transform.localScale.x, transform.localScale.y);
        public bool SplitEnabled => splitEnabled;

        /// <summary>主窗口实际尺寸：分裂时缩小，避免两扇窗加起来比原来还大。</summary>
        public Vector2 PrimarySize => splitEnabled ? Size * primaryScale : Size;

        public Vector2 SecondarySize =>
            new Vector2(Size.x * secondaryScale, Size.y * secondaryHeightScale);

        void LateUpdate() => Refresh();

        void OnValidate() => Refresh();

        public void Configure(Transform newOwner, Vector2 newOffset)
        {
            owner = newOwner;
            offset = newOffset;
            splitEnabled = false;
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

        /// <summary>主窗口（照对面）的中心。</summary>
        public Vector2 ResolveCenter() =>
            owner == null ? (Vector2)transform.position : (Vector2)owner.position + offset;

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
