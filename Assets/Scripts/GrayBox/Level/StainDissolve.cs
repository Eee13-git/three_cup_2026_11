using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 污渍的溶解动画：从若干个起点向外扩散，把贴图逐渐擦掉。
    /// 每个起点有各自的半径和起始延时，扩散完不等大，看起来才像手擦的。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class StainDissolve : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;

        [Tooltip("扩散起点，相对污渍中心的偏移（世界单位）")]
        [SerializeField] Vector2[] seedOffsets =
        {
            new Vector2(-0.22f, 0.06f),
            new Vector2(0.18f, 0.22f),
            new Vector2(0.12f, -0.22f)
        };

        [SerializeField] float duration = 0.7f;
        [SerializeField] float softness = 0.22f;

        [Tooltip("每次擦除时在污渍内部随机撒点，而不是固定几个位置")]
        [SerializeField] bool randomizeSeeds = true;

        [Tooltip("随机点的散布范围（占污渍尺寸的比例）")]
        [SerializeField, Range(0.1f, 1f)] float randomSpread = 0.6f;

        [Tooltip("相邻起点之间的起始延时（占整段的比例）")]
        [SerializeField] float stagger = 0.12f;

        static readonly int SeedsId = Shader.PropertyToID("_Seeds");
        static readonly int RadiiId = Shader.PropertyToID("_Radii");
        static readonly int SoftnessId = Shader.PropertyToID("_Softness");

        const int MaxSeeds = 4;

        readonly Vector4[] seeds = new Vector4[MaxSeeds];
        readonly float[] radii = new float[MaxSeeds];
        readonly float[] seedMaxRadius = new float[MaxSeeds];

        MaterialPropertyBlock block;
        float timer = -1f;

        public bool Playing => timer >= 0f;
        public bool Finished { get; private set; }

        public void Play()
        {
            if (Playing || Finished) return;
            if (target == null) target = GetComponent<SpriteRenderer>();

            PrepareSeeds();
            timer = 0f;
        }

        void PrepareSeeds()
        {
            if (block == null) block = new MaterialPropertyBlock();

            Bounds bounds = target.bounds;
            Vector2 center = target.transform.position;
            int count = Mathf.Min(seedOffsets != null ? seedOffsets.Length : 0, MaxSeeds);

            for (int i = 0; i < MaxSeeds; i++)
            {
                if (i < count)
                {
                    Vector2 p = randomizeSeeds ? RandomSeed(bounds, center, i) : center + seedOffsets[i];
                    seeds[i] = new Vector4(p.x, p.y, 0f, 0f);

                    // 该起点要扩散到污渍最远角，才算擦干净
                    float max = 0f;
                    Vector3 min = bounds.min, maxCorner = bounds.max;
                    for (int c = 0; c < 4; c++)
                    {
                        var corner = new Vector2(
                            (c & 1) == 0 ? min.x : maxCorner.x,
                            (c & 2) == 0 ? min.y : maxCorner.y);
                        max = Mathf.Max(max, Vector2.Distance(p, corner));
                    }

                    seedMaxRadius[i] = max + softness;
                }
                else
                {
                    seeds[i] = Vector4.zero;
                    seedMaxRadius[i] = 0f;
                }

                radii[i] = 0f;
            }

            target.GetPropertyBlock(block);
            block.SetVectorArray(SeedsId, seeds);
            block.SetFloatArray(RadiiId, radii);
            block.SetFloat(SoftnessId, softness);
            target.SetPropertyBlock(block);
        }

        /// <summary>
        /// 在污渍包围盒内随机取一个起点，并尽量和前几个点拉开距离，
        /// 免得三个点挤在一起、擦除看起来像只从一个地方散开。
        /// </summary>
        Vector2 RandomSeed(Bounds bounds, Vector2 center, int index)
        {
            Vector2 size = bounds.size;
            float minGap = Mathf.Min(size.x, size.y) * 0.22f;
            Vector2 point = center;

            for (int attempt = 0; attempt < 16; attempt++)
            {
                point = new Vector2(
                    center.x + Random.Range(-0.5f, 0.5f) * size.x * randomSpread,
                    center.y + Random.Range(-0.5f, 0.5f) * size.y * randomSpread);

                bool farEnough = true;
                for (int j = 0; j < index; j++)
                {
                    var other = new Vector2(seeds[j].x, seeds[j].y);
                    if ((point - other).sqrMagnitude < minGap * minGap) { farEnough = false; break; }
                }

                if (farEnough) break;
            }

            return point;
        }

        void Update()
        {
            if (!Playing || Finished) return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / Mathf.Max(duration, 0.01f));

            for (int i = 0; i < MaxSeeds; i++)
            {
                if (seedMaxRadius[i] <= 0f) { radii[i] = 0f; continue; }

                float start = i * stagger;
                float local = Mathf.Clamp01((t - start) / Mathf.Max(0.01f, 1f - start));
                radii[i] = Mathf.Lerp(0f, seedMaxRadius[i], local);
            }

            block.SetFloatArray(RadiiId, radii);
            target.SetPropertyBlock(block);

            if (t < 1f) return;

            Finished = true;
            target.enabled = false;
        }
    }
}
