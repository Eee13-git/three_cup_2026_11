using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 可见区域的注册表。组件在 OnEnable/OnDisable 里加进来，
    /// VisionFog 只读这份列表，避免每帧 FindObjectsOfType。
    /// </summary>
    public static class VisionHoleRegistry
    {
        static readonly List<IVisionHole> items = new List<IVisionHole>();

        public static IReadOnlyList<IVisionHole> Items => items;

        public static void Register(IVisionHole hole)
        {
            if (hole == null || items.Contains(hole)) return;
            items.Add(hole);
        }

        public static void Unregister(IVisionHole hole)
        {
            if (hole == null) return;
            items.Remove(hole);
        }
    }
}
