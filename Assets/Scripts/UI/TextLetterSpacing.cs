using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// 给 UGUI Text 加字间距（tracking）。UGUI 原生不支持，只能改顶点。
    /// 只处理单行文本，够美术稿里的标题/提示/明细用。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class TextLetterSpacing : BaseMeshEffect
    {
        [SerializeField] float spacing = 4f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount < 8 || Mathf.Approximately(spacing, 0f)) return;

            var text = graphic as Text;
            if (text == null) return;
            if (text.cachedTextGenerator != null && text.cachedTextGenerator.lines.Count > 1) return;

            // Text 每个字符一个四边形（4 个顶点），多行/富文本标记不在这个范围内
            int charCount = vh.currentVertCount / 4;
            if (charCount <= 1) return;

            float anchor = AlignmentAnchor(text.alignment);
            float pivotShift = (charCount - 1) * spacing * anchor;

            var vertex = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                vertex.position.x += i / 4 * spacing - pivotShift;
                vh.SetUIVertex(vertex, i);
            }
        }

        static float AlignmentAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.LowerLeft:
                    return 0f;
                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    return 1f;
                default:
                    return 0.5f;
            }
        }
    }
}
