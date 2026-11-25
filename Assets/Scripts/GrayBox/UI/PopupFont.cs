using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 弹窗共用的中文字体。Unity 内置字体没有中文字形，
    /// 不显式加载系统字体的话弹窗里的中文会显示成方块或空白。
    /// </summary>
    public static class PopupFont
    {
        static Font cached;
        static Font serif;

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

        /// <summary>衬线（宋体类）字形，死亡弹窗沿用美术稿的排版风格。</summary>
        public static Font Serif
        {
            get
            {
                if (serif != null) return serif;

                serif = Font.CreateDynamicFontFromOSFont(
                    new[] { "Source Han Serif SC", "Noto Serif CJK SC", "SimSun", "STSong", "Songti SC" },
                    40);

                return serif != null ? serif : Cjk;
            }
        }
    }
}
