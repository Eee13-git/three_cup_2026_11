using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.UI
{
    /// <summary>
    /// 设置面板。关闭按钮调用 Close 隐藏自身，后续再往里填具体选项。
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Button closeButton;

        void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void Close() => gameObject.SetActive(false);
    }
}
