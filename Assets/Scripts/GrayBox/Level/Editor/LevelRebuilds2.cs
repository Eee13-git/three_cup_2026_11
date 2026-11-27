using UnityEditor;
using UnityEngine;
using ThreeCup.GrayBox;

namespace ThreeCup.EditorTools
{
    /// <summary>
    /// L022–L024 第三版：统一的「四层回环」骨架。
    /// 每侧都是：出生台 → 升降台 → 一层 → 游动平台 → 二层 → 游动平台 → 三层（开关/压板）
    /// → 从三层坠回一层、再落回出生台 → 走过底层断桥 → 终点。
    /// 开关/压板放在三层，爬升是强制的，不能从底层跳过终点。
    /// </summary>
    public static class LevelRebuilds2
    {
        const string PrefabDir = "Assets/Prefabs/GrayBox/";
        const string ConfigDir = "Assets/GameData/Levels/";

        [MenuItem("Tools/GrayBox/Rebuild2/L022 回旋塔")]
        public static void BuildL022()
        {
            var root = BuildSkeleton("L022", out var terrain, out var mechanisms, out var goals);
            BuildLadders(mechanisms);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -6.4f, -4.6f, -3.00f);
            var bridgeRight = Bridge(mechanisms, "Bridge_Right", 2.2f, 3.4f, -3.00f);

            // 双压板放在两侧三层：两块同时踩住才长出两段补笔带，踩过一次后永久保持
            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-1.8f, 3.01f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(4.0f, 3.01f));
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);

            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate_Plates");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            Finish(root, goals, "L022", "回旋塔", new Vector2(8.2f, 4.04f), new Vector2(-8.2f, 4.04f));
        }

        [MenuItem("Tools/GrayBox/Rebuild2/L023 折线")]
        public static void BuildL023()
        {
            var root = LevelBuildKit.NewRoot("L023", out var terrain, out var mechanisms, out var goals);
            var camera = root.GetComponent<LevelCamera>();
            LevelBuildKit.SetAll(camera, ("size", 7f), ("boundsMargin", 0.2f));

            // ---- 左半屏：顶层起点 → 三级下降 → 底层补笔带桥 → 折回左端污渍 ----
            LevelBuildKit.Platform(terrain, "Plat_L0", -12.55f, -10f, 3.32f);
            LevelBuildKit.Platform(terrain, "Plat_L1", -9.4f, -7.2f, 1.40f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -6.6f, -4.4f, -1.16f);
            LevelBuildKit.Platform(terrain, "Plat_L3", -2.6f, -0.2f, -3.08f);
            LevelBuildKit.Platform(terrain, "Plat_LG", -12.55f, -6.2f, -3.08f);

            // ---- 右半屏：底层经补笔带桥 → 未干块 → 长廊 → 竖直游动平台 → 高层 → 蘑菇上顶层 ----
            LevelBuildKit.Platform(terrain, "Plat_R0", 0.2f, 3.4f, -3.08f);
            LevelBuildKit.Platform(terrain, "Plat_R1", 7.0f, 9.4f, -3.08f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 6.4f, 9.3f, -1.16f);
            LevelBuildKit.Platform(terrain, "Plat_R3", 2.6f, 5.7f, 1.40f);
            LevelBuildKit.Platform(terrain, "Plat_R4", 1.2f, 3.3f, 3.00f);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -6.2f, -2.6f, -3.08f);
            var bridgeRight = Bridge(mechanisms, "Bridge_Right", 3.4f, 7.0f, -3.08f);

            // 交叉接线：B 出生台的开关开 A 的桥，A 落到底层后的开关开 B 的桥
            var switchLeft = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Left", new Vector2(-2.0f, -2.48f));
            LevelBuildKit.SetArray(switchLeft.GetComponent<Switch>(), "targets", bridgeRight.GetComponent<ToggleBlock>());
            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.4f, -2.48f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            // 未干块：A 的第三次下降落点；B 的爬升台阶
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L1", new Vector2(-3.6f, -3.45f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(9.9f, -2.81f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(10.9f, -2.17f));

            var moverRight = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(6.08f, -1.48f));
            LevelBuildKit.SetAll(moverRight.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 3.20f)),
                ("oneWaySeconds", 3.0f));

            var springRight = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(4.2f, 1.49f));
            LevelBuildKit.Set(springRight.GetComponent<SpringBed>(), "bounceVelocity", 14f);

            // 折痕横在长廊下方：站在长廊上时视窗翻到 −3.44，正好看到下一层的落点
            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-5.5f, -2.30f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-1f, 0f)),
                ("localEnd", new Vector2(1f, 0f)),
                ("influence", 1.25f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            var lens = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-8.3f, 1.74f));
            LevelBuildKit.SetAll(lens.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-9.6f, -2.83f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(8.8f, -0.91f));

            var checkpointLeft = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Left", new Vector2(-5.9f, -0.72f));
            LevelBuildKit.SetAll(checkpointLeft.GetComponent<Checkpoint>(), ("side", PlayerSide.Left), ("order", 0));
            var checkpointRight = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Right", new Vector2(7.7f, -0.72f));
            LevelBuildKit.SetAll(checkpointRight.GetComponent<Checkpoint>(), ("side", PlayerSide.Right), ("order", 0));

            Lamp(mechanisms, "Beacon_Lamp_Left_Bottom", new Vector2(-1.2f, -2.63f), "Glow_Left_Bottom", new Vector2(1.2f, -2.63f));
            Lamp(mechanisms, "Beacon_Lamp_Left_Mid", new Vector2(-4.8f, -0.71f), "Glow_Left_Mid", new Vector2(4.8f, -0.71f));
            Lamp(mechanisms, "Beacon_Lamp_Right", new Vector2(6.7f, -0.71f), "Glow_Right", new Vector2(-6.7f, -0.71f));

            LevelBuildKit.Goal(goals, true, -11.0f, -3.08f);
            LevelBuildKit.Goal(goals, false, 2.0f, 3.00f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-11.4f, 3.64f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.2f, -2.76f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.08f, 3.64f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-11.28f, -2.76f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L023.prefab");
            Config("L023", "折线", new Vector2(12.48f, 0f), new Vector2(-12.48f, 0f));
            Debug.Log("[GrayBox] 已重建 L023 折线（L002 规模）");
        }

        [MenuItem("Tools/GrayBox/Rebuild2/L024 终章")]
        public static void BuildL024()
        {
            var root = LevelBuildKit.NewRoot("L024", out var terrain, out var mechanisms, out var goals);
            var camera = root.GetComponent<LevelCamera>();
            LevelBuildKit.SetAll(camera, ("size", 7f), ("boundsMargin", 0.2f));

            // ---- 左半屏：未干块两级 → 升降台 → 长廊 → 蘑菇上高层压板 → 掉回长廊 → 底层桥 → 污渍 ----
            LevelBuildKit.Platform(terrain, "Plat_L0", -12.55f, -9.8f, -4.36f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -7.4f, -3.4f, -1.80f);
            LevelBuildKit.Platform(terrain, "Plat_L3", -9.6f, -7.2f, 0.76f);
            LevelBuildKit.Platform(terrain, "Plat_LG", -2.4f, -0.2f, -4.36f);

            // ---- 右半屏：升降台上长廊 → 再升降台到高层压板 → 掉回底层桥 → 污渍 ----
            LevelBuildKit.Platform(terrain, "Plat_R0", 8.6f, 12.4f, -3.08f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 4.6f, 8.4f, -1.16f);
            LevelBuildKit.Platform(terrain, "Plat_R3", 2.6f, 3.9f, 0.76f);
            LevelBuildKit.Platform(terrain, "Plat_RG", 0.2f, 2.0f, -3.08f);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -6.0f, -2.4f, -4.36f);
            var bridgeRight = Bridge(mechanisms, "Bridge_Right", 2.0f, 5.6f, -3.08f);

            // 双压板各在两侧高层：两块同时踩住才长出两段补笔带，踩过一次后永久保持
            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-8.4f, 1.01f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(3.2f, 1.01f));
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);

            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate_Plates");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L1", new Vector2(-9.2f, -4.09f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L2", new Vector2(-8.1f, -3.45f));

            var liftLeft = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-7.8f, -3.40f));
            LevelBuildKit.SetAll(liftLeft.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 1.92f),
                ("speed", 2.5f));

            var springLeft = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-6.2f, -1.71f));
            LevelBuildKit.Set(springLeft.GetComponent<SpringBed>(), "bounceVelocity", 14f);

            var lens = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-5.2f, -1.46f));
            LevelBuildKit.SetAll(lens.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(8.84f, -3.40f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var liftRight2 = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right2", new Vector2(4.4f, -1.48f));
            LevelBuildKit.SetAll(liftRight2.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(6.4f, -0.91f));

            var checkpointLeft = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Left", new Vector2(-4.2f, -1.36f));
            LevelBuildKit.SetAll(checkpointLeft.GetComponent<Checkpoint>(), ("side", PlayerSide.Left), ("order", 0));
            var checkpointRight = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Right", new Vector2(5.2f, -0.72f));
            LevelBuildKit.SetAll(checkpointRight.GetComponent<Checkpoint>(), ("side", PlayerSide.Right), ("order", 0));

            Lamp(mechanisms, "Beacon_Lamp_Left", new Vector2(-9.2f, 1.21f), "Glow_Left", new Vector2(9.4f, 1.21f));
            Lamp(mechanisms, "Beacon_Lamp_Right", new Vector2(7.8f, -0.71f), "Glow_Right", new Vector2(-7.8f, -0.71f));

            LevelBuildKit.Goal(goals, true, -1.4f, -4.36f);
            LevelBuildKit.Goal(goals, false, 1.0f, -3.08f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-11.2f, -4.04f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(9.8f, -2.76f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.28f, -4.04f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-2.68f, -2.76f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L024.prefab");
            Config("L024", "终章", new Vector2(12.48f, 0f), new Vector2(-12.48f, 0f));
            Debug.Log("[GrayBox] 已重建 L024 终章（L002 规模）");
        }

        static GameObject BuildSkeleton(string id, out Transform terrain, out Transform mechanisms, out Transform goals)
        {
            var root = LevelBuildKit.NewRoot(id, out terrain, out mechanisms, out goals);

            // 左：终点台 − 断桥 − 出生台；一层/二层/三层
            LevelBuildKit.Platform(terrain, "Plat_LGoal", -7.8f, -6.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LSpawn", -4.6f, -3.0f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L1", -3.6f, -1.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -6.6f, -4.6f, 0.84f);
            LevelBuildKit.Platform(terrain, "Plat_L3", -3.0f, -0.6f, 2.76f);

            // 右：出生台 − 断桥 − 终点台；一层/二层/三层
            LevelBuildKit.Platform(terrain, "Plat_RSpawn", 0.2f, 2.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RGoal", 3.4f, 4.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_R1", 2.0f, 4.4f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 4.6f, 6.4f, 0.84f);
            LevelBuildKit.Platform(terrain, "Plat_R3", 3.0f, 5.0f, 2.76f);

            return root;
        }

        static void BuildLadders(Transform mechanisms)
        {
            var liftLeft = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-4.2f, -3.32f));
            LevelBuildKit.SetAll(liftLeft.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var moverLeft1 = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_L1", new Vector2(-3.2f, -1.40f));
            LevelBuildKit.SetAll(moverLeft1.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            var moverLeft2 = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_L2", new Vector2(-5.2f, 0.52f));
            LevelBuildKit.SetAll(moverLeft2.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(1.6f, -3.32f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var moverRight1 = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_R1", new Vector2(4.0f, -1.40f));
            LevelBuildKit.SetAll(moverRight1.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            var moverRight2 = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_R2", new Vector2(5.6f, 0.52f));
            LevelBuildKit.SetAll(moverRight2.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));
        }

        static void Finish(GameObject root, Transform goals, string id, string name, Vector2 leftOffset, Vector2 rightOffset)
        {
            LevelBuildKit.Goal(goals, true, -7.1f, -3.00f);
            LevelBuildKit.Goal(goals, false, 4.0f, -3.00f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-3.4f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(0.6f, -2.68f));

            // 视窗固定 ±8.2 偏移：角色在整个平台范围内移动时，窗口中心都留在对面半屏，
            // 不会因为走到自己半屏的边缘而跨过中线被裁回自己这一侧。
            LevelBuildKit.Preview(root.transform, true, new Vector2(-3.4f + 8.2f, -2.68f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(0.6f - 8.2f, -2.68f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + id + ".prefab");
            Config(id, name, leftOffset, rightOffset);
            Debug.Log($"[GrayBox] 已重建 {id} {name}");
        }

        static GameObject Bridge(Transform parent, string name, float x1, float x2, float topY)
        {
            var bridge = LevelBuildKit.Part(parent, "ToggleBlock", name, new Vector2((x1 + x2) * 0.5f, topY - 0.355f));
            bridge.transform.localScale = new Vector3(Mathf.Abs(x2 - x1) / 3.64f, 1f, 1f);
            LevelBuildKit.SetAll(bridge.GetComponent<ToggleBlock>(),
                ("startActive", false),
                ("growFromLeft", true));
            return bridge;
        }

        /// <summary>
        /// 照面灯 + 对面半屏的光斑。绑了光斑之后，遮蔽层开的孔就看光斑的位置和渲染尺寸，
        /// 所以光斑摆哪儿就照哪儿，灯本身只负责开关。
        /// </summary>
        static void Lamp(Transform parent, string lampName, Vector2 lampPos, string glowName, Vector2 glowPos,
            float glowWidth = 3.0f, float glowHeight = 2.2f)
        {
            var lamp = LevelBuildKit.Part(parent, "Beacon_Lamp", lampName, lampPos);
            var glow = LevelBuildKit.Part(parent, "Beacon_Glow", glowName, glowPos);

            var sr = glow.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Vector2 baseSize = sr.sprite.bounds.size;
                if (baseSize.x > 0.001f && baseSize.y > 0.001f)
                    glow.transform.localScale = new Vector3(glowWidth / baseSize.x, glowHeight / baseSize.y, 1f);
            }

            var beacon = lamp.GetComponent<BeaconSwitch>();
            LevelBuildKit.SetAll(beacon,
                ("targetSize", new Vector2(glowWidth, glowHeight)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));
            beacon.SetRemoteGlow(glow);
        }

        static void Config(string levelId, string displayName, Vector2 leftOffset, Vector2 rightOffset)
        {
            var path = ConfigDir + levelId + ".asset";
            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
            if (config == null) throw new System.Exception("找不到关卡配置 " + path);

            var so = new SerializedObject(config);
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("windowSize").vector2Value = new Vector2(4f, 3f);
            so.FindProperty("leftWindowOffset").vector2Value = leftOffset;
            so.FindProperty("rightWindowOffset").vector2Value = rightOffset;
            so.FindProperty("usePreviewOffsets").boolValue = true;
            so.FindProperty("leftWindowInverted").boolValue = false;
            so.FindProperty("rightWindowInverted").boolValue = false;
            so.FindProperty("leftWindowInvertedY").boolValue = false;
            so.FindProperty("rightWindowInvertedY").boolValue = false;
            so.FindProperty("windowMirrorAxisX").floatValue = 0f;
            so.FindProperty("windowMirrorAxisY").floatValue = 0f;
            so.FindProperty("tutorialTitle").stringValue = "";
            so.FindProperty("tutorialBody").stringValue = "";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }
}
