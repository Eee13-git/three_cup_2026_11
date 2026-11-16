using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Data + position of one reveal rectangle. The fog shader reads the resolved
    /// rectangle every frame, so this component only has to describe it.
    /// </summary>
    [ExecuteAlways]
    public sealed class VisionWindow : MonoBehaviour
    {
        [SerializeField] Transform owner;
        [SerializeField] Vector2 offset = new Vector2(8f, 0f);

        public Transform Owner => owner;
        public Vector2 Offset => offset;
        public Vector2 Size => new Vector2(transform.localScale.x, transform.localScale.y);

        void LateUpdate() => Refresh();

        void OnValidate() => Refresh();

        public void Configure(Transform newOwner, Vector2 newOffset)
        {
            owner = newOwner;
            offset = newOffset;
            Refresh();
        }

        /// <summary>Where the hole should be this frame, independent of transform sync order.</summary>
        public Vector2 ResolveCenter() =>
            owner == null ? (Vector2)transform.position : (Vector2)owner.position + offset;

        public void Refresh()
        {
            Vector2 target = ResolveCenter();
            if (((Vector2)transform.position - target).sqrMagnitude < 0.000001f) return;

            transform.position = new Vector3(target.x, target.y, 0f);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.92f, 0.55f, 0.6f);
            Gizmos.DrawWireCube(ResolveCenter(), Size);
        }
    }
}
