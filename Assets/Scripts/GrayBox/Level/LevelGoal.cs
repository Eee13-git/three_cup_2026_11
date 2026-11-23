using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 一关的污渍（终点）。角色站上去按 F 才能清除；
    /// 两个污渍都清完才算通关，清完之后不需要继续站着。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelGoal : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;
        [SerializeField] StainDissolve dissolve;

        public PlayerSide Side => side;
        public bool Occupied { get; private set; }
        public bool Cleaned { get; private set; }
        public MechanismKind Kind => MechanismKind.Stain;

        public void Configure(PlayerSide newSide) => side = newSide;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void Awake()
        {
            if (dissolve == null) dissolve = GetComponentInChildren<StainDissolve>();
        }

        /// <summary>角色踩在上面时才允许清除。返回是否真的清掉了。</summary>
        public bool TryClean()
        {
            if (Cleaned || !Occupied) return false;

            Cleaned = true;
            if (dissolve != null) dissolve.Play();
            return true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsOwnCharacter(other)) Occupied = true;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (IsOwnCharacter(other)) Occupied = false;
        }

        bool IsOwnCharacter(Collider2D other)
        {
            var motor = other.GetComponent<PlayerMotor2D>();
            return motor != null && motor.Side == side;
        }
    }
}
