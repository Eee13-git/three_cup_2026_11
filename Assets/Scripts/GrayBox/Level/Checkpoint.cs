using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 手动安放的存档点。角色经过后，死亡就从这里复活。
    /// order 越大越靠后；角色只记录已到达的最大 order，
    /// 所以走回头路经过更早的存档点不会把复活点往回退。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;

        [Tooltip("同一侧内的先后顺序，越大越靠后")]
        [SerializeField] int order;

        public PlayerSide Side => side;
        public int Order => order;
        public MechanismKind Kind => MechanismKind.Checkpoint;

        public void Configure(PlayerSide newSide, int newOrder)
        {
            side = newSide;
            order = newOrder;
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var motor = other.GetComponent<PlayerMotor2D>();
            if (motor == null || motor.Side != side) return;

            motor.ReachCheckpoint(transform.position, order);
        }
    }
}
