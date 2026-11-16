using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// IMGUI 弹窗共用的中文字体。Unity 内置 GUI 字体没有中文字形，
    /// 不显式加载系统字体的话弹窗里的中文会显示成方块或空白。
    /// </summary>
    public static class PopupFont
    {
        static Font cached;

        public static Font Cjk
        {
            get
            {
                if (cached != null) return cached;

                cached = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Noto Sans CJK SC", "Arial" },
                    24);

                return cached;
            }
        }
    }
}
