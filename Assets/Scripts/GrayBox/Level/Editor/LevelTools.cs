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

                    foreach (var goalName in new[] { "Goal_Left", "Goal_Right" })
                    {
                        var goal = FindDeep(root, goalName);
                        if (goal == null)
                        {
                            report.AppendLine($"  {label} 预制缺少 {goalName}");
                            problems++;
                        }
                        else if (goal.GetComponent<LevelGoal>() == null)
                        {
                            report.AppendLine($"  {label} 的 {goalName} 上没有 LevelGoal 组件");
                            problems++;
                        }
                    }

                    problems += ValidateHazards(root, label, report);
                    problems += ValidateCheckpoints(root, label, report);

                    if (!level.Enabled) report.AppendLine($"  {label} 已标记为禁用");
                }
            }

            report.AppendLine(problems == 0 ? "结果：全部通过" : $"结果：{problems} 处问题");

            if (problems == 0) Debug.Log("[GrayBox]\n" + report);
            else Debug.LogWarning("[GrayBox]\n" + report);
        }

        static int ValidateHazards(Transform root, string label, StringBuilder report)
        {
            int problems = 0;
            var hazards = root.GetComponentsInChildren<Hazard>(true);
            if (hazards.Length == 0) return 0;

            // 角色是 0.8 x 0.8，出生点周围这么大一块都不能有机关
            const float playerSize = 0.8f;
            var spawns = new List<Transform>();
            foreach (var name in new[] { "Spawn_Left", "Spawn_Right" })
            {
                var spawn = FindDeep(root, name);
                if (spawn != null) spawns.Add(spawn);
            }

            foreach (var hazard in hazards)
            {
                var col = hazard.GetComponent<Collider2D>();
                if (col == null)
                {
                    report.AppendLine($"  {label} 机关 {hazard.name} 上没有碰撞体");
                    problems++;
                    continue;
                }

                if (!col.isTrigger)
                {
                    report.AppendLine($"  {label} 机关 {hazard.name} 的碰撞体不是 Trigger，会挡住角色");
                    problems++;
                }

                var hazardBounds = GetWorldBounds(col);
                foreach (var spawn in spawns)
                {
                    var spawnBounds = new Bounds(spawn.position, new Vector3(playerSize, playerSize, 1f));
                    if (!hazardBounds.Intersects(spawnBounds)) continue;

                    report.AppendLine($"  {label} 出生点 {spawn.name} 与机关 {hazard.name} 重叠或贴得太近");
                    problems++;
                }
            }

            return problems;
        }

        static int ValidateCheckpoints(Transform root, string label, StringBuilder report)
        {
            int problems = 0;
            var checkpoints = root.GetComponentsInChildren<Checkpoint>(true);
            if (checkpoints.Length == 0) return 0;

            var hazardBounds = new List<(string name, Bounds bounds)>();
            foreach (var hazard in root.GetComponentsInChildren<Hazard>(true))
            {
                var hc = hazard.GetComponent<Collider2D>();
                if (hc != null) hazardBounds.Add((hazard.name, GetWorldBounds(hc)));
            }

            var seen = new HashSet<(PlayerSide, int)>();

            foreach (var checkpoint in checkpoints)
            {
                var col = checkpoint.GetComponent<Collider2D>();
                if (col == null)
                {
                    report.AppendLine($"  {label} 存档点 {checkpoint.name} 上没有碰撞体");
                    problems++;
                    continue;
                }

                if (!col.isTrigger)
                {
                    report.AppendLine($"  {label} 存档点 {checkpoint.name} 的碰撞体不是 Trigger");
                    problems++;
                }

                if (!seen.Add((checkpoint.Side, checkpoint.Order)))
                {
                    report.AppendLine($"  {label} 存档点 {checkpoint.name} 与同侧其他存档点 order={checkpoint.Order} 重复");
                    problems++;
                }

                var bounds = GetWorldBounds(col);
                foreach (var (hazardName, hazardBound) in hazardBounds)
                {
                    if (!bounds.Intersects(hazardBound)) continue;

                    report.AppendLine($"  {label} 存档点 {checkpoint.name} 与机关 {hazardName} 重叠，复活后会立刻再死");
                    problems++;
                }
            }

            return problems;
        }

        /// <summary>
        /// 预制资产不在物理世界里，Collider2D.bounds 是零尺寸，
        /// 所以用 size × lossyScale 自己算。
        /// </summary>
        static Bounds GetWorldBounds(Collider2D col)
        {
            var t = col.transform;
            var scale = t.lossyScale;

            if (col is BoxCollider2D box)
            {
                var size = new Vector3(
                    Mathf.Abs(box.size.x * scale.x),
                    Mathf.Abs(box.size.y * scale.y),
                    1f);
                var center = t.position + new Vector3(box.offset.x * scale.x, box.offset.y * scale.y, 0f);
                return new Bounds(center, size);
            }

            return new Bounds(t.position, Vector3.zero);
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;

            return null;
        }
    }
}
