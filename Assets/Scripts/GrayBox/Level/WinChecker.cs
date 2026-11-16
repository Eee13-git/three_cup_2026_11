using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Clearing a level means both characters stand in their own portal at the same time.
    /// </summary>
    public sealed class WinChecker : MonoBehaviour
    {
        [SerializeField] Portal leftPortal;
        [SerializeField] Portal rightPortal;
        [SerializeField] float requiredSeconds = 0.5f;

        public event System.Action Won;

        public bool Completed { get; private set; }
        public float HeldSeconds => heldSeconds;

        float heldSeconds;

        public void Configure(Portal left, Portal right)
        {
            leftPortal = left;
            rightPortal = right;
        }

        public void ResetState()
        {
            heldSeconds = 0f;
            Completed = false;
        }

        void Update()
        {
            if (Completed) return;

            bool bothInside = leftPortal != null && rightPortal != null &&
                              leftPortal.Occupied && rightPortal.Occupied;

            heldSeconds = bothInside ? heldSeconds + Time.deltaTime : 0f;
            if (heldSeconds < requiredSeconds) return;

            Completed = true;
            Debug.Log("[GrayBox] 通关：两名角色同时站在各自的传送门内");
            Won?.Invoke();
        }
    }
}
