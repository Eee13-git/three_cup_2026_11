using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 按固定周期开关一个子物体，用来做周期机关（周期激光）。
    /// target 必须是子物体：把自身关掉的话 Update 也会一起停。
    /// </summary>
    public sealed class TimedToggler : MonoBehaviour
    {
        [SerializeField] GameObject target;
        [SerializeField] float onSeconds = 1.5f;
        [SerializeField] float offSeconds = 1.5f;
        [SerializeField] float phaseOffset;

        public bool IsOn { get; private set; }

        void OnEnable() => Apply(Compute());

        void Update() => Apply(Compute());

        bool Compute()
        {
            float cycle = onSeconds + offSeconds;
            if (cycle <= 0.01f) return true;

            return Mathf.Repeat(Time.time + phaseOffset, cycle) < onSeconds;
        }

        void Apply(bool on)
        {
            IsOn = on;
            if (target != null && target.activeSelf != on) target.SetActive(on);
        }
    }
}
