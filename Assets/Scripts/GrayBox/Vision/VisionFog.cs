using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Drives the fog material with the rectangles of every vision window.
    /// The shader turns those rectangles into soft-edged holes, which SpriteMask
    /// cannot do because stencil is binary.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VisionFog : MonoBehaviour
    {
        [SerializeField] VisionWindow[] windows = new VisionWindow[2];

        static readonly int RectAId = Shader.PropertyToID("_RectA");
        static readonly int RectBId = Shader.PropertyToID("_RectB");
        static readonly Vector4 Disabled = new Vector4(9999f, 9999f, 0.001f, 0.001f);

        SpriteRenderer fog;
        MaterialPropertyBlock block;

        void LateUpdate() => Push();

        void OnValidate() => Push();

        public void Configure(VisionWindow left, VisionWindow right)
        {
            windows = new[] { left, right };
            Push();
        }

        public void Push()
        {
            EnsureRefs();
            if (fog == null || block == null) return;

            fog.GetPropertyBlock(block);
            block.SetVector(RectAId, Pack(0));
            block.SetVector(RectBId, Pack(1));
            fog.SetPropertyBlock(block);
        }

        Vector4 Pack(int index)
        {
            if (windows == null || index >= windows.Length || windows[index] == null) return Disabled;

            var window = windows[index];
            Vector2 center = window.ResolveCenter();
            Vector2 half = window.Size * 0.5f;

            return new Vector4(center.x, center.y, Mathf.Max(half.x, 0.001f), Mathf.Max(half.y, 0.001f));
        }

        void EnsureRefs()
        {
            if (fog == null) fog = GetComponent<SpriteRenderer>();
            if (block == null) block = new MaterialPropertyBlock();
        }
    }
}
