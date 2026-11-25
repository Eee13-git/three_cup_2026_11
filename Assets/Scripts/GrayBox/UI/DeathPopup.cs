using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 死亡弹窗：一张米白笔触 +「角色A已死亡 / 操作已锁定 2 秒」。
    /// 弹出期间 LevelSession 已经锁住了两个角色的操作，所以这里只负责显示。
    /// </summary>
    public sealed class DeathPopup : MonoBehaviour
    {
        [SerializeField] LevelSession session;

        [Header("Visual")]
        [SerializeField] CanvasGroup group;
        [SerializeField] Text titleText;
        [SerializeField] Text subtitleText;

        [Header("Text")]
        [SerializeField] string leftLetter = "A";
        [SerializeField] string rightLetter = "B";
        [SerializeField] Color letterColor = new Color32(0xA1, 0x50, 0x3F, 0xFF);
        [SerializeField] string subtitle = "操作已锁定 2 秒";

        PlayerSide? shownSide;
        bool? shownVisible;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();

            if (titleText != null) titleText.font = PopupFont.Serif;
            if (subtitleText != null)
            {
                subtitleText.font = PopupFont.Serif;
                subtitleText.text = subtitle;
            }

            SetVisible(false);
        }

        void Update()
        {
            bool visible = session != null && session.DeathPopupVisible && Time.timeScale > 0f;
            if (visible) RefreshContent();
            SetVisible(visible);
        }

        void RefreshContent()
        {
            var side = session.DeathPopupSide;
            if (shownSide.HasValue && shownSide.Value == side) return;
            shownSide = side;

            if (titleText == null) return;

            string letter = side == PlayerSide.Left ? leftLetter : rightLetter;
            string hex = ColorUtility.ToHtmlStringRGB(letterColor);
            titleText.text = $"角色<color=#{hex}>{letter}</color>已死亡";
        }

        void SetVisible(bool visible)
        {
            if (shownVisible.HasValue && shownVisible.Value == visible) return;
            shownVisible = visible;

            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            // 隐藏时清掉缓存的侧别，下次弹出一定重新拼一次标题
            if (!visible) shownSide = null;
        }
    }
}
