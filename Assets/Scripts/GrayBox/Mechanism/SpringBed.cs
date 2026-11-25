using System.Collections;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 弹床：画里长着的一丛圆蘑菇。落上去会被弹到固定高度，
    /// 不消耗跳跃，所以盲跳也不用怕——轨迹每次一样。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class SpringBed : MonoBehaviour
    {
        [Tooltip("弹起速度，比角色正常跳跃(10.5)高才有效果")]
        [SerializeField] float bounceVelocity = 15f;

        [SerializeField] float cooldown = 0.15f;

        [Header("Visual")]
        [SerializeField] Transform visual;
        [SerializeField, Range(0.2f, 1f)] float squashScale = 0.55f;
        [SerializeField] float squashSeconds = 0.16f;

        Vector3 baseScale;
        float lastBounce = -99f;

        public MechanismKind Kind => MechanismKind.SpringBed;

        void Awake()
        {
            if (visual == null) visual = transform;
            baseScale = visual.localScale;
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var motor = collision.collider.GetComponentInParent<PlayerMotor2D>();
            if (motor == null) return;

            // 只认从上方落下来的
            if (motor.transform.position.y < transform.position.y) return;
            if (Time.time < lastBounce + cooldown) return;

            var body = motor.GetComponent<Rigidbody2D>();
            if (body == null) return;

            body.velocity = new Vector2(body.velocity.x, bounceVelocity);
            lastBounce = Time.time;

            StartCoroutine(SquashRoutine());
        }

        IEnumerator SquashRoutine()
        {
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime / Mathf.Max(squashSeconds, 0.01f);

                // 先压扁再弹回，用一个对称的三角形
                float t = Mathf.Clamp01(timer);
                float squash = t < 0.5f ? Mathf.Lerp(1f, squashScale, t * 2f) : Mathf.Lerp(squashScale, 1f, (t - 0.5f) * 2f);

                visual.localScale = new Vector3(baseScale.x, baseScale.y * squash, baseScale.z);
                yield return null;
            }

            visual.localScale = baseScale;
        }

        public void Configure(Transform newVisual, float newBounce)
        {
            visual = newVisual;
            bounceVelocity = newBounce;
            if (visual != null) baseScale = visual.localScale;
        }
    }
}
