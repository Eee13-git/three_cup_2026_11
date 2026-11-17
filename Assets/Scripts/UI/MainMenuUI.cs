using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// Start 场景的开始界面：开始游戏 / 设置 / 退出。
    /// 纯色贴图占位，正式美术后再替换按钮与背景。
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] Button startButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;

        [Header("Settings")]
        [SerializeField] GameObject settingsPanel;

        [Header("Scenes")]
        [SerializeField] string selectSceneName = "Select";

        void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(OnStart);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        void OnStart() => SceneManager.LoadScene(selectSceneName);

        void OnSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
