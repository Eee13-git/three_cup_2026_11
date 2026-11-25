using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 升降台：有人站在上面就往上抬，人离开就自己落回原处。
    /// 用 kinematic 刚体推动，玩家由 PlayerMotor2D 读 Velocity 跟随。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Lift : MonoBehaviour, IPlatformMover
    {
        [Tooltip("从起点往上抬多高")]
        [SerializeField] float riseDistance = 3f;

        [SerializeField] float speed = 2.5f;

        [Tooltip("台面尺寸，用来检测上方有没有人")]
        [SerializeField] Vector2 size = new Vector2(1.6f, 0.8f);

        Rigidbody2D body;
        Vector2 origin;
        float currentOffset;

        public MechanismKind Kind => MechanismKind.Lift;
        public Vector2 Velocity { get; private set; }
        public bool Risen => currentOffset > riseDistance - 0.05f;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            origin = body.position;
        }

        void FixedUpdate()
        {
            bool rider = PlayerOnTop();
            float target = rider ? riseDistance : 0f;

            float next = Mathf.MoveTowards(currentOffset, target, speed * Time.fixedDeltaTime);
            Vector2 nextPosition = origin + Vector2.up * next;

            Vector2 before = body.position;
            body.MovePosition(nextPosition);
            Velocity = (nextPosition - before) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);

            currentOffset = next;
        }

        bool PlayerOnTop()
        {
            Vector2 center = origin + Vector2.up * (currentOffset + size.y * 0.5f + 0.12f);
            var hits = Physics2D.OverlapBoxAll(center, new Vector2(size.x * 0.9f, 0.3f), 0f);

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.GetComponentInParent<PlayerMotor2D>() != null) return true;
            }

            return false;
        }

        public void Configure(Vector2 newSize, float newRise, float newSpeed)
        {
            size = newSize;
            riseDistance = Mathf.Max(0.1f, newRise);
            speed = Mathf.Max(0.1f, newSpeed);
        }
    }
}
