using UnityEngine;

namespace ThreeCup.GrayBox
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Portal : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;

        public PlayerSide Side => side;
        public bool Occupied { get; private set; }

        public void Configure(PlayerSide newSide) => side = newSide;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
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
