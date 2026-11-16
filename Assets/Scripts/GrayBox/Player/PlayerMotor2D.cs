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

        [Header("Fall Recovery")]
        [SerializeField] float fallRespawnY = -6f;

        [Header("Ground Check")]
        [SerializeField] LayerMask groundMask = ~0;
        [SerializeField] float groundRayLength = 0.1f;

        public PlayerSide Side => side;
        public bool Grounded { get; private set; }
        public int RespawnCount { get; private set; }
        public Vector2 SafePoint => safePoint;

        Rigidbody2D body;
        Collider2D bodyCollider;
        InputRouter router;
        Vector2 safePoint;
        float horizontalInput;
        bool jumpRequested;

        public void Configure(PlayerSide newSide) => side = newSide;

        public void ResetSafePoint(Vector2 position) => safePoint = position;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            safePoint = body.position;
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
            Grounded = CheckGrounded();

            Vector2 velocity = body.velocity;
            velocity.x = horizontalInput * moveSpeed;
            if (jumpRequested && Grounded) velocity.y = jumpVelocity;
            jumpRequested = false;
            body.velocity = velocity;

            // The last spot we stood still on is the safest place to come back to.
            if (Grounded && Mathf.Abs(velocity.y) < 0.6f) safePoint = body.position;

            if (body.position.y < fallRespawnY) Respawn();
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

        void Respawn()
        {
            body.velocity = Vector2.zero;
            body.position = safePoint;
            transform.position = safePoint;
            RespawnCount++;
        }
    }
}
