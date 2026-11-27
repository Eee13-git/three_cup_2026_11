using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 通关最后一关后的胜利弹窗：沿用失败弹窗的撕纸卡片风格（同样的贴图与排版），
    /// 整屏压暗 + 标题 / 提示 / 落款。由 LevelSession.Victory 驱动。
    /// </summary>
    public sealed class VictoryPopup : MonoBehaviour
    {
        [SerializeField] LevelSession session;

        [Header("Visual")]
        [SerializeField] CanvasGroup group;
        [SerializeField] Text detailText;

        bool? shownVisible;
        string shownDetail;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();

            var font = PopupFont.Serif;
            if (font != null)
                foreach (var text in GetComponentsInChildren<Text>(true))
                    text.font = font;

            SetVisible(false);
        }

        void Update()
        {
            // 暂停时让暂停菜单露出来，不和胜利卡片叠在一起
            bool visible = session != null && session.Victory && Time.timeScale > 0f;
            if (visible) RefreshDetail();
            SetVisible(visible);
        }

        void RefreshDetail()
        {
            if (detailText == null) return;

            int count = session != null ? session.LevelCount : 0;
            string detail = count > 0 ? $"全部 {count} 关 · 完成" : "全部关卡 · 完成";
            if (detail == shownDetail) return;

            shownDetail = detail;
            detailText.text = detail;
        }

        void SetVisible(bool visible)
        {
            if (shownVisible.HasValue && shownVisible.Value == visible) return;
            shownVisible = visible;

            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}
