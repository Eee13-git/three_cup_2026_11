using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 死亡弹窗。临时实现（IMGUI），正式 UI 做好后整块删掉。
    /// 弹出期间 LevelSession 已经锁住了两个角色的操作。
    /// </summary>
    public sealed class DeathPopup : MonoBehaviour
    {
        [SerializeField] LevelSession session;

        GUIStyle title;
        GUIStyle subtitle;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();
        }

        void OnGUI()
        {
            if (session == null || !session.DeathPopupVisible) return;

            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 52,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    font = PopupFont.Cjk
                };
                title.normal.textColor = new Color(1f, 0.36f, 0.32f);

                subtitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    font = PopupFont.Cjk
                };
                subtitle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
            }

            var previous = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            float w = Mathf.Min(620f, Screen.width - 80f);
            float h = 220f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.color = new Color(0.10f, 0.03f, 0.04f, 0.96f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

            GUI.color = new Color(0.95f, 0.35f, 0.32f, 1f);
            GUI.DrawTexture(new Rect(x, y, w, 4f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + h - 4f, w, 4f), Texture2D.whiteTexture);

            GUI.color = previous;

            string who = session.DeathPopupSide == PlayerSide.Left ? "角色A" : "角色B";
            GUI.Label(new Rect(x, y + 48f, w, 72f), $"{who} 已死亡", title);
            GUI.Label(new Rect(x, y + 138f, w, 32f), "操作已锁定 2 秒", subtitle);
        }
    }
}
