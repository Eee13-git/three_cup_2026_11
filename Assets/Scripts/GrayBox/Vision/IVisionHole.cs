using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 能在遮蔽层上挖出一块可见区域的物件（远程点亮的机关就实现它）。
    /// 由 VisionFog 每帧读取，点亮时该区域透光。
    /// </summary>
    public interface IVisionHole
    {
        bool HoleActive { get; }
        Vector2 HoleCenter { get; }
        Vector2 HoleHalfSize { get; }
    }
}
