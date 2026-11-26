using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 双压板的总闸：两块石压板都被人踩住时，才驱动相连的 ISwitchTarget。
    /// 两块通常分在左右半屏，逼两个人分头行动——而他们互相看不见。
    /// oneShot 打开时，只要同时踩过一次就永久保持。
    /// </summary>
    public sealed class TandemGate : MonoBehaviour
    {
        [SerializeField] TandemPlate plateA;
        [SerializeField] TandemPlate plateB;

        [Tooltip("挂任意实现了 ISwitchTarget 的组件")]
        [SerializeField] MonoBehaviour[] targets;

        [Tooltip("两块同时踩过一次后永久保持，不再要求站着")]
        [SerializeField] bool oneShot;

        bool open;
        bool latched;

        public MechanismKind Kind => MechanismKind.TandemPlate;
        public bool Open => open;

        void Update()
        {
            if (oneShot && latched) return;

            bool both = plateA != null && plateB != null && plateA.Pressed && plateB.Pressed;
            if (both == open) return;

            open = both;
            if (both && oneShot) latched = true;
            Apply(open);
        }

        void OnDisable()
        {
            if (!open) return;

            open = false;
            latched = false;
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

        public void Configure(TandemPlate a, TandemPlate b, MonoBehaviour[] newTargets, bool latch = false)
        {
            plateA = a;
            plateB = b;
            targets = newTargets;
            oneShot = latch;
        }
    }
}
