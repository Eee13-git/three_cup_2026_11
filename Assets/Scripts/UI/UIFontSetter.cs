using UnityEngine;
using UnityEngine.UI;
using ThreeCup.GrayBox;

namespace ThreeCup.UI
{
    /// <summary>
    /// 给 Canvas 下的所有 Text 统一赋系统中文字体。
    /// 复用灰盒阶段 PopupFont（Unity 内置字体没有中文字形）。
    /// Awake 和 Start 各跑一次，覆盖 LevelSelectUI 在 Awake 里动态生成的按钮文字。
    /// </summary>
    public sealed class UIFontSetter : MonoBehaviour
    {
        void Awake() => Apply();

        void Start() => Apply();

        public void Apply()
        {
            var font = PopupFont.Cjk;
            if (font == null) return;

            foreach (var text in GetComponentsInChildren<Text>(true))
                if (text.font != font) text.font = font;
        }
    }
}
