using UnityEngine;

namespace ThreeCup.GrayBox
{
    [CreateAssetMenu(menuName = "GrayBox/Level Config", fileName = "Level_")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string levelId = "L001";
        [SerializeField] string displayName = "关卡 1";
        [SerializeField] bool enabled = true;

        [Header("Content")]
        [SerializeField] GameObject levelPrefab;

        [Header("Vision")]
        [SerializeField] Vector2 windowSize = new Vector2(4f, 3f);
        [SerializeField] Vector2 leftWindowOffset = new Vector2(8f, 0f);
        [SerializeField] Vector2 rightWindowOffset = new Vector2(-8f, 0f);

        [Header("Tutorial")]
        [Tooltip("进关提示卡的标题；留空则不显示提示卡")]
        [SerializeField] string tutorialTitle = "";
        [Tooltip("进关提示卡的正文，可换行")]
        [SerializeField, TextArea(2, 6)] string tutorialBody = "";

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public bool Enabled => enabled;
        public GameObject LevelPrefab => levelPrefab;
        public Vector2 WindowSize => windowSize;
        public Vector2 LeftWindowOffset => leftWindowOffset;
        public Vector2 RightWindowOffset => rightWindowOffset;
        public string TutorialTitle => tutorialTitle;
        public string TutorialBody => tutorialBody;

        /// <summary>编辑器/构建脚本用：一次性写入全部字段。</summary>
        public void Configure(
            string id,
            string display,
            GameObject prefab,
            Vector2 visionSize,
            Vector2 leftOffset,
            Vector2 rightOffset)
        {
            levelId = id;
            displayName = display;
            levelPrefab = prefab;
            windowSize = visionSize;
            leftWindowOffset = leftOffset;
            rightWindowOffset = rightOffset;
            enabled = true;
        }

        /// <summary>编辑器/构建脚本用：写入教学提示文案。</summary>
        public void ConfigureTutorial(string title, string body)
        {
            tutorialTitle = title;
            tutorialBody = body;
        }
    }
}
