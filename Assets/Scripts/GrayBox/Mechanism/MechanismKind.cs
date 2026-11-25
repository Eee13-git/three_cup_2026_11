namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 机关的稳定标识。教程提示与图鉴都靠它查 MechanismCatalog，
    /// 新增机关时只在末尾追加，不要改动已有项的数值。
    /// </summary>
    public enum MechanismKind
    {
        None = 0,

        /// <summary>硬结的颜料碴（尖刺）</summary>
        Spike = 1,

        /// <summary>颜料罐（开关）</summary>
        Switch = 2,

        /// <summary>补上的笔触（显隐方块）</summary>
        ToggleBlock = 3,

        /// <summary>留白（存档点）</summary>
        Checkpoint = 4,

        /// <summary>污渍（待清洁点）</summary>
        Stain = 5,

        /// <summary>漏光缝</summary>
        LightPillar = 6,

        /// <summary>以前的压纸钉，已移除，保留数值避免后续项错位</summary>
        PinStation = 7,

        /// <summary>未干的颜料块</summary>
        CrumblePlatform = 8,

        /// <summary>游动的颜料团</summary>
        MovingPlatform = 9,

        /// <summary>分光缝</summary>
        SplitAperture = 10,

        /// <summary>折痕</summary>
        Crease = 11,

        /// <summary>透镜（露珠，视窗缩放）</summary>
        Lens = 12,

        /// <summary>弹床（圆蘑菇）</summary>
        SpringBed = 13,

        /// <summary>双压板（石压板）</summary>
        TandemPlate = 14,

        /// <summary>升降台</summary>
        Lift = 15,

        /// <summary>照面灯（点亮对面视野）</summary>
        Beacon = 16
    }
}
