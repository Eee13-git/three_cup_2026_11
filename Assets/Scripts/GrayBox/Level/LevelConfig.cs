using UnityEngine;

namespace ThreeCup.GrayBox
{
    [CreateAssetMenu(menuName = "GrayBox/Level Config", fileName = "Level_")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string levelId = "L001";
        [SerializeField] string displayName = "关卡 1";
        [SerializeField] bool enabled = true;

        [Header("Content")]
        [SerializeField] GameObject levelPrefab;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public bool Enabled => enabled;
        public GameObject LevelPrefab => levelPrefab;
    }
}
