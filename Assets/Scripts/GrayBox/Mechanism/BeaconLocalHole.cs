using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 照面灯脚下的那一小块透光区。和灯本身共享点亮状态，
    /// 但洞落在灯的位置，让玩家看清自己脚边。
    /// </summary>
    public sealed class BeaconLocalHole : MonoBehaviour, IVisionHole
    {
        [SerializeField] BeaconSwitch owner;
        [SerializeField] Vector2 size = new Vector2(1.8f, 1.5f);
        [SerializeField] Vector2 offset = new Vector2(0f, -0.2f);

        public bool HoleActive => owner != null && owner.Lit;

        public Vector2 HoleCenter =>
            (owner != null ? (Vector2)owner.transform.position : (Vector2)transform.position) + offset;

        public Vector2 HoleHalfSize => size * 0.5f;

        void Awake()
        {
            if (owner == null) owner = GetComponent<BeaconSwitch>();
        }

        void OnEnable() => VisionHoleRegistry.Register(this);

        void OnDisable() => VisionHoleRegistry.Unregister(this);
    }
}
