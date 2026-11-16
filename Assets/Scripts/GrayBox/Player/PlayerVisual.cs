using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// Shows a highlight ring around the character that is currently taking input.
    /// The character's own colour is never touched, so both sides stay equally readable.
    /// </summary>
    public sealed class PlayerVisual : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;
        [SerializeField] GameObject highlight;

        InputRouter router;

        public void Configure(PlayerSide newSide, GameObject highlightObject)
        {
            side = newSide;
            highlight = highlightObject;
            SetHighlight(false);
        }

        void Awake() => ResolveHighlight();

        void ResolveHighlight()
        {
            if (highlight != null) return;

            var found = transform.Find("Highlight");
            if (found != null) highlight = found.gameObject;
        }

        void Update()
        {
            ResolveHighlight();
            if (router == null) router = FindObjectOfType<InputRouter>();
            if (router == null) return;

            SetHighlight(router.IsControlled(side));
        }

        void SetHighlight(bool visible)
        {
            if (highlight == null) return;
            if (highlight.activeSelf == visible) return;

            highlight.SetActive(visible);
        }
    }
}
