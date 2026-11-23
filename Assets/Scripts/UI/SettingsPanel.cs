using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// 设置面板：音量三档 / 亮度 / 全屏，值与持久化统一交给 GameSettings。
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider bgmSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Slider brightnessSlider;
        [SerializeField] Toggle fullscreenToggle;

        void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        void Start()
        {
            LoadValues();
            Bind();
        }

        void LoadValues()
        {
            var s = GameSettings.Instance;
            if (s == null) return;

            if (masterSlider != null) masterSlider.SetValueWithoutNotify(s.MasterVolume);
            if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(s.BgmVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(s.SfxVolume);
            if (brightnessSlider != null) brightnessSlider.SetValueWithoutNotify(s.Brightness);
            if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(s.Fullscreen);
        }

        void Bind()
        {
            var s = GameSettings.Instance;
            if (s == null) return;

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(v => s.MasterVolume = v);
            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(v => s.BgmVolume = v);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(v => s.SfxVolume = v);
            if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(v => s.Brightness = v);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(v => s.Fullscreen = v);
        }

        public void Close() => gameObject.SetActive(false);
    }
}
