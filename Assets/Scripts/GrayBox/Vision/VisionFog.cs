using System.Collections;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Drives the fog material with the rectangles of every vision window.
    /// 通关时还会用同一套"从若干点扩散"的逻辑把整层遮蔽擦掉。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VisionFog : MonoBehaviour
    {
        [SerializeField] VisionWindow[] windows = new VisionWindow[2];

        [Header("Window Edge")]
        [SerializeField] bool showWindowEdge = true;
        [SerializeField, Range(0f, 1f)] float windowEdgeStrength = 1f;

        [Header("Clear Dissolve")]
        [Tooltip("通关时遮蔽层被擦掉的时长")]
        [SerializeField] float dissolveDuration = 1.8f;

        [Tooltip("漏光缝的羽化（世界单位）。比视窗更大，边缘才像纸变薄而不是开了个口子")]
        [SerializeField] float holeFeather = 1.1f;

        [Tooltip("中间隔离墙所在位置，视窗不会越过这条线")]
        [SerializeField] float splitX;

        [SerializeField] float dissolveSoftness = 0.4f;

        [Tooltip("扩散起点，相对遮蔽层中心的偏移")]
        [SerializeField] Vector2[] dissolveSeedOffsets =
        {
            new Vector2(-6f, 3f),
            new Vector2(5f, -3f),
            new Vector2(-2f, -4f),
            new Vector2(4f, 4f)
        };

        static readonly int RectAId = Shader.PropertyToID("_RectA");
        static readonly int RectBId = Shader.PropertyToID("_RectB");
        static readonly int RectCId = Shader.PropertyToID("_RectC");
        static readonly int RectDId = Shader.PropertyToID("_RectD");
        // 名字与 shader 里的 _LightHoles 对应：Unity 按属性名缓存数组长度，
        // 换新名字才能在不重启编辑器的情况下把槽位从 4 扩到 12。
        static readonly int HolesId = Shader.PropertyToID("_LightHoles");
        static readonly int HoleFeatherId = Shader.PropertyToID("_HoleFeather");
        static readonly int EdgeStrengthId = Shader.PropertyToID("_EdgeStrength");
        static readonly int SeedsId = Shader.PropertyToID("_Seeds");
        static readonly int RadiiId = Shader.PropertyToID("_Radii");
        static readonly int DissolveSoftnessId = Shader.PropertyToID("_DissolveSoftness");
        static readonly Vector4 Disabled = new Vector4(9999f, 9999f, 0.001f, 0.001f);

        const int MaxSeeds = 4;

        /// <summary>
        /// 最多同时生效的洞数量，受 shader 数组长度限制（VisionFog.shader 的 _Holes）。
        /// 照面灯每盏占 2 个洞（对面光斑 + 脚下），所以 12 个槽位够 6 盏灯同时亮着。
        /// </summary>
        const int MaxHoles = 12;

        readonly Vector4[] seeds = new Vector4[MaxSeeds];
        readonly float[] radii = new float[MaxSeeds];
        readonly float[] seedMaxRadius = new float[MaxSeeds];
        readonly Vector4[] holes = new Vector4[MaxHoles];

        SpriteRenderer fog;
        MaterialPropertyBlock block;

        public bool IsDissolving { get; private set; }

        /// <summary>视窗描边的开关，留给以后的设置面板调用。</summary>
        public bool ShowWindowEdge
        {
            get => showWindowEdge;
            set
            {
                if (showWindowEdge == value) return;

                showWindowEdge = value;
                Push();
            }
        }

        void LateUpdate() => Push();

        void OnValidate() => Push();

        public void Configure(VisionWindow left, VisionWindow right)
        {
            windows = new[] { left, right };
            Push();
        }

        /// <summary>换关时把遮蔽层恢复原状（扩散进度与可见性都清掉）。</summary>
        public void ResetFog()
        {
            EnsureRefs();

            IsDissolving = false;
            for (int i = 0; i < MaxSeeds; i++) radii[i] = 0f;

            if (fog != null)
            {
                fog.enabled = true;
                Push();
            }
        }

        /// <summary>进关绘制时先把遮蔽层藏起来，绘制结束后由 ResetFog 恢复。</summary>
        public void SetVisible(bool visible)
        {
            EnsureRefs();
            if (fog == null) return;

            fog.enabled = visible;
        }

        /// <summary>从几个起点向外扩散，把整层遮蔽擦掉。</summary>
        public IEnumerator DissolveRoutine()
        {
            if (IsDissolving) yield break;

            EnsureRefs();
            if (fog == null) yield break;

            IsDissolving = true;
            PrepareDissolveSeeds();

            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime / Mathf.Max(dissolveDuration, 0.01f);
                UpdateDissolveRadii(Mathf.Clamp01(timer));
                yield return null;
            }

            UpdateDissolveRadii(1f);
            Push();
            fog.enabled = false;
        }

        void PrepareDissolveSeeds()
        {
            EnsureRefs();

            Bounds bounds = fog.bounds;
            Vector2 center = fog.transform.position;
            int count = Mathf.Min(dissolveSeedOffsets != null ? dissolveSeedOffsets.Length : 0, MaxSeeds);

            for (int i = 0; i < MaxSeeds; i++)
            {
                radii[i] = 0f;

                if (i >= count)
                {
                    seeds[i] = Vector4.zero;
                    seedMaxRadius[i] = 0f;
                    continue;
                }

                Vector2 p = center + dissolveSeedOffsets[i];
                seeds[i] = new Vector4(p.x, p.y, 0f, 0f);

                float max = 0f;
                Vector3 min = bounds.min, maxCorner = bounds.max;
                for (int c = 0; c < 4; c++)
                {
                    var corner = new Vector2(
                        (c & 1) == 0 ? min.x : maxCorner.x,
                        (c & 2) == 0 ? min.y : maxCorner.y);
                    max = Mathf.Max(max, Vector2.Distance(p, corner));
                }

                seedMaxRadius[i] = max + dissolveSoftness;
            }

            Push();
        }

        void UpdateDissolveRadii(float t)
        {
            for (int i = 0; i < MaxSeeds; i++)
            {
                if (seedMaxRadius[i] <= 0f) { radii[i] = 0f; continue; }

                float start = i * 0.08f;
                float local = Mathf.Clamp01((t - start) / Mathf.Max(0.01f, 1f - start));
                radii[i] = Mathf.Lerp(0f, seedMaxRadius[i], local);
            }
        }

        public void Push()
        {
            EnsureRefs();
            if (fog == null || block == null) return;

            fog.GetPropertyBlock(block);
            block.SetVector(RectAId, Pack(0));
            block.SetVector(RectBId, Pack(1));
            block.SetVector(RectCId, Pack(2));
            block.SetVector(RectDId, Pack(3));
            block.SetFloat(EdgeStrengthId, showWindowEdge ? Mathf.Clamp01(windowEdgeStrength) : 0f);
            block.SetVectorArray(SeedsId, seeds);
            block.SetFloatArray(RadiiId, radii);
            block.SetFloat(DissolveSoftnessId, dissolveSoftness);

            CollectHoles();
            block.SetVectorArray(HolesId, holes);
            block.SetFloat(HoleFeatherId, Mathf.Max(0.01f, holeFeather));

            fog.SetPropertyBlock(block);
        }

        /// <summary>远程点亮的可见区域，同样遵守中线裁剪。</summary>
        void CollectHoles()
        {
            int count = 0;
            var registry = VisionHoleRegistry.Items;

            for (int i = 0; i < registry.Count && count < MaxHoles; i++)
            {
                var hole = registry[i];
                if (hole == null || !hole.HoleActive) continue;

                Vector4 rect = PackRect(hole.HoleCenter, hole.HoleHalfSize);
                if (rect.z <= 0.002f) continue;

                holes[count++] = rect;
            }

            for (int i = count; i < MaxHoles; i++) holes[i] = Disabled;
        }

        /// <summary>
        /// 0/1 是左右主窗口，2/3 是分光缝展开后的副窗口。
        /// 主窗口在分裂时由 VisionWindow 自己缩小。
        /// </summary>
        Vector4 Pack(int index)
        {
            int windowIndex = index >> 1;
            bool secondary = (index & 1) == 1;

            // 分光缝已移除，2/3 号槽长期留空
            if (secondary) return Disabled;
            if (windows == null || windowIndex >= windows.Length || windows[windowIndex] == null)
                return Disabled;

            var window = windows[windowIndex];
            return PackRect(window.ResolveCenter(), window.PrimarySize * 0.5f);
        }

        Vector4 PackRect(Vector2 center, Vector2 half)
        {
            // 视窗不能越过中间的隔离墙：右侧窗口裁掉中线左边，左侧窗口裁掉右边，
            // 否则视窗贴到边界时会漏出对侧一小条地形。
            float minX = center.x - half.x;
            float maxX = center.x + half.x;

            if (center.x >= splitX) minX = Mathf.Max(minX, splitX);
            else maxX = Mathf.Min(maxX, splitX);

            float width = maxX - minX;
            if (width <= 0.001f) return Disabled;

            center.x = (minX + maxX) * 0.5f;

            return new Vector4(center.x, center.y, Mathf.Max(width * 0.5f, 0.001f), Mathf.Max(half.y, 0.001f));
        }

        void EnsureRefs()
        {
            if (fog == null) fog = GetComponent<SpriteRenderer>();
            if (block == null) block = new MaterialPropertyBlock();
        }
    }
}
