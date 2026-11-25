using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 双压板的总闸：两块石压板都被人踩住时，才驱动相连的 ISwitchTarget。
    /// 两块通常分在左右半屏，逼两个人分头行动——而他们互相看不见。
    /// </summary>
    public sealed class TandemGate : MonoBehaviour
    {
        [SerializeField] TandemPlate plateA;
        [SerializeField] TandemPlate plateB;

        [Tooltip("挂任意实现了 ISwitchTarget 的组件")]
        [SerializeField] MonoBehaviour[] targets;

        bool open;

        public MechanismKind Kind => MechanismKind.TandemPlate;
        public bool Open => open;

        void Update()
        {
            bool both = plateA != null && plateB != null && plateA.Pressed && plateB.Pressed;
            if (both == open) return;

            open = both;
            Apply(open);
        }

        void OnDisable()
        {
            if (!open) return;

            open = false;
            Apply(false);
        }

        void Apply(bool value)
        {
            if (targets == null) return;

            foreach (var target in targets)
            {
                if (target is ISwitchTarget switchTarget) switchTarget.SetActive(value);
            }
        }

        public void Configure(TandemPlate a, TandemPlate b, MonoBehaviour[] newTargets)
        {
            plateA = a;
            plateB = b;
            targets = newTargets;
        }
    }
}
