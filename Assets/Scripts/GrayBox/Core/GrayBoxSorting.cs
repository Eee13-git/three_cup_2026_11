namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 唯一的渲染排序来源。新增任何物件都从这里取顺序号，
    /// 不要把 magic number 写进场景或构建脚本。
    /// </summary>
    public static class GrayBoxSorting
    {
        /// <summary>地形与机关，会被遮盖层挡住。</summary>
        public const int Terrain = 0;

        /// <summary>被遮盖的机关（尖刺、方块等）。</summary>
        public const int Mechanism = 5;

        /// <summary>遮盖层本身。</summary>
        public const int Fog = 10;

        /// <summary>中央分隔墙等框架元素，永远可见。</summary>
        public const int Divider = 15;

        /// <summary>两个角色，永远可见。</summary>
        public const int Player = 20;

        /// <summary>引导层：路标、信标，永远可见且压在最上面。</summary>
        public const int Hint = 25;
    }
}
