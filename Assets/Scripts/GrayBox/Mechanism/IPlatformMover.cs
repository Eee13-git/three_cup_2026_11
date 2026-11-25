using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 会自己移动、玩家站上去会被带着走的平台（游动颜料团、升降台都实现它）。
    /// </summary>
    public interface IPlatformMover
    {
        Vector2 Velocity { get; }
    }
}
