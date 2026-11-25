using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 主控角色的高光：用同一个 sprite 放大一圈、染成黄色垫在角色下面，
    /// 形状跟着角色的当前帧走，所以看起来是沿轮廓的黄光而不是一块方牌子。
    /// 角色自己的颜色不动，两个角色都保持可读。
    /// </summary>
    public sealed class PlayerVisual : MonoBehaviour
    {
        [SerializeField] PlayerSide side = PlayerSide.Left;
        [SerializeField] GameObject highlight;
        [SerializeField] SpriteRenderer targetRenderer;

        SpriteRenderer highlightRenderer;
        InputRouter router;

        public void Configure(PlayerSide newSide, GameObject highlightObject)
        {
            side = newSide;
            highlight = highlightObject;
            SetHighlight(false);
        }

        void Awake()
        {
            ResolveHighlight();
            if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
            if (highlight != null) highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        }

        void ResolveHighlight()
        {
            if (highlight != null) return;

            var found = transform.Find("Highlight");
            if (found != null) highlight = found.gameObject;
        }

        void Update()
        {
            ResolveHighlight();
            SyncOutline();

            if (router == null) router = FindObjectOfType<InputRouter>();
            if (router == null) return;

            SetHighlight(router.IsControlled(side));
        }

        /// <summary>轮廓光跟着角色的帧和朝向，不然会露馅。</summary>
        void SyncOutline()
        {
            if (targetRenderer == null || highlight == null) return;
            if (highlightRenderer == null) highlightRenderer = highlight.GetComponent<SpriteRenderer>();
            if (highlightRenderer == null) return;

            if (highlightRenderer.sprite != targetRenderer.sprite)
                highlightRenderer.sprite = targetRenderer.sprite;

            if (highlightRenderer.flipX != targetRenderer.flipX)
                highlightRenderer.flipX = targetRenderer.flipX;
        }

        void SetHighlight(bool visible)
        {
            if (highlight == null) return;
            if (highlight.activeSelf == visible) return;

            highlight.SetActive(visible);
        }
    }
}
