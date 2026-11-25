using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ThreeCup.GrayBox;

namespace ThreeCup.UI
{
    /// <summary>
    /// Select 场景选关界面：教学关目录 / 正式关目录分组，小正方形网格 + 翻页。
    /// 类型由「关卡在哪个目录」决定，两个目录用不同纯色。
    /// </summary>
    public sealed class LevelSelectUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] LevelCatalog tutorialCatalog;
        [SerializeField] LevelCatalog normalCatalog;

        [Header("Grid")]
        [SerializeField] Transform gridRoot;
        [SerializeField] Button cellTemplate;
        [Tooltip("与 gridRoot 上 GridLayoutGroup 的列数保持一致")]
        [SerializeField] int columns = 4;
        [SerializeField] int rows = 3;

        [Header("Colors")]
        [SerializeField] Color tutorialColor = new Color(0.85f, 0.62f, 0.20f, 1f);
        [SerializeField] Color normalColor = new Color(0.24f, 0.38f, 0.56f, 1f);

        [Header("Sprites")]
        [Tooltip("教学关方块贴图；为空则回退到 tutorialColor 纯色")]
        [SerializeField] Sprite tutorialSprite;
        [Tooltip("正式关方块贴图；为空则回退到 normalColor 纯色")]
        [SerializeField] Sprite normalSprite;

        [Header("Paging")]
        [SerializeField] Button prevButton;
        [SerializeField] Button nextButton;
        [SerializeField] Text pageLabel;

        [Header("Placeholder")]
        [SerializeField] Text placeholderText;

        [Header("Navigation")]
        [SerializeField] Button backButton;
        [SerializeField] string startSceneName = "Start";

        readonly List<LevelConfig> tutorials = new List<LevelConfig>();
        readonly List<LevelConfig> normals = new List<LevelConfig>();
        readonly List<Button> cells = new List<Button>();

        int page;
        int totalPages;
        int capacity;

        void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBack);
            if (prevButton != null) prevButton.onClick.AddListener(() => GoToPage(page - 1));
            if (nextButton != null) nextButton.onClick.AddListener(() => GoToPage(page + 1));

            capacity = columns * rows;
            SplitLevels();
            StoreGlobalMapping();
            BuildCells();
            GoToPage(1);
        }

        void SplitLevels()
        {
            tutorials.Clear();
            normals.Clear();
            Collect(tutorialCatalog, tutorials);
            Collect(normalCatalog, normals);
        }

        void Collect(LevelCatalog catalog, List<LevelConfig> target)
        {
            if (catalog == null) return;

            for (int i = 0; i < catalog.Count; i++)
            {
                var config = catalog.Get(i);
                if (config != null && config.Enabled) target.Add(config);
            }
        }

        void BuildCells()
        {
            if (gridRoot == null || cellTemplate == null) return;

            cellTemplate.gameObject.SetActive(false);

            for (int i = 0; i < capacity; i++)
            {
                var cell = Instantiate(cellTemplate, gridRoot);
                cell.gameObject.SetActive(true);
                cells.Add(cell);

                int slot = i;
                cell.onClick.AddListener(() => OnCellClicked(slot));
            }
        }

        int TutorialPages => Mathf.Max(1, Mathf.CeilToInt(tutorials.Count / (float)capacity));
        int NormalPages => Mathf.CeilToInt(normals.Count / (float)capacity);
        int TotalPages => TutorialPages + NormalPages;

        void GoToPage(int target)
        {
            totalPages = TotalPages;
            page = Mathf.Clamp(target, 1, totalPages);
            Render();
        }

        void Render()
        {
            if (page <= TutorialPages)
            {
                int start = (page - 1) * capacity;
                FillCells(tutorials, start, tutorialColor, true);

                if (placeholderText != null)
                    placeholderText.gameObject.SetActive(tutorials.Count == 0);
            }
            else
            {
                int normalPage = page - TutorialPages;
                int start = (normalPage - 1) * capacity;
                FillCells(normals, start, normalColor, false);

                if (placeholderText != null) placeholderText.gameObject.SetActive(false);
            }

            UpdatePager();
        }

        void FillCells(List<LevelConfig> list, int start, Color color, bool tutorial)
        {
            int globalOffset = tutorial ? 0 : tutorials.Count;
            int unlocked = LevelProgress.UnlockedIndex;
            var lockedTint = new Color(0.5f, 0.5f, 0.5f, 1f);
            var lockedLabel = new Color(0.32f, 0.30f, 0.28f, 1f);
            var normalLabel = new Color(0.227f, 0.184f, 0.149f, 1f);

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                int index = start + i;

                if (index >= list.Count)
                {
                    cell.gameObject.SetActive(false);
                    continue;
                }

                cell.gameObject.SetActive(true);
                var config = list[index];

                int globalIndex = globalOffset + index + 1;
                bool locked = globalIndex > unlocked;
                bool isProgress = globalIndex == unlocked;

                var img = cell.GetComponent<Image>();
                if (img != null)
                {
                    var sprite = tutorial ? tutorialSprite : normalSprite;
                    if (sprite != null)
                    {
                        img.sprite = sprite;
                        img.color = locked ? lockedTint : Color.white;
                    }
                    else img.color = color;
                }

                var btn = cell.GetComponent<Button>();
                if (btn != null) btn.interactable = !locked;

                var hover = cell.GetComponent<CellHoverInk>();
                if (hover != null)
                {
                    hover.SetHoverEnabled(!locked);
                    hover.SetAlwaysOn(isProgress);
                }

                var label = cell.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = (index + 1).ToString();
                    label.color = locked ? lockedLabel : normalLabel;
                }
            }
        }

        void StoreGlobalMapping()
        {
            for (int i = 0; i < tutorials.Count; i++)
                LevelProgress.StoreGlobalIndex(tutorials[i].LevelId, i + 1);

            for (int i = 0; i < normals.Count; i++)
                LevelProgress.StoreGlobalIndex(normals[i].LevelId, tutorials.Count + i + 1);
        }

        void OnCellClicked(int slot)
        {
            LevelConfig config = null;
            string sceneName = null;

            if (page <= TutorialPages)
            {
                int start = (page - 1) * capacity;
                int index = start + slot;
                if (index < tutorials.Count)
                {
                    config = tutorials[index];
                    sceneName = tutorialCatalog != null ? tutorialCatalog.SceneName : null;
                }
            }
            else
            {
                int normalPage = page - TutorialPages;
                int start = (normalPage - 1) * capacity;
                int index = start + slot;
                if (index < normals.Count)
                {
                    config = normals[index];
                    sceneName = normalCatalog != null ? normalCatalog.SceneName : null;
                }
            }

            if (config == null || string.IsNullOrEmpty(sceneName)) return;

            GameFlow.SelectedLevelId = config.LevelId;
            SceneManager.LoadScene(sceneName);
        }

        void UpdatePager()
        {
            if (pageLabel != null) pageLabel.text = page + " / " + totalPages;
            if (prevButton != null) prevButton.interactable = page > 1;
            if (nextButton != null) nextButton.interactable = page < totalPages;
        }

        void OnBack() => SceneManager.LoadScene(startSceneName);
    }
}
