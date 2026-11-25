using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 局内左上角 HUD：关卡名 / 关卡 ID、剩余生命血滴、当前主控角色 chip（Left=A，Right=B）。
    /// 取代临时 IMGUI HUD。
    /// </summary>
    public sealed class LevelHud : MonoBehaviour
    {
        [SerializeField] LevelSession session;
        [SerializeField] InputRouter router;

        [Header("Level")]
        [SerializeField] Text levelNameText;
        [SerializeField] Text levelIdText;

        [Header("Lives")]
        [SerializeField] Transform dropsRoot;
        [SerializeField] Image dropTemplate;
        [SerializeField] Sprite dropFull;
        [SerializeField] Sprite dropEmpty;

        [Header("Control")]
        [SerializeField] GameObject chipA;
        [SerializeField] GameObject chipB;

        readonly List<Image> drops = new List<Image>();
        string lastLevelName;
        string lastLevelId;
        PlayerSide lastSide;
        bool sideInit;

        void Awake()
        {
            if (session == null) session = FindObjectOfType<LevelSession>();
            if (router == null) router = FindObjectOfType<InputRouter>();
            BuildDrops();
        }

        void OnEnable()
        {
            if (session != null) session.LivesChanged += OnLivesChanged;
        }

        void OnDisable()
        {
            if (session != null) session.LivesChanged -= OnLivesChanged;
        }

        void Start()
        {
            if (session != null) OnLivesChanged(session.LivesRemaining);
            UpdateLevel();
            UpdateChip(true);
        }

        void Update()
        {
            UpdateLevel();
            if (router != null && (!sideInit || router.ActiveSide != lastSide))
                UpdateChip(false);
        }

        void BuildDrops()
        {
            if (dropTemplate == null || dropsRoot == null) return;

            dropTemplate.gameObject.SetActive(false);
            drops.Clear();

            int count = session != null ? session.LivesPerLevel : 3;
            for (int i = 0; i < count; i++)
            {
                var d = Instantiate(dropTemplate, dropsRoot);
                d.gameObject.SetActive(true);
                drops.Add(d);
            }
        }

        void UpdateLevel()
        {
            var level = session != null ? session.Current : null;
            if (level == null) return;

            if (levelNameText != null && level.DisplayName != lastLevelName)
            {
                levelNameText.text = level.DisplayName;
                lastLevelName = level.DisplayName;
            }

            if (levelIdText != null && level.LevelId != lastLevelId)
            {
                levelIdText.text = level.LevelId;
                lastLevelId = level.LevelId;
            }
        }

        void OnLivesChanged(int remaining)
        {
            for (int i = 0; i < drops.Count; i++)
            {
                var sprite = i < remaining ? dropFull : dropEmpty;
                if (drops[i] != null && sprite != null) drops[i].sprite = sprite;
            }
        }

        void UpdateChip(bool force)
        {
            if (router == null) return;
            if (!force && sideInit && router.ActiveSide == lastSide) return;

            lastSide = router.ActiveSide;
            sideInit = true;

            bool left = router.ActiveSide == PlayerSide.Left;
            if (chipA != null) chipA.SetActive(left);
            if (chipB != null) chipB.SetActive(!left);
        }
    }
}
