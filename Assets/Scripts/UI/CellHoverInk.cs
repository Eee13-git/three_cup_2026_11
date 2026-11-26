using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// 选关格子上的手绘圈：
    /// - alwaysOn：进度关常驻显示（灰圈，玩家当前玩到的那一关）；
    /// - 其余可玩格子在悬停时显示（青绿圈）；
    /// - 悬停优先：进度关被悬停时也换成青绿圈；
    /// - 锁定格子关闭悬停，不显示圈。
    /// </summary>
    public sealed class CellHoverInk : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] GameObject ink;
        [SerializeField] Sprite progressSprite;
        [SerializeField] Sprite hoverSprite;

        bool alwaysOn;
        bool hovering;
        bool hoverEnabled = true;
        Image inkImage;

        void Awake() => Refresh();

        public void SetAlwaysOn(bool value)
        {
            alwaysOn = value;
            Refresh();
        }

        public void SetHoverEnabled(bool value)
        {
            hoverEnabled = value;
            if (!value) hovering = false;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            Refresh();
        }

        void Refresh()
        {
            bool showHover = hoverEnabled && hovering;
            bool showProgress = alwaysOn && !showHover;

            if (ink != null) ink.SetActive(showHover || showProgress);

            var image = ResolveImage();
            if (image == null) return;

            var sprite = showHover && hoverSprite != null ? hoverSprite : progressSprite;
            if (sprite != null) image.sprite = sprite;
        }

        Image ResolveImage()
        {
            if (inkImage == null && ink != null) inkImage = ink.GetComponent<Image>();
            return inkImage;
        }
    }
}
