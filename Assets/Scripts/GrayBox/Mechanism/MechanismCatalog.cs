using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 全部机关档案的总表。教程与图鉴只认这一份，
    /// 用 MechanismKind 查，不依赖任何场景对象。
    /// </summary>
    [CreateAssetMenu(menuName = "GrayBox/Mechanism Catalog", fileName = "MechanismCatalog")]
    public sealed class MechanismCatalog : ScriptableObject
    {
        [SerializeField] List<MechanismInfo> entries = new List<MechanismInfo>();

        public IReadOnlyList<MechanismInfo> Entries => entries;
        public int Count => entries.Count;

        public MechanismInfo Find(MechanismKind kind)
        {
            if (kind == MechanismKind.None) return null;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.Kind == kind) return entry;
            }

            return null;
        }

        public string TutorialHint(MechanismKind kind) => Find(kind)?.TutorialHint ?? "";

        public void SetEntries(List<MechanismInfo> newEntries) => entries = newEntries;
    }
}
