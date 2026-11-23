using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 游动的颜料团：一坨浓颜料浮在纸上，沿直线缓慢往返。
    /// 玩家站在上面会跟着一起被带走（由 PlayerMotor2D 读取 Velocity 补偿）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [Tooltip("相对起始位置的往返位移")]
        [SerializeField] Vector2 travel = new Vector2(6f, 0f);

        [Tooltip("单程时长（秒）")]
        [SerializeField] float oneWaySeconds = 2.5f;

        [Tooltip("两端停留时长")]
        [SerializeField] float edgePause = 0.15f;

        Rigidbody2D body;
        Vector2 origin;
        Vector2 previous;

        /// <summary>当前帧速度，供站在上面的角色做位移补偿。</summary>
        public Vector2 Velocity { get; private set; }

        public MechanismKind Kind => MechanismKind.MovingPlatform;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            origin = body.position;
            previous = origin;
        }

        void FixedUpdate()
        {
            float segment = Mathf.Max(oneWaySeconds, 0.05f);
            float hold = Mathf.Max(0f, edgePause);
            float cycle = segment + hold;

            // 0..1 三角波，两端各停 hold 秒
            float phase = Mathf.Repeat(Time.fixedTime, cycle * 2f);
            float p;
            if (phase < hold) p = 0f;
            else if (phase < hold + segment) p = (phase - hold) / segment;
            else if (phase < hold + segment + hold) p = 1f;
            else p = 1f - (phase - hold * 2f - segment) / segment;

            p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(p));

            Vector2 target = origin + travel * p;
            body.MovePosition(target);

            Velocity = (target - previous) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            previous = target;
        }

        public void Configure(Vector2 newTravel, float newOneWaySeconds)
        {
            travel = newTravel;
            oneWaySeconds = Mathf.Max(0.1f, newOneWaySeconds);
        }
    }
}
