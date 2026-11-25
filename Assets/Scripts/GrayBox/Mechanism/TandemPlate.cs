using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 单块石压板。被角色站住时下沉并变色，离开后弹回。
    /// 本身不驱动任何东西，由 TandemGate 检查两块是否同时被踩。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class TandemPlate : MonoBehaviour
    {
        [SerializeField] Transform plate;
        [SerializeField] SpriteRenderer visual;
        [SerializeField] float sinkDepth = 0.07f;
        [SerializeField] Color idleColor = Color.white;
        [SerializeField] Color pressedColor = new Color(0.72f, 0.78f, 0.84f, 1f);
        [SerializeField] float sinkSeconds = 0.1f;

        int occupants;
        Vector3 basePosition;
        float sink;

        public MechanismKind Kind => MechanismKind.TandemPlate;
        public bool Pressed => occupants > 0;

        void Awake()
        {
            if (plate == null) plate = transform;
            if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();

            basePosition = plate.localPosition;
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerMotor2D>() == null) return;

            occupants++;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerMotor2D>() == null) return;

            occupants = Mathf.Max(0, occupants - 1);
        }

        void Update()
        {
            float target = Pressed ? 1f : 0f;
            sink = Mathf.MoveTowards(sink, target, Time.deltaTime / Mathf.Max(sinkSeconds, 0.01f));

            if (plate != null)
                plate.localPosition = basePosition + Vector3.down * (sinkDepth * sink);

            if (visual != null)
                visual.color = Color.Lerp(idleColor, pressedColor, sink);
        }

        public void Configure(Transform newPlate, SpriteRenderer newVisual)
        {
            plate = newPlate;
            visual = newVisual;
            if (plate != null) basePosition = plate.localPosition;
        }
    }
}
