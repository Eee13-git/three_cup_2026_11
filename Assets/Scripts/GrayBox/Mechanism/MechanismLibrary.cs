using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 机关资料的统一取用入口。图鉴与教程可能出现在主菜单或选关场景，
    /// 拿不到 LevelSession，所以这里不依赖场景引用：
    /// 优先使用注入的目录，其次读 Resources 下的同一份资产。
    /// </summary>
    public static class MechanismLibrary
    {
        const string ResourcePath = "MechanismCatalog";

        static MechanismCatalog catalog;

        public static MechanismCatalog Catalog
        {
            get
            {
                if (catalog == null) catalog = Resources.Load<MechanismCatalog>(ResourcePath);
                return catalog;
            }
        }

        /// <summary>启动时把工程里指定的目录注入进来，之后就不再走 Resources。</summary>
        public static void SetCatalog(MechanismCatalog value) => catalog = value;

        public static MechanismInfo Get(MechanismKind kind) =>
            Catalog != null ? Catalog.Find(kind) : null;

        public static string DisplayName(MechanismKind kind) =>
            Get(kind)?.DisplayName ?? string.Empty;

        public static string Hint(MechanismKind kind) =>
            Get(kind)?.TutorialHint ?? string.Empty;

        public static string Lore(MechanismKind kind) =>
            Get(kind)?.Lore ?? string.Empty;

        public static string Rule(MechanismKind kind) =>
            Get(kind)?.Rule ?? string.Empty;
    }
}
