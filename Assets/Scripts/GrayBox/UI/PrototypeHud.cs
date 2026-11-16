using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 临时调试 HUD。失败提示已经交给 FailPopup，这里只保留状态行。
    /// 正式 UI 做好之后整块删掉。
    /// </summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] LevelSession session;
        [SerializeField] InputRouter router;

        GUIStyle status;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();
            if (router == null) router = FindObjectOfType<InputRouter>();
        }

        void OnGUI()
        {
            if (session == null) return;

            if (status == null)
            {
                status = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    font = PopupFont.Cjk
                };
                status.normal.textColor = Color.white;
            }

            var level = session.Current;
            string levelText = level != null ? $"{level.LevelId}  {level.DisplayName}" : "-";
            string controlText = router != null ? router.ActiveSide.ToString() : "-";

            Draw(8, 8, 520, 28);
            GUI.Label(new Rect(16, 12, 520, 24),
                $"{levelText}    lives {session.LivesRemaining}/{session.LivesPerLevel}    ctrl {controlText}", status);
        }

        static void Draw(float x, float y, float w, float h)
        {
            var previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
