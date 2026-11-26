using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 关卡内常驻的教学提示卡：标题 + 正文 + 页脚。
    /// 文案由 LevelSession 在换关时推送；没有文案的关卡整张卡片隐藏。
    /// 进关时等画布绘制动画结束才出现，达成胜利条件后淡出，让出整幅画布。
    /// </summary>
    public sealed class TutorialHintUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] Image plate;
        [SerializeField] Text titleText;
        [SerializeField] Text bodyText;
        [SerializeField] Text footerText;

        [Header("底板")]
        [Tooltip("1~2 行短文案用的宽卡")]
        [SerializeField] Sprite widePlate;
        [Tooltip("3 行以上文案用的方卡")]
        [SerializeField] Sprite squarePlate;
        [SerializeField] Vector2 wideSize = new Vector2(576f, 216f);
        [SerializeField] Vector2 squareSize = new Vector2(420f, 280f);
        [SerializeField] int wideMaxLines = 2;

        public void Show(LevelConfig config, int displayIndex)
        {
            if (config == null || string.IsNullOrEmpty(config.TutorialTitle))
            {
                Hide();
                return;
            }

            if (titleText != null) titleText.text = config.TutorialTitle;
            if (bodyText != null) bodyText.text = config.TutorialBody;
            if (footerText != null)
                footerText.text = "TUTORIAL " + Mathf.Max(1, displayIndex).ToString("00") + " / " + config.TutorialTitle;

            ApplyPlate(config.TutorialBody);

            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (group != null) group.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>胜利时淡出，让后面的画布展开不受遮挡。协程由 LevelSession 驱动。</summary>
        public IEnumerator FadeOutRoutine(float seconds)
        {
            if (group == null || !gameObject.activeSelf) yield break;

            float from = group.alpha;
            if (from <= 0.001f)
            {
                Hide();
                yield break;
            }

            float duration = Mathf.Max(0.01f, seconds);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, 0f, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            Hide();
        }

        void Awake()
        {
            ApplyFont();
            Hide();
        }

        void ApplyPlate(string body)
        {
            if (plate == null) return;

            bool useSquare = squarePlate != null && LineCount(body) > wideMaxLines;
            plate.sprite = useSquare ? squarePlate : widePlate;
            plate.type = Image.Type.Simple;
            plate.preserveAspect = false;

            var rt = plate.rectTransform;
            rt.sizeDelta = useSquare ? squareSize : wideSize;
        }

        static int LineCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 1;

            int lines = 1;
            for (int i = 0; i < text.Length; i++)
                if (text[i] == '\n') lines++;

            return lines;
        }

        /// <summary>编辑器里序列化保存的字体优先级更高，这里是运行时兜底。</summary>
        void ApplyFont()
        {
            var font = PopupFont.Project;
            if (font == null) return;

            if (titleText != null && titleText.font == null) titleText.font = font;
            if (bodyText != null && bodyText.font == null) bodyText.font = font;
            if (footerText != null && footerText.font == null) footerText.font = font;
        }
    }
}
