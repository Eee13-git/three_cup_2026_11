namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 可被开关驱动的对象。放在 Mechanism 下，Switch 通过组件引用找到它。
    /// </summary>
    public interface ISwitchTarget
    {
        void SetActive(bool active);
    }
}
