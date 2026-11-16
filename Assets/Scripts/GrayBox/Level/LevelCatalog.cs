using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    [CreateAssetMenu(menuName = "GrayBox/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] List<LevelConfig> levels = new List<LevelConfig>();

        public IReadOnlyList<LevelConfig> Levels => levels;
        public int Count => levels.Count;

        public LevelConfig Get(int index) =>
            index >= 0 && index < levels.Count ? levels[index] : null;

        public void SetLevels(List<LevelConfig> newLevels)
        {
            levels = newLevels;
        }
    }
}
