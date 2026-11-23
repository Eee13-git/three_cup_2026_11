using UnityEngine;

namespace ThreeCup.UI
{
    /// <summary>
    /// 选关进度：PlayerPrefs 存「玩家玩到第几关」。
    /// 全局序号从教学关第一关 = 1 开始，教学关在前、正式关在后连续计数。
    /// </summary>
    public static class LevelProgress
    {
        const string KEY = "Select.Progress";
        const string IDX_PREFIX = "Select.GlobalIdx.";

        /// <summary>已解锁的最大全局关卡序号（1-based），默认 1（只解锁第一关）。</summary>
        public static int UnlockedIndex
        {
            get => Mathf.Max(1, PlayerPrefs.GetInt(KEY, 1));
            set
            {
                PlayerPrefs.SetInt(KEY, Mathf.Max(1, value));
                PlayerPrefs.Save();
            }
        }

        public static void Unlock(int globalIndex)
        {
            if (globalIndex > UnlockedIndex) UnlockedIndex = globalIndex;
        }

        /// <summary>Select 场景构建全局序列时调用，登记每个 levelId 的全局序号，供通关解锁时反查。</summary>
        public static void StoreGlobalIndex(string levelId, int globalIndex)
        {
            if (string.IsNullOrEmpty(levelId)) return;
            PlayerPrefs.SetInt(IDX_PREFIX + levelId, globalIndex);
        }

        /// <summary>通关某关后调用：解锁它的下一关。</summary>
        public static void UnlockNext(string clearedLevelId)
        {
            if (string.IsNullOrEmpty(clearedLevelId)) return;
            int gi = PlayerPrefs.GetInt(IDX_PREFIX + clearedLevelId, 0);
            if (gi > 0) Unlock(gi + 1);
        }
    }
}
