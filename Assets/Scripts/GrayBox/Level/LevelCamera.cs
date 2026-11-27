using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 本关的摄像机画幅。挂在关卡预制体根节点上，运行时由 ViewFitter 读取：
    /// size 就是 Camera 组件的 Size（正交尺寸），背景层与左右物理墙会按它自动适配。
    /// </summary>
    public sealed class LevelCamera : MonoBehaviour
    {
        [Tooltip("本关摄像机的 Size（正交尺寸）。1920x1080 下 4.5 对应 16x9 的世界视野。")]
        [SerializeField] float size = 4.5f;

        [Tooltip("左右物理墙放在屏幕边缘之外多少（世界单位），给角色留一点出屏余量。")]
        [SerializeField] float boundsMargin = 0.2f;

        [Tooltip("在 Scene 视图里标出本关画幅（摄像机能看到的世界范围），搭关卡时对齐平台用。仅编辑器可见。")]
        [SerializeField] bool showFrameGizmo = true;

        public float Size => size;
        public float BoundsMargin => boundsMargin;

        public void Configure(float newSize, float newMargin)
        {
            size = Mathf.Max(0.5f, newSize);
            boundsMargin = newMargin;
        }

        /// <summary>
        /// 始终在 Scene 视图里画出本关画幅：摄像机固定在 (0,0) 的设定，框的边界就是玩家实际能看到的范围。
        /// 穿透绘制（zTest Always），不会被地形挡住；Prefab 模式下没有主相机，按 16:9 估算。
        /// </summary>
        void OnDrawGizmos()
        {
            if (!showFrameGizmo) return;

            float aspect = ResolveAspect();
            float height = size * 2f;
            float width = height * aspect;
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float wallX = halfWidth + boundsMargin;

            var bl = new Vector3(-halfWidth, -halfHeight, 0f);
            var tl = new Vector3(-halfWidth, halfHeight, 0f);
            var tr = new Vector3(halfWidth, halfHeight, 0f);
            var br = new Vector3(halfWidth, -halfHeight, 0f);

#if UNITY_EDITOR
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            // 画幅底色 + 细边框
            Handles.DrawSolidRectangleWithOutline(
                new[] { bl, tl, tr, br },
                new Color(0.35f, 0.7f, 1f, 0.04f),
                new Color(0.35f, 0.7f, 1f, 0.95f));

            // 四角加粗，边界一眼能认出来
            float bracket = Mathf.Min(1.2f, Mathf.Min(width, height) * 0.1f);
            Handles.color = new Color(0.1f, 0.45f, 1f, 1f);
            Handles.DrawAAPolyLine(4f, bl + Vector3.right * bracket, bl, bl + Vector3.up * bracket);
            Handles.DrawAAPolyLine(4f, tl + Vector3.right * bracket, tl, tl + Vector3.down * bracket);
            Handles.DrawAAPolyLine(4f, tr + Vector3.left * bracket, tr, tr + Vector3.down * bracket);
            Handles.DrawAAPolyLine(4f, br + Vector3.left * bracket, br, br + Vector3.up * bracket);

            // 左右物理墙将要落到的位置（ViewFitter 按画幅宽度 + 余量摆放）
            Handles.color = new Color(1f, 0.55f, 0.2f, 0.85f);
            Handles.DrawDottedLine(new Vector3(-wallX, -halfHeight, 0f), new Vector3(-wallX, halfHeight, 0f), 4f);
            Handles.DrawDottedLine(new Vector3(wallX, -halfHeight, 0f), new Vector3(wallX, halfHeight, 0f), 4f);

            // 中缝
            Handles.color = new Color(1f, 1f, 1f, 0.3f);
            Handles.DrawDottedLine(Vector3.zero, new Vector3(0f, halfHeight, 0f), 6f);

            Handles.color = Color.white;
            Handles.Label(new Vector3(-halfWidth, halfHeight + 0.3f, 0f),
                $"Size {size:0.##}   画幅 {width:0.#}×{height:0.#}   墙 ±{wallX:0.##}");

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
#endif
        }

        static float ResolveAspect()
        {
            var cam = Camera.main;
            return cam != null && cam.aspect > 0.001f ? cam.aspect : 16f / 9f;
        }
    }
}
