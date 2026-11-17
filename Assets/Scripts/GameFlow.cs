namespace ThreeCup
{
    /// <summary>
    /// 跨场景的轻量流程状态。选关界面写入，游戏场景读取。
    /// 关卡本身互相不知道对方，这里的序号只负责「这一局从哪一关开始」。
    /// </summary>
    public static class GameFlow
    {
        /// <summary>选关界面选择的关卡序号。-1 表示未选择，沿用场景默认 startIndex。</summary>
        public static int SelectedLevelIndex = -1;
    }
}
