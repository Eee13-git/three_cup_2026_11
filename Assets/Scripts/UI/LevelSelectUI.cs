using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ThreeCup.GrayBox;

namespace ThreeCup.UI
{
    /// <summary>
    /// Select 场景的选关界面：运行时从 LevelCatalog 动态生成关卡按钮。
    /// 点按钮把关卡序号写进 GameFlow，再加载游戏场景。
    /// </summary>
    public sealed class LevelSelectUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] LevelCatalog catalog;

        [Header("UI")]
        [SerializeField] Transform buttonRoot;
        [SerializeField] Button levelButtonTemplate;
        [SerializeField] Button backButton;

        [Header("Scenes")]
        [Tooltip("点击关卡后进入的游戏场景名，可在 Inspector 修改")]
        [SerializeField] string gameSceneName = "LHJ_TestScene";
        [SerializeField] string startSceneName = "Start";

        void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBack);
            BuildLevelButtons();
        }

        void BuildLevelButtons()
        {
            if (buttonRoot == null || levelButtonTemplate == null || catalog == null) return;

            levelButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < catalog.Count; i++)
            {
                var config = catalog.Get(i);
                if (config == null || !config.Enabled) continue;

                var button = Instantiate(levelButtonTemplate, buttonRoot);
                button.gameObject.SetActive(true);

                int index = i;
                button.onClick.AddListener(() => OnLevelSelected(index));

                var label = button.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{config.LevelId}  {config.DisplayName}";
            }
        }

        void OnLevelSelected(int index)
        {
            GameFlow.SelectedLevelIndex = index;
            SceneManager.LoadScene(gameSceneName);
        }

        void OnBack() => SceneManager.LoadScene(startSceneName);
    }
}
