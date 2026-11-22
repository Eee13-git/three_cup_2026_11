using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 一关的终点。两人必须同时待在各自的终点里才算通关。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelGoal : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;

        [Tooltip("角色到达时在污渍处播放的清洁晕")]
        [SerializeField] GameObject cleanEffectPrefab;

        public PlayerSide Side => side;
        public bool Occupied { get; private set; }

        public void Configure(PlayerSide newSide) => side = newSide;

        /// <summary>由 LevelSession 在装配关卡时统一注入，避免逐关实例各存一份引用。</summary>
        public void SetCleanEffect(GameObject prefab) => cleanEffectPrefab = prefab;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsOwnCharacter(other)) return;

            Occupied = true;
            if (cleanEffectPrefab != null)
                Instantiate(cleanEffectPrefab, transform.position, Quaternion.identity);
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
