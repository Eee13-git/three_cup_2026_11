using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 命数耗尽的失败弹窗。临时实现（IMGUI），正式 UI 做好后整块删掉。
    /// </summary>
    public sealed class FailPopup : MonoBehaviour
    {
        [SerializeField] LevelSession session;

        GUIStyle title;
        GUIStyle hint;
        GUIStyle detail;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();
        }

        void OnGUI()
        {
            if (session == null || !session.Failed) return;

            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 56,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    font = PopupFont.Cjk
                };
                title.normal.textColor = new Color(1f, 0.34f, 0.30f);

                hint = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    alignment = TextAnchor.MiddleCenter,
                    font = PopupFont.Cjk
                };
                hint.normal.textColor = Color.white;

                detail = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter,
                    font = PopupFont.Cjk
                };
                detail.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
            }

            var previous = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            float w = Mathf.Min(680f, Screen.width - 80f);
            float h = 260f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.color = new Color(0.08f, 0.02f, 0.03f, 0.97f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

            GUI.color = new Color(0.95f, 0.30f, 0.27f, 1f);
            GUI.DrawTexture(new Rect(x, y, w, 5f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + h - 5f, w, 5f), Texture2D.whiteTexture);

            GUI.color = previous;

            string levelName = session.Current != null ? session.Current.DisplayName : "";
            GUI.Label(new Rect(x, y + 40f, w, 74f), "关卡失败", title);
            GUI.Label(new Rect(x, y + 128f, w, 34f), "按 R 重新开始本关", hint);
            GUI.Label(new Rect(x, y + 190f, w, 28f), $"{levelName}   生命已耗尽", detail);
        }
    }
}
