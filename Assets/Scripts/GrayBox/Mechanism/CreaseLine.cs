using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>折痕作用范围：默认按折痕中点所在半屏判断，避免把对面角色的视窗也翻走。</summary>
    public enum CreaseOwnerFilter
    {
        Auto,
        Left,
        Right,
        Both
    }

    /// <summary>
    /// 折痕：纸被折过的一道棱。颜料精灵靠近时，透光口沿这道棱翻到另一侧，
    /// 于是能看到折过去的那个高度；走开就恢复原样。
    /// 折痕斜放时，视线也会跟着斜过去，所以能照到高台或坑底。
    /// </summary>
    public sealed class CreaseLine : MonoBehaviour
    {
        [Tooltip("角色离折痕多近开始生效（世界单位）")]
        [SerializeField] float influence = 2.5f;

        [Tooltip("折痕在本地空间的起点")]
        [SerializeField] Vector2 localStart = new Vector2(-3.3f, 0f);

        [Tooltip("折痕在本地空间的终点")]
        [SerializeField] Vector2 localEnd = new Vector2(3.3f, 0f);

        [Tooltip("这条折痕只翻哪一侧角色的视窗。Auto 按折痕中点所在半屏判断")]
        [SerializeField] CreaseOwnerFilter ownerFilter = CreaseOwnerFilter.Auto;

        [SerializeField] SpriteRenderer visual;

        VisionWindow[] windows;

        /// <summary>本折痕翻过的窗口。多条折痕同场时，只能清自己设置过的那几个，
        /// 否则另一条折痕会对"不属于自己"的窗口调用 ClearCrease，把折痕瞬间抹掉。</summary>
        readonly System.Collections.Generic.HashSet<VisionWindow> affected = new System.Collections.Generic.HashSet<VisionWindow>();

        public MechanismKind Kind => MechanismKind.Crease;

        void Awake()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (windows == null) windows = FindObjectsOfType<VisionWindow>();

            Vector2 a = transform.TransformPoint(localStart);
            Vector2 b = transform.TransformPoint(localEnd);
            float midX = (a.x + b.x) * 0.5f;

            for (int i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                if (window == null) continue;

                var owner = window.Owner;
                if (owner == null) continue;

                if (!AppliesTo(owner.position.x, midX))
                {
                    // 只清自己翻过的窗口，不去动另一条折痕负责的那一侧
                    if (affected.Remove(window)) window.ClearCrease();
                    continue;
                }

                float distance = DistanceToSegment(owner.position, a, b);
                if (distance <= influence)
                {
                    window.SetCrease(a, b);
                    affected.Add(window);
                }
                else if (affected.Remove(window))
                {
                    window.ClearCrease();
                }
            }
        }

        bool AppliesTo(float ownerX, float midX)
        {
            switch (ownerFilter)
            {
                case CreaseOwnerFilter.Left: return ownerX < 0f;
                case CreaseOwnerFilter.Right: return ownerX > 0f;
                case CreaseOwnerFilter.Both: return true;
                default: return (ownerX < 0f) == (midX < 0f);
            }
        }

        void OnDisable()
        {
            foreach (var window in affected)
                if (window != null) window.ClearCrease();

            affected.Clear();
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.000001f) return Vector2.Distance(point, a);

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq);
            return Vector2.Distance(point, a + ab * t);
        }

        public void Configure(float newInfluence, Vector2 start, Vector2 end)
        {
            influence = newInfluence;
            localStart = start;
            localEnd = end;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.55f, 0.8f);
            Gizmos.DrawLine(transform.TransformPoint(localStart), transform.TransformPoint(localEnd));
        }
    }
}
