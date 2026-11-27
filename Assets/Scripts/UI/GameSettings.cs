using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// 全局游戏设置单例：主音量 / BGM / SFX / 亮度 / 全屏，PlayerPrefs 持久化。
    /// 音量目前只做存档 + 预留读取接口，音频系统接入后直接读这些属性即可。
    /// 亮度用一张常驻的全屏黑色遮罩层实现（只能压暗，不能真正提亮）。
    /// </summary>
    public sealed class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance { get; private set; }

        const string KEY_MASTER = "Settings.MasterVolume";
        const string KEY_BGM = "Settings.BgmVolume";
        const string KEY_SFX = "Settings.SfxVolume";
        const string KEY_BRIGHT = "Settings.Brightness";
        const string KEY_FULLSCREEN = "Settings.Fullscreen";

        // 窗口模式下的默认尺寸：游戏按 1920×1080（16:9）设计
        const int WindowedWidth = 1920;
        const int WindowedHeight = 1080;

        Image brightnessOverlay;

        float masterVolume = 1f;
        float bgmVolume = 0.8f;
        float sfxVolume = 1f;
        float brightness = 1f;
        bool fullscreen = true;

        public float MasterVolume { get => masterVolume; set { masterVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KEY_MASTER, masterVolume); } }
        public float BgmVolume { get => bgmVolume; set { bgmVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KEY_BGM, bgmVolume); } }
        public float SfxVolume { get => sfxVolume; set { sfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KEY_SFX, sfxVolume); } }

        public float Brightness
        {
            get => brightness;
            set { brightness = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KEY_BRIGHT, brightness); ApplyBrightness(); }
        }

        public bool Fullscreen
        {
            get => fullscreen;
            set
            {
                fullscreen = value;
                PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
                ApplyDisplay();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EnsureExists()
        {
            if (Instance != null) return;

            var go = new GameObject("GameSettings");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<GameSettings>(); // AddComponent 触发 Awake -> Setup
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Setup();
        }

        void Setup()
        {
            // 常驻 Canvas，排序在最顶层，用于全局亮度遮罩。
            var canvasGo = new GameObject("BrightnessCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var overlayGo = new GameObject("BrightnessOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayGo.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)overlayGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            brightnessOverlay = overlayGo.GetComponent<Image>();
            // 用 4x4 白色纹理造一个 1x1 单位的纯白 sprite，保证 Image 能渲染纯色。
            brightnessOverlay.sprite = Sprite.Create(Texture2D.whiteTexture,
                new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
            brightnessOverlay.type = Image.Type.Simple;
            brightnessOverlay.raycastTarget = false; // 不阻挡下层 UI 点击

            Load();
            ApplyBrightness();
        }

        void Load()
        {
            masterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
            bgmVolume = PlayerPrefs.GetFloat(KEY_BGM, 0.8f);
            sfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);
            brightness = PlayerPrefs.GetFloat(KEY_BRIGHT, 1f);
            fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) != 0;
            ApplyDisplay();
        }

        /// <summary>
        /// 全屏时显式切到显示器的原生分辨率。只写 Screen.fullScreen 会沿用当前窗口尺寸，
        /// 一旦之前的窗口比较小（或者被拖动过），全屏就是放大拉伸，画面会糊。
        /// </summary>
        void ApplyDisplay()
        {
            if (fullscreen)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
            }
            else
            {
                Screen.SetResolution(WindowedWidth, WindowedHeight, FullScreenMode.Windowed);
            }
        }

        void ApplyBrightness()
        {
            if (brightnessOverlay != null)
                brightnessOverlay.color = new Color(0f, 0f, 0f, 1f - brightness);
        }
    }
}
