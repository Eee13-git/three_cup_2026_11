using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 角色走过、站过的地方会留下水渍：踩得越久越深，离开后逐渐变淡。
    /// 水渍排序在地形之上、遮蔽层之下，所以只有透过视窗才看得见。
    /// 全部点位池化复用，换关时统一清空。
    /// </summary>
    public sealed class FootprintSystem : MonoBehaviour
    {
        [Header("Spawning")]
        [Tooltip("走多远留下一个点（世界单位）")]
        [SerializeField] float spawnDistance = 0.3f;

        [Tooltip("水渍半径")]
        [SerializeField] float radius = 0.28f;

        [Tooltip("最多同时存在的点数")]
        [SerializeField] int maxFootprints = 120;

        [Header("Look")]
        [Tooltip("左角色留下的痕迹颜色（取其角色色的加深版）")]
        [SerializeField] Color leftTint = new Color(0.63f, 0.55f, 0.48f, 1f);

        [Tooltip("右角色留下的痕迹颜色")]
        [SerializeField] Color rightTint = new Color(0.56f, 0.57f, 0.55f, 1f);

        [Tooltip("单个点最深的不透明度")]
        [SerializeField, Range(0.01f, 1f)] float maxAlpha = 0.6f;

        [Tooltip("刚踩出来时的深度，太低会让走动痕迹几乎看不见")]
        [SerializeField, Range(0f, 1f)] float spawnIntensity = 0.7f;

        [Tooltip("排序：压在遮蔽层之上、角色之下")]
        [SerializeField] int sortingOrder = GrayBoxSorting.FogGlow;

        [Header("Timing")]
        [Tooltip("站在同一点多久达到最深")]
        [SerializeField] float growSeconds = 1.4f;

        [Tooltip("离开后多久完全消失")]
        [SerializeField] float fadeSeconds = 2.4f;

        class Print
        {
            public SpriteRenderer renderer;
            public Transform transform;
            public Vector2 position;
            public float intensity;
            public bool touched;
            public Color color;
        }

        static Sprite circleSprite;

        readonly List<Print> prints = new List<Print>();
        readonly Vector2[] lastSpawn = new Vector2[2];
        PlayerMotor2D[] players;
        bool[] hasSpawn;

        public int Count => prints.Count;

        void Awake()
        {
            hasSpawn = new bool[2];

            for (int i = 0; i < maxFootprints; i++)
                prints.Add(CreatePrint("Footprint_" + i));
        }

        void Update()
        {
            EnsurePlayers();

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // 1) 走动时在脚下补点
            foreach (var player in players)
            {
                if (player == null || !player.Grounded) continue;

                int index = player.Side == PlayerSide.Left ? 0 : 1;
                Vector2 foot = FootPosition(player);

                if (!hasSpawn[index] || (foot - lastSpawn[index]).sqrMagnitude > spawnDistance * spawnDistance)
                {
                    Spawn(foot, player.Side == PlayerSide.Left ? leftTint : rightTint);
                    lastSpawn[index] = foot;
                    hasSpawn[index] = true;
                }
            }

            // 2) 站在附近的点会被踩深，其余逐渐变淡
            foreach (var print in prints) print.touched = false;

            foreach (var player in players)
            {
                if (player == null || !player.Grounded) continue;

                Vector2 foot = FootPosition(player);
                float sqrRadius = radius * radius;

                for (int i = 0; i < prints.Count; i++)
                {
                    var print = prints[i];
                    if (print.intensity <= 0f) continue;
                    if ((print.position - foot).sqrMagnitude > sqrRadius) continue;

                    print.touched = true;
                    print.intensity = Mathf.Min(1f, print.intensity + dt / Mathf.Max(growSeconds, 0.01f));
                    Apply(print);
                }
            }

            for (int i = 0; i < prints.Count; i++)
            {
                var print = prints[i];
                if (print.touched || print.intensity <= 0f) continue;

                print.intensity -= dt / Mathf.Max(fadeSeconds, 0.01f);
                if (print.intensity <= 0.001f)
                {
                    print.intensity = 0f;
                    print.renderer.enabled = false;
                }
                else
                {
                    Apply(print);
                }
            }
        }

        Vector2 FootPosition(PlayerMotor2D player) =>
            player.transform.position;

        void EnsurePlayers()
        {
            if (players != null && players.Length == 2 && players[0] != null && players[1] != null) return;

            players = FindObjectsOfType<PlayerMotor2D>();
        }

        void Spawn(Vector2 position, Color color)
        {
            Print target = null;

            // 先找空闲点，没有就复用最淡的
            float weakest = 2f;
            Print weakestPrint = null;

            for (int i = 0; i < prints.Count; i++)
            {
                var print = prints[i];
                if (print.intensity <= 0f) { target = print; break; }

                if (print.intensity < weakest)
                {
                    weakest = print.intensity;
                    weakestPrint = print;
                }
            }

            if (target == null) target = weakestPrint;
            if (target == null) return;

            target.position = position;
            target.color = color;
            target.intensity = Mathf.Max(target.intensity, spawnIntensity);
            target.touched = true;

            target.transform.position = new Vector3(position.x, position.y, 0f);
            target.renderer.enabled = true;

            Apply(target);
        }

        void Apply(Print print)
        {
            float alpha = maxAlpha * Mathf.Clamp01(print.intensity);
            print.renderer.color = new Color(print.color.r, print.color.g, print.color.b, alpha);
        }

        Print CreatePrint(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.sortingOrder = sortingOrder;
            sr.color = new Color(1f, 1f, 1f, 0f);
            sr.enabled = false;

            go.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

            return new Print
            {
                renderer = sr,
                transform = go.transform,
                intensity = 0f
            };
        }

        static Sprite GetCircleSprite()
        {
            if (circleSprite != null) return circleSprite;

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "FootprintCircle"
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(1f - distance);
                alpha *= alpha;

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            circleSprite.name = "FootprintCircle";
            return circleSprite;
        }

        /// <summary>换关时清空所有水渍。</summary>
        public void ClearAll()
        {
            foreach (var print in prints)
            {
                print.intensity = 0f;
                print.touched = false;
                print.renderer.enabled = false;
            }

            if (hasSpawn != null)
                for (int i = 0; i < hasSpawn.Length; i++) hasSpawn[i] = false;
        }
    }
}
