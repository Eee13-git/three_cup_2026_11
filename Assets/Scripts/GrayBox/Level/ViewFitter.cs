using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 每关加载时把「本关摄像机 Size」应用到主相机，并让背景层和左右物理墙跟着适配：
    /// - 相机 orthographicSize = LevelCamera.Size，位置固定在 (0,0)
    /// - 三层背景按 size 换算出的世界视野铺满（高 = 2×size，宽 = 高 × 屏幕宽高比）
    /// - 左右墙贴住视野边缘并向外留 boundsMargin，中央墙保持在 0
    /// 没有 LevelCamera 的关卡按 Size 4.5 / 余量 0.2 处理；运行中手动改相机 Size 也会自动重排。
    /// </summary>
    public sealed class ViewFitter : MonoBehaviour
    {
        const float CanvasSpriteAspect = 1.13f;   // 空白画布贴图在 1 单位宽下的高度
        const float DefaultSize = 4.5f;
        const float DefaultMargin = 0.2f;
        const float DesignAspect = 16f / 9f;      // 关卡的设计画幅比

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Ensure();
        }

        // 菜单流程会连续切场景（Start → Select → 关卡），第一个场景里建好的实例跟着旧场景一起被销毁，
        // 所以每加载一个场景都要确认还有一个实例在跑，否则关卡里的 LevelCamera 没人读。
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Ensure();

        static void Ensure()
        {
            if (FindObjectOfType<ViewFitter>() != null) return;

            var go = new GameObject("ViewFitter");
            go.AddComponent<ViewFitter>();
        }

        LevelSession session;
        GameObject appliedLevel;
        LevelCamera levelCamera;
        float margin = DefaultMargin;
        float appliedSize = -1f;
        bool logged;

        Camera cam;
        float lastSize = -1f;
        float lastAspect = -1f;

        void Update()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();
            if (session == null) return;

            if (session.Instance != appliedLevel)
            {
                appliedLevel = session.Instance;
                levelCamera = appliedLevel != null ? appliedLevel.GetComponentInChildren<LevelCamera>(true) : null;
                appliedSize = -1f;
                logged = false;
                ApplyRandomBackdrops();
                ApplyLevelCamera();
            }

            // 运行时直接改关卡实例上的 Size 也立刻生效（不改预制体时最方便的调试方式）
            if (levelCamera != null && (!Mathf.Approximately(levelCamera.Size, appliedSize) || !Mathf.Approximately(levelCamera.BoundsMargin, margin)))
            {
                ApplyLevelCamera();
            }

            if (cam == null)
            {
                cam = Camera.main;
                lastSize = -1f;
            }

            if (cam == null) return;

            // 窗口比例一变就要按「设计宽度」重算正交尺寸，
            // 否则窄窗口（16:10、4:3）会把关卡两端切出画面，站在边缘的角色就"消失"了
            if (!Mathf.Approximately(cam.aspect, lastAspect))
            {
                ApplyLevelCamera();
            }

            if (!Mathf.Approximately(cam.orthographicSize, lastSize) || !Mathf.Approximately(cam.aspect, lastAspect))
            {
                lastSize = cam.orthographicSize;
                lastAspect = cam.aspect;
                Refit();
            }
        }

        /// <summary>
        /// 关卡都按 16:9 设计。窗口更窄时放大正交尺寸，让横向视野保持设计宽度不变，
        /// 代价是上下会多露出一点背景——总比关卡两端被切掉好。
        /// </summary>
        static float FitSize(float size, float aspect)
        {
            if (aspect <= 0.01f) return size;
            return aspect < DesignAspect ? size * DesignAspect / aspect : size;
        }

        /// <summary>
        /// 每关换一对斑驳背景：从图集里随机抽两张，左右各一张。
        /// 图集缺失时就保留场景里原来的贴图。
        /// </summary>
        void ApplyRandomBackdrops()
        {
            var library = BackdropLibrary.Load();
            if (library == null) return;

            var pair = library.PickTwo();
            if (pair.Length < 2) return;

            var left = FindTransform("Backdrop_Left");
            var right = FindTransform("Backdrop_Right");

            if (left != null)
            {
                var sr = left.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = pair[0];
            }

            if (right != null)
            {
                var sr = right.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = pair[1];
            }
        }

        void ApplyLevelCamera()
        {
            if (cam == null) cam = Camera.main;

            float size = levelCamera != null ? levelCamera.Size : DefaultSize;
            margin = levelCamera != null ? levelCamera.BoundsMargin : DefaultMargin;
            appliedSize = size;

            if (cam != null)
            {
                cam.orthographicSize = FitSize(size, cam.aspect);
                var position = cam.transform.position;
                position.x = 0f;
                position.y = 0f;
                cam.transform.position = position;
                lastSize = cam.orthographicSize;
                lastAspect = cam.aspect;
            }

            if (!logged)
            {
                logged = true;
                var id = session != null && session.Current != null ? session.Current.LevelId : "?";
                Debug.Log($"[GrayBox] {id} 摄像机 Size={size:0.##} 余量={margin:0.##} 实际正交={cam?.orthographicSize:0.##}");
            }

            Refit();
        }

        void Refit()
        {
            if (cam == null) return;

            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            float halfWidth = width * 0.5f;

            FitStrip("Backdrop_Left", -halfWidth * 0.5f, halfWidth, height);
            FitStrip("Backdrop_Right", halfWidth * 0.5f, halfWidth, height);

            // 空白画布：sprite 高 1.13，缩放要按比例换算
            FitCanvas("CanvasBlank_Left", -halfWidth * 0.5f, halfWidth, height);
            FitCanvas("CanvasBlank_Right", halfWidth * 0.5f, halfWidth, height);

            var fog = FindTransform("Fog");
            if (fog != null)
            {
                fog.localScale = new Vector3(width * 1.5f, height * 1.55f, 1f);
                fog.localPosition = new Vector3(0f, 0f, fog.localPosition.z);
            }

            PlaceWalls(halfWidth, height);
        }

        void FitStrip(string name, float x, float width, float height)
        {
            var t = FindTransform(name);
            if (t == null) return;

            t.localPosition = new Vector3(x, 0f, t.localPosition.z);

            var sr = t.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            // 斑驳背景是 Simple 采样模式，设 sr.size 不会生效，只能按贴图原始尺寸做缩放
            if (sr.drawMode != SpriteDrawMode.Simple)
            {
                t.localScale = Vector3.one;
                sr.size = new Vector2(width, height);
                return;
            }

            var sprite = sr.sprite;
            if (sprite == null) return;

            Vector2 baseSize = sprite.bounds.size;
            if (baseSize.x <= 0.0001f || baseSize.y <= 0.0001f) return;

            var parent = t.parent;
            Vector3 parentScale = parent != null ? parent.lossyScale : Vector3.one;
            t.localScale = new Vector3(
                width / (baseSize.x * Mathf.Max(0.0001f, Mathf.Abs(parentScale.x))),
                height / (baseSize.y * Mathf.Max(0.0001f, Mathf.Abs(parentScale.y))),
                1f);
        }

        void FitCanvas(string name, float x, float width, float height)
        {
            var t = FindTransform(name);
            if (t == null) return;

            t.localPosition = new Vector3(x, 0f, t.localPosition.z);
            t.localScale = new Vector3(width, height / CanvasSpriteAspect, 1f);
        }

        void PlaceWalls(float halfWidth, float height)
        {
            float edge = halfWidth + margin;
            float wallHeight = Mathf.Max(height + 3f, 6f);

            var left = FindTransform("Wall_LeftEdge");
            if (left != null) left.localPosition = new Vector3(-edge, left.localPosition.y, left.localPosition.z);

            var right = FindTransform("Wall_RightEdge");
            if (right != null) right.localPosition = new Vector3(edge, right.localPosition.y, right.localPosition.z);

            var center = FindTransform("Wall_Center");
            if (center != null) center.localPosition = new Vector3(0f, center.localPosition.y, center.localPosition.z);

            foreach (var name in new[] { "Wall_LeftEdge", "Wall_RightEdge", "Wall_Center" })
            {
                var t = FindTransform(name);
                if (t == null) continue;

                var scale = t.localScale;
                scale.y = wallHeight;
                t.localScale = scale;
            }
        }

        static Transform FindTransform(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.transform : null;
        }
    }
}
