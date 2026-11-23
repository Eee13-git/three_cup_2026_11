using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 通关条件：两处污渍都被清除（各自站上去按 F）。
    /// </summary>
    public sealed class WinChecker : MonoBehaviour
    {
        [SerializeField] LevelGoal leftGoal;
        [SerializeField] LevelGoal rightGoal;

        [Tooltip("两个污渍都清除后，还要保持多久才判定通关")]
        [SerializeField] float requiredSeconds;

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

            bool bothCleaned = leftGoal != null && rightGoal != null &&
                               leftGoal.Cleaned && rightGoal.Cleaned;

            if (!bothCleaned)
            {
                heldSeconds = 0f;
                return;
            }

            heldSeconds += Time.deltaTime;
            if (heldSeconds < requiredSeconds) return;

            Completed = true;
            Debug.Log("[GrayBox] 通关：两处污渍都已清除");
            Won?.Invoke();
        }
    }
}
