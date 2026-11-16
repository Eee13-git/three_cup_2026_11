using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ThreeCup.GrayBox;

namespace ThreeCup.EditorTools
{
    public static class LevelTools
    {
        [MenuItem("Tools/GrayBox/Validate Levels")]
        public static void ValidateLevels()
        {
            var report = new StringBuilder();
            int problems = 0;

            var catalogGuids = AssetDatabase.FindAssets("t:LevelCatalog");
            if (catalogGuids.Length == 0)
            {
                Debug.LogWarning("[GrayBox] 工程里没有 LevelCatalog");
                return;
            }

            var seenIds = new Dictionary<string, string>();

            foreach (var catalogGuid in catalogGuids)
            {
                var catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuid);
                var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(catalogPath);
                report.AppendLine($"Catalog: {catalogPath} ({catalog.Count} 关)");

                for (int i = 0; i < catalog.Count; i++)
                {
                    var level = catalog.Get(i);
                    if (level == null)
                    {
                        report.AppendLine($"  [{i}] <空引用>");
                        problems++;
                        continue;
                    }

                    var label = $"[{i}] {level.LevelId} \"{level.DisplayName}\"";

                    if (string.IsNullOrWhiteSpace(level.LevelId))
                    {
                        report.AppendLine($"  {label} 缺少 levelId");
                        problems++;
                    }
                    else if (seenIds.TryGetValue(level.LevelId, out var other))
                    {
                        report.AppendLine($"  {label} levelId 与 {other} 重复");
                        problems++;
                    }
                    else
                    {
                        seenIds[level.LevelId] = catalogPath;
                    }

                    if (level.LevelPrefab == null)
                    {
                        report.AppendLine($"  {label} 没有指定关卡预制");
                        problems++;
                        continue;
                    }

                    var root = level.LevelPrefab.transform;
                    foreach (var marker in new[] { "Spawn_Left", "Spawn_Right" })
                    {
                        if (FindDeep(root, marker) == null)
                        {
                            report.AppendLine($"  {label} 预制缺少 {marker}");
                            problems++;
                        }
                    }

                    foreach (var portalName in new[] { "Portal_Left", "Portal_Right" })
                    {
                        var portal = FindDeep(root, portalName);
                        if (portal == null)
                        {
                            report.AppendLine($"  {label} 预制缺少 {portalName}");
                            problems++;
                        }
                        else if (portal.GetComponent<Portal>() == null)
                        {
                            report.AppendLine($"  {label} 的 {portalName} 上没有 Portal 组件");
                            problems++;
                        }
                    }

                    if (!level.Enabled) report.AppendLine($"  {label} 已标记为禁用");
                }
            }

            report.AppendLine(problems == 0 ? "结果：全部通过" : $"结果：{problems} 处问题");

            if (problems == 0) Debug.Log("[GrayBox]\n" + report);
            else Debug.LogWarning("[GrayBox]\n" + report);
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;

            return null;
        }
    }
}
