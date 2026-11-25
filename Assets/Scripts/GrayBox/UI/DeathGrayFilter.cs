using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 角色死亡（含命数耗尽的失败）期间把游戏画面去色。
    /// 靠 URP 全局 Volume 的 Color Adjustments 实现；HUD、死亡弹窗与失败弹窗是 Screen Space - Overlay 的 Canvas，
    /// 在后处理之后绘制，所以保持原色。失败时画面另外由 FailPopup 的黑色蒙层压暗。
    /// </summary>
    public sealed class DeathGrayFilter : MonoBehaviour
    {
        [SerializeField] LevelSession session;
        [SerializeField] Volume volume;
        [SerializeField] Camera targetCamera;
        [SerializeField] float fadeSeconds = 0.15f;

        UniversalAdditionalCameraData cameraData;
        float weight;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();
            if (volume == null) volume = GetComponent<Volume>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null)
                cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();

            Apply(0f);
        }

        void Update()
        {
            // 暂停时弹窗会隐藏，这里跟着一起淡出；命数耗尽的失败界面同样保持去色
            bool gray = session != null
                        && (session.DeathPopupVisible || session.Failed)
                        && Time.timeScale > 0f;
            float target = gray ? 1f : 0f;
            float step = fadeSeconds > 0f ? Time.unscaledDeltaTime / fadeSeconds : 1f;

            float next = Mathf.MoveTowards(weight, target, step);
            if (!Mathf.Approximately(next, weight)) Apply(next);
        }

        void Apply(float value)
        {
            weight = value;
            if (volume != null) volume.weight = value;

            // 后处理只在需要时开，避免平时每帧多一次全屏处理
            if (cameraData != null)
            {
                bool needed = value > 0.0001f;
                if (cameraData.renderPostProcessing != needed)
                    cameraData.renderPostProcessing = needed;
            }
        }
    }
}
