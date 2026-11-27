using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    [CreateAssetMenu(menuName = "GrayBox/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] List<LevelConfig> levels = new List<LevelConfig>();

        [Tooltip("该目录内关卡点击后加载的游戏场景名")]
        [SerializeField] string sceneName = "Formal";

        public IReadOnlyList<LevelConfig> Levels => levels;
        public int Count => levels.Count;
        public string SceneName => sceneName;

        public LevelConfig Get(int index) =>
            index >= 0 && index < levels.Count ? levels[index] : null;

        public void SetLevels(List<LevelConfig> newLevels)
        {
            levels = newLevels;
        }
    }
}
