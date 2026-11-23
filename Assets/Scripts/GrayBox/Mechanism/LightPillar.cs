using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 漏光缝：裱纸翘起的一道缝，纸背的光漏下来，把下面一小片画布照亮。
    /// 它不改任何碰撞——地形本来就在，只是平时被裱纸盖着；
    /// 作用是把遮蔽层在自身范围内挖开，让玩家看得见落点。
    /// </summary>
    [ExecuteAlways]
    public sealed class LightPillar : MonoBehaviour
    {
        [Tooltip("透光范围（世界单位）。形状是椭圆，边缘按 VisionFog 的 holeFeather 羽化")]
        [SerializeField] Vector2 size = new Vector2(2.8f, 3.6f);

        static readonly List<LightPillar> active = new List<LightPillar>();

        public static IReadOnlyList<LightPillar> Active => active;

        public MechanismKind Kind => MechanismKind.LightPillar;
        public Vector2 Center => transform.position;
        public Vector2 HalfSize => size * 0.5f;

        public void SetSize(Vector2 newSize)
        {
            if (newSize.x <= 0f || newSize.y <= 0f) return;

            size = newSize;
        }

        void OnEnable()
        {
            if (!active.Contains(this)) active.Add(this);
        }

        void OnDisable() => active.Remove(this);

        void OnValidate()
        {
            size.x = Mathf.Max(0.1f, size.x);
            size.y = Mathf.Max(0.1f, size.y);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.95f, 0.72f, 0.55f);
            Gizmos.DrawWireCube(Center, new Vector3(size.x, size.y, 0f));
        }
    }
}
