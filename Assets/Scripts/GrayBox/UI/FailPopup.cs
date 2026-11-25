using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 命数耗尽的失败弹窗：整屏压暗 + 撕纸卡片（关卡失败 / 按 R 重新开始本关 / 关卡与颜料状态）。
    /// 取代原来的 IMGUI 版本。
    /// </summary>
    public sealed class FailPopup : MonoBehaviour
    {
        [SerializeField] LevelSession session;

        [Header("Visual")]
        [SerializeField] CanvasGroup group;
        [SerializeField] Text detailText;

        [Header("Text")]
        [SerializeField] string exhaustedText = "颜料已耗尽";

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
            // 暂停时让暂停菜单露出来，不和失败卡片叠在一起
            bool visible = session != null && session.Failed && Time.timeScale > 0f;
            if (visible) RefreshDetail();
            SetVisible(visible);
        }

        void RefreshDetail()
        {
            if (detailText == null) return;

            var level = session.Current;
            if (level == null) return;

            string detail = $"{level.DisplayName} · {level.LevelId} · {exhaustedText}";
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
