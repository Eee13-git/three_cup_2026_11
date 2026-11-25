using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeCup.UI
{
    /// <summary>
    /// 选关格子上的手绘墨圈：
    /// - alwaysOn：进度关常驻显示（玩家当前玩到的那一关）；
    /// - 其余可玩格子在悬停时显示；
    /// - 锁定格子关闭悬停，不显示墨圈。
    /// </summary>
    public sealed class CellHoverInk : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] GameObject ink;

        bool alwaysOn;
        bool hovering;
        bool hoverEnabled = true;

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
            if (ink != null) ink.SetActive(alwaysOn || (hoverEnabled && hovering));
        }
    }
}
