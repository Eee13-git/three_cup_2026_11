using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// 主菜单按钮悬停反馈：配合 Button 的 SpriteSwap（悬停时换成 btn_plate 浅色贴图），
    /// 悬停时把按钮文字从浅色切成深色，保证浅色底上仍可读。
    /// </summary>
    public sealed class MenuButtonHoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Text label;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color hoverColor = new Color(0.16f, 0.14f, 0.12f, 1f);

        void Awake()
        {
            if (label == null) label = GetComponentInChildren<Text>(true);
            Apply(normalColor);
        }

        public void OnPointerEnter(PointerEventData eventData) => Apply(hoverColor);

        public void OnPointerExit(PointerEventData eventData) => Apply(normalColor);

        void Apply(Color c)
        {
            if (label != null) label.color = c;
        }
    }
}
