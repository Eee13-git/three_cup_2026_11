using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 一个机关的世界观档案。文案与逻辑分开存放，
    /// 教程提示取 TutorialHint，图鉴取 Lore / Rule，避免文案散落在各个脚本里。
    /// </summary>
    [CreateAssetMenu(menuName = "GrayBox/Mechanism Info", fileName = "Mech_")]
    public sealed class MechanismInfo : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] MechanismKind kind = MechanismKind.None;
        [SerializeField] string displayName = "";

        [Tooltip("图鉴标题下的一句副标")]
        [SerializeField] string subtitle = "";

        [Header("Text")]
        [Tooltip("世界观描述：这个东西在画里是什么、为什么会这样")]
        [SerializeField, TextArea(3, 8)] string lore = "";

        [Tooltip("规则说明：玩家能拿它做什么、要注意什么")]
        [SerializeField, TextArea(2, 6)] string rule = "";

        [Tooltip("教程里弹出的那一句话，尽量短")]
        [SerializeField, TextArea(1, 3)] string tutorialHint = "";

        [Header("Art")]
        [SerializeField] Sprite icon;

        public MechanismKind Kind => kind;
        public string DisplayName => displayName;
        public string Subtitle => subtitle;
        public string Lore => lore;
        public string Rule => rule;
        public string TutorialHint => tutorialHint;
        public Sprite Icon => icon;

        public void Configure(
            MechanismKind newKind,
            string newDisplayName,
            string newSubtitle,
            string newLore,
            string newRule,
            string newTutorialHint,
            Sprite newIcon)
        {
            kind = newKind;
            displayName = newDisplayName;
            subtitle = newSubtitle;
            lore = newLore;
            rule = newRule;
            tutorialHint = newTutorialHint;
            icon = newIcon;
        }
    }
}
