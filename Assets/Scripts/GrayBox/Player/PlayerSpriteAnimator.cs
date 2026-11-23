using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 序列帧播放器。站立/行走是循环播放；跳跃不做成循环 clip，
    /// 而是按"是否离地 + 垂直速度"选帧，这样起跳、最高点、落地
    /// 和物理时序天然对齐，不会出现落地了还在播上升帧的情况。
    /// </summary>
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;

        [Header("Frames")]
        [SerializeField] Sprite[] idle = new Sprite[0];
        [SerializeField] Sprite[] walk = new Sprite[0];

        [Tooltip("0 蓄力 / 1 上升 / 2 最高点 / 3 下落 / 4 落地")]
        [SerializeField] Sprite[] jump = new Sprite[0];

        [Header("Timing")]
        [SerializeField] float idleFps = 8f;
        [SerializeField] float walkFps = 10f;
        [SerializeField] float moveThreshold = 0.3f;

        [Tooltip("垂直速度超过这个值才算真正的上升/下落，中间算滞空最高点")]
        [SerializeField] float airborneSpeed = 3f;

        [SerializeField] float takeoffHold = 0.06f;
        [SerializeField] float landingHold = 0.1f;

        [Header("Facing")]
        [Tooltip("水平速度超过这个值才更新朝向，避免静止时抖动")]
        [SerializeField] float facingThreshold = 0.1f;

        enum State { Idle, Walk, Airborne }

        PlayerMotor2D motor;
        Rigidbody2D body;
        State state = State.Idle;
        int frame;
        float timer;
        bool wasGrounded = true;
        bool facingLeft;
        bool descending;
        float takeoffUntil;
        float landingUntil;

        public void Configure(SpriteRenderer renderer, Sprite[] idleFrames, Sprite[] walkFrames, Sprite[] jumpFrames)
        {
            target = renderer;
            idle = idleFrames;
            walk = walkFrames;
            jump = jumpFrames;
        }

        void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            body = GetComponent<Rigidbody2D>();
            if (target == null) target = GetComponent<SpriteRenderer>();

            // 序列帧素材本身朝左，静止时按原图显示即可
            facingLeft = true;
            if (target != null) target.flipX = false;
        }

        void Update()
        {
            if (motor == null || body == null || target == null) return;

            bool grounded = motor.Grounded;
            float verticalSpeed = body.velocity.y;
            float horizontalSpeed = Mathf.Abs(body.velocity.x);

            UpdateFacing(body.velocity.x, horizontalSpeed);

            // 起跳瞬间先播蓄力帧，落地瞬间先播落地帧
            if (wasGrounded && !grounded)
            {
                takeoffUntil = Time.time + takeoffHold;
                descending = false;
            }
            if (!wasGrounded && grounded) landingUntil = Time.time + landingHold;
            wasGrounded = grounded;

            if (!grounded)
            {
                PlayAirborne(verticalSpeed);
                return;
            }

            if (Time.time < landingUntil)
            {
                Show(jump, 4);
                return;
            }

            if (horizontalSpeed > moveThreshold) PlayLoop(walk, walkFps, State.Walk);
            else PlayLoop(idle, idleFps, State.Idle);
        }

        /// <summary>
        /// 镜像的是 sprite 轴心，所以轴心取在角色内容中心才能原地翻转。
        /// 素材默认朝左：向左移动不翻，向右移动才翻。
        /// </summary>
        void UpdateFacing(float horizontalVelocity, float speed)
        {
            if (speed <= facingThreshold) return;

            bool left = horizontalVelocity < 0f;
            if (left == facingLeft) return;

            facingLeft = left;
            target.flipX = !facingLeft;
        }

        void PlayAirborne(float verticalSpeed)
        {
            state = State.Airborne;

            // 一旦确认在下落就锁住下落帧，避免触地瞬间速度归零时闪回"最高点"帧
            if (verticalSpeed < -airborneSpeed) descending = true;

            if (Time.time < takeoffUntil) Show(jump, 0);
            else if (verticalSpeed > airborneSpeed) Show(jump, 1);
            else if (descending) Show(jump, 3);
            else Show(jump, 2);
        }

        void PlayLoop(Sprite[] set, float fps, State next)
        {
            if (set == null || set.Length == 0) return;

            if (state != next)
            {
                state = next;
                frame = 0;
                timer = 0f;
            }

            timer += Time.deltaTime;
            float step = 1f / Mathf.Max(fps, 0.01f);
            while (timer >= step)
            {
                timer -= step;
                frame = (frame + 1) % set.Length;
            }

            target.sprite = set[frame];
        }

        void Show(Sprite[] set, int index)
        {
            if (set == null || set.Length == 0) return;

            state = State.Airborne;
            target.sprite = set[Mathf.Clamp(index, 0, set.Length - 1)];
        }
    }
}
