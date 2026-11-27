using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ThreeCup.GrayBox;

namespace ThreeCup.UI
{
    /// <summary>
    /// 暂停菜单：ESC 呼出/隐藏，暂停/恢复游戏（Time.timeScale）。
    /// 三个按钮：设置 / 返回选关 / 返回主界面。
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] Button settingsButton;
        [SerializeField] Button selectButton;
        [SerializeField] Button mainMenuButton;

        [Header("Settings")]
        [SerializeField] GameObject settingsPanel;

        [Header("Scenes")]
        [SerializeField] string selectSceneName = "Select";
        [SerializeField] string mainMenuSceneName = "Start";

        bool paused;
        LevelSession session;

        void Awake()
        {
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (selectButton != null) selectButton.onClick.AddListener(OnSelect);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // 胜利界面不弹暂停菜单：ESC 直接回主界面
            if (IsVictory)
            {
                OnMainMenu();
                return;
            }

            // 设置面板开着时，ESC 先关设置面板
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                return;
            }

            SetPaused(!paused);
        }

        bool IsVictory
        {
            get
            {
                if (session == null) session = FindObjectOfType<LevelSession>();
                return session != null && session.Victory;
            }
        }

        void SetPaused(bool value)
        {
            paused = value;
            if (pausePanel != null) pausePanel.SetActive(value);
            Time.timeScale = value ? 0f : 1f;
        }

        void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        void OnSelect()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(selectSceneName);
        }

        void OnMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
