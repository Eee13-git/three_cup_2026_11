using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 全项目共用的中文字体。
    /// 优先使用工程内导入的 Chusung（Resources/Fonts/Chusung），
    /// 找不到时才回退到系统字体，保证任何环境下中文都不会显示成方块。
    /// </summary>
    public static class PopupFont
    {
        const string ProjectFontPath = "Fonts/Chusung";

        static Font cached;
        static Font serif;
        static Font project;

        /// <summary>工程内字体；不存在时返回 null。</summary>
        public static Font Project
        {
            get
            {
                if (project != null) return project;

                project = Resources.Load<Font>(ProjectFontPath);
                return project;
            }
        }

        public static Font Cjk
        {
            get
            {
                var custom = Project;
                if (custom != null) return custom;

                if (cached != null) return cached;

                cached = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Noto Sans CJK SC", "Arial" },
                    24);

                return cached;
            }
        }

        /// <summary>衬线（宋体类）字形；工程字体存在时也统一用它。</summary>
        public static Font Serif
        {
            get
            {
                var custom = Project;
                if (custom != null) return custom;

                if (serif != null) return serif;

                serif = Font.CreateDynamicFontFromOSFont(
                    new[] { "Source Han Serif SC", "Noto Serif CJK SC", "SimSun", "STSong", "Songti SC" },
                    40);

                return serif != null ? serif : Cjk;
            }
        }
    }
}
