using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 致死机关。进入和停留都会判定，受伤节奏由玩家自身的无敌帧控制，
    /// 所以持续判定型机关不会一帧扣掉多条命。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hazard : MonoBehaviour
    {
        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other) => Hurt(other);

        // 兜底：玩家若被卡在机关里，无敌帧结束后会继续受伤
        void OnTriggerStay2D(Collider2D other) => Hurt(other);

        static void Hurt(Collider2D other)
        {
            var motor = other.GetComponent<PlayerMotor2D>();
            if (motor != null) motor.Kill(DeathCause.Hazard);
        }
    }
}
