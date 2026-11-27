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

        [Header("Vision Direction")]
        [Tooltip("关卡预制体里的 WindowPreview_Left/Right 决定视窗初位与尺寸；关掉则回到手填的 offset")]
        [SerializeField] bool usePreviewOffsets = true;

        [Tooltip("反向跟随：角色往右走，视窗往左走。开启时该侧 offset 通常填 (0, 0)")]
        [SerializeField] bool leftWindowInverted;
        [SerializeField] bool rightWindowInverted;

        [Tooltip("垂直反向：角色往上走，视窗往下走")]
        [SerializeField] bool leftWindowInvertedY;
        [SerializeField] bool rightWindowInvertedY;

        [Tooltip("反向跟随的镜像轴，0 表示中央墙所在的中缝")]
        [SerializeField] float windowMirrorAxisX;

        [Tooltip("垂直反向的镜像水平线（世界 y）")]
        [SerializeField] float windowMirrorAxisY;

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
        public bool LeftWindowInverted => leftWindowInverted;
        public bool RightWindowInverted => rightWindowInverted;
        public bool LeftWindowInvertedY => leftWindowInvertedY;
        public bool RightWindowInvertedY => rightWindowInvertedY;
        public float WindowMirrorAxisX => windowMirrorAxisX;
        public float WindowMirrorAxisY => windowMirrorAxisY;
        public bool UsePreviewOffsets => usePreviewOffsets;
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
            Configure(id, display, prefab, visionSize, leftOffset, rightOffset, false, false, 0f);
        }

        /// <summary>只设置水平反向的版本。</summary>
        public void Configure(
            string id,
            string display,
            GameObject prefab,
            Vector2 visionSize,
            Vector2 leftOffset,
            Vector2 rightOffset,
            bool leftInverted,
            bool rightInverted,
            float mirrorAxisX)
        {
            Configure(
                id, display, prefab, visionSize, leftOffset, rightOffset,
                leftInverted, rightInverted, false, false, mirrorAxisX, 0f);
        }

        /// <summary>带完整视窗方向的全量写入（水平 / 垂直反向各自独立）。</summary>
        public void Configure(
            string id,
            string display,
            GameObject prefab,
            Vector2 visionSize,
            Vector2 leftOffset,
            Vector2 rightOffset,
            bool leftInverted,
            bool rightInverted,
            bool leftInvertedY,
            bool rightInvertedY,
            float mirrorAxisX,
            float mirrorAxisY)
        {
            levelId = id;
            displayName = display;
            levelPrefab = prefab;
            windowSize = visionSize;
            leftWindowOffset = leftOffset;
            rightWindowOffset = rightOffset;
            leftWindowInverted = leftInverted;
            rightWindowInverted = rightInverted;
            leftWindowInvertedY = leftInvertedY;
            rightWindowInvertedY = rightInvertedY;
            windowMirrorAxisX = mirrorAxisX;
            windowMirrorAxisY = mirrorAxisY;
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
