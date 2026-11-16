using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 通关条件：两名角色同时待在自己的终点里。
    /// </summary>
    public sealed class WinChecker : MonoBehaviour
    {
        [SerializeField] LevelGoal leftGoal;
        [SerializeField] LevelGoal rightGoal;
        [SerializeField] float requiredSeconds = 0.5f;

        public event System.Action Won;

        public bool Completed { get; private set; }
        public float HeldSeconds => heldSeconds;

        float heldSeconds;

        public void Configure(LevelGoal left, LevelGoal right)
        {
            leftGoal = left;
            rightGoal = right;
        }

        public void ResetState()
        {
            heldSeconds = 0f;
            Completed = false;
        }

        void Update()
        {
            if (Completed) return;

            bool bothInside = leftGoal != null && rightGoal != null &&
                              leftGoal.Occupied && rightGoal.Occupied;

            heldSeconds = bothInside ? heldSeconds + Time.deltaTime : 0f;
            if (heldSeconds < requiredSeconds) return;

            Completed = true;
            Debug.Log("[GrayBox] 通关：两名角色同时站在各自的终点内");
            Won?.Invoke();
        }
    }
}
