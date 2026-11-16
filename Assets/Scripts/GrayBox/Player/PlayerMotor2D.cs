using UnityEngine;

namespace ThreeCup.GrayBox
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] PlayerSide side = PlayerSide.Left;

        [Header("Movement")]
        [SerializeField] float moveSpeed = 5.5f;
        [SerializeField] float jumpVelocity = 10.5f;

        [Header("Death")]
        [SerializeField] float fallRespawnY = -6f;

        /// <summary>机关持续判定时防止一帧扣掉多条命。</summary>
        [SerializeField] float invulnerabilitySeconds = 0.5f;

        [Header("Ground Check")]
        [SerializeField] LayerMask groundMask = ~0;
        [SerializeField] float groundRayLength = 0.1f;

        /// <summary>掉坑和机关致死共用这一条路径。</summary>
        public event System.Action<PlayerSide, DeathCause> Died;

        public PlayerSide Side => side;
        public bool Grounded { get; private set; }
        public bool Frozen { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;
        public int DeathCount { get; private set; }

        /// <summary>死亡后复活的位置：出生点，或已经过的最后一个存档点。</summary>
        public Vector2 RespawnPoint => respawnPoint;

        Rigidbody2D body;
        Collider2D bodyCollider;
        InputRouter router;
        Vector2 respawnPoint;
        int checkpointOrder = -1;
        float horizontalInput;
        bool jumpRequested;
        bool frozen;
        float invulnerableUntil;

        public void Configure(PlayerSide newSide) => side = newSide;

        /// <summary>套用关卡出生点，并清空存档点进度（重开关卡时用）。</summary>
        public void ResetRespawn(Vector2 position)
        {
            respawnPoint = position;
            checkpointOrder = -1;
        }

        /// <summary>经过存档点。只有更靠后的存档点才会生效。</summary>
        public void ReachCheckpoint(Vector2 position, int order)
        {
            if (order <= checkpointOrder) return;

            checkpointOrder = order;
            respawnPoint = position;
        }

        /// <summary>Freezes the character in place, used when the level is failed.</summary>
        public void SetFrozen(bool value)
        {
            frozen = value;
            Frozen = value;

            if (body == null) body = GetComponent<Rigidbody2D>();
            if (body == null) return;

            body.velocity = Vector2.zero;
            body.simulated = !value;
        }

        /// <summary>唯一的死亡入口。掉坑、尖刺、激光都走这里。</summary>
        public void Kill(DeathCause cause)
        {
            if (frozen) return;
            if (IsInvulnerable) return;

            invulnerableUntil = Time.time + invulnerabilitySeconds;

            body.velocity = Vector2.zero;
            body.position = respawnPoint;
            transform.position = respawnPoint;
            DeathCount++;

            Died?.Invoke(side, cause);
        }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            respawnPoint = body.position;
        }

        void Start()
        {
            router = FindObjectOfType<InputRouter>();
            if (router != null) router.Register(this);
        }

        void OnDestroy()
        {
            if (router != null) router.Unregister(this);
        }

        void Update()
        {
            if (frozen)
            {
                horizontalInput = 0f;
                jumpRequested = false;
                return;
            }

            if (router == null) router = FindObjectOfType<InputRouter>();
            if (router == null)
            {
                horizontalInput = 0f;
                jumpRequested = false;
                return;
            }

            horizontalInput = router.GetHorizontal(side);
            if (router.GetJumpDown(side)) jumpRequested = true;
        }

        void FixedUpdate()
        {
            if (frozen) return;

            Grounded = CheckGrounded();

            Vector2 velocity = body.velocity;
            velocity.x = horizontalInput * moveSpeed;
            if (jumpRequested && Grounded) velocity.y = jumpVelocity;
            jumpRequested = false;
            body.velocity = velocity;

            if (body.position.y < fallRespawnY) Kill(DeathCause.Fall);
        }

        /// <summary>Ray starts just below the collider so it cannot hit ourselves.</summary>
        bool CheckGrounded()
        {
            Bounds bounds = bodyCollider.bounds;
            float originY = bounds.min.y - 0.02f;
            float inset = 0.05f;

            if (Physics2D.Raycast(new Vector2(bounds.center.x, originY), Vector2.down, groundRayLength, groundMask)) return true;
            if (Physics2D.Raycast(new Vector2(bounds.min.x + inset, originY), Vector2.down, groundRayLength, groundMask)) return true;
            if (Physics2D.Raycast(new Vector2(bounds.max.x - inset, originY), Vector2.down, groundRayLength, groundMask)) return true;
            return false;
        }
    }
}
