using UnityEditor;
using UnityEngine;
using ThreeCup.GrayBox;

namespace ThreeCup.EditorTools
{
    /// <summary>
    /// L020–L024 重做版：纵向落差为主，每关结构不同，全部遵守
    /// 「上下重叠的平台至少相隔 1.92（头顶余量 1.28）」「机关离出生点 ≥0.7」。
    /// </summary>
    public static class LevelRebuilds
    {
        const string PrefabDir = "Assets/Prefabs/GrayBox/";
        const string ConfigDir = "Assets/GameData/Levels/";

        [MenuItem("Tools/GrayBox/Rebuild/L020 深井")]
        public static void BuildL020()
        {
            var root = LevelBuildKit.NewRoot("L020", out var terrain, out var mechanisms, out var goals);

            // 左：A 从最高层往下走（1.48 → −0.44 → −3.00）
            LevelBuildKit.Platform(terrain, "Plat_LT", -7.8f, -5.0f, 1.48f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -5.4f, -2.6f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_LB1", -7.8f, -4.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LB2", -2.8f, -0.2f, -3.00f);

            // 右：B 从底层往上爬（−3.00 → −0.44 → 1.48）
            LevelBuildKit.Platform(terrain, "Plat_RB", 0.2f, 3.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.4f, 5.2f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 6.0f, 7.8f, 1.48f);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -4.6f, -2.8f, -3.00f);
            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.4f, -2.40f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(2.9f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 3.20f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(5.6f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-3.7f, -2.76f));

            LevelBuildKit.Goal(goals, true, -5.6f, -3.00f);
            LevelBuildKit.Goal(goals, false, 7.0f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-6.4f, 1.80f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.68f));

            // 反 Y 视窗：在上层的人往下看、在下层的人往上看
            LevelBuildKit.Preview(root.transform, true, new Vector2(1.8f, -2.8f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, 1.68f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L020.prefab");
            Config("L020", "深井", new Vector2(8.2f, 0f), new Vector2(-8.2f, 0f), false, false, true, true, -0.5f);
            Debug.Log("[GrayBox] 已重建 L020 深井");
        }

        [MenuItem("Tools/GrayBox/Rebuild/L021 悬廊")]
        public static void BuildL021()
        {
            var root = LevelBuildKit.NewRoot("L021", out var terrain, out var mechanisms, out var goals);

            // 画幅按 L002 的规模：Size 7 → 世界视野 24.9 × 14
            var camera = root.GetComponent<LevelCamera>();
            LevelBuildKit.SetAll(camera, ("size", 7f), ("boundsMargin", 0.2f));

            // ---- 左半屏：底层跨空 → 升降台 → 中层长廊 → 桥 → 蘑菇上高层 ----
            LevelBuildKit.Platform(terrain, "Plat_L0", -12.55f, -9.2f, -4.36f);
            LevelBuildKit.Platform(terrain, "Plat_L1", -5.4f, -0.2f, -4.36f);
            LevelBuildKit.Platform(terrain, "Plat_L2b", -5.0f, -2.36f, -1.80f);
            LevelBuildKit.Platform(terrain, "Plat_L2a", -12.55f, -8.6f, -1.80f);
            LevelBuildKit.Platform(terrain, "Plat_L3", -12.55f, -10.6f, 0.12f);

            // ---- 右半屏：底层 → 桥 → 未干块爬升 → 长廊 → 竖直游动平台 → 高层 → 蘑菇上顶层 ----
            LevelBuildKit.Platform(terrain, "Plat_R0", 0.2f, 4.0f, -3.08f);
            LevelBuildKit.Platform(terrain, "Plat_R1", 7.6f, 9.2f, -3.08f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.4f, 9.2f, -1.16f);
            LevelBuildKit.Platform(terrain, "Plat_R3", 2.2f, 4.6f, 1.40f);
            LevelBuildKit.Platform(terrain, "Plat_R4", 1.2f, 3.3f, 3.00f);

            // ---- 机关 ----
            // 两座桥交叉接线：左边的开关开右边的桥，右边的开关开左边的桥
            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -8.6f, -5.0f, -1.80f);
            var bridgeRight = Bridge(mechanisms, "Bridge_Right", 4.0f, 7.6f, -3.08f);

            var switchLeft = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Left", new Vector2(-3.6f, -3.76f));
            LevelBuildKit.SetArray(switchLeft.GetComponent<Switch>(), "targets", bridgeRight.GetComponent<ToggleBlock>());

            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(3.6f, -2.48f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var moverLeft = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Left", new Vector2(-8.88f, -4.68f));
            LevelBuildKit.SetAll(moverLeft.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(3.16f, 0f)),
                ("oneWaySeconds", 3.4f));

            var liftLeft = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-2.0f, -4.68f));
            LevelBuildKit.SetAll(liftLeft.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 3.20f),
                ("speed", 2.5f));

            var moverRight = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(5.0f, -1.48f));
            LevelBuildKit.SetAll(moverRight.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 3.20f)),
                ("oneWaySeconds", 3.0f));

            var lens = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-3.5f, -1.46f));
            LevelBuildKit.SetAll(lens.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-8.9f, -0.60f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-1f, 0f)),
                ("localEnd", new Vector2(1f, 0f)),
                ("influence", 1.25f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            // 未干块碰撞体 offset y=0.17、高 0.40 → 顶面 = 根节点 y + 0.37
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(9.93f, -2.81f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(11.13f, -2.17f));

            var springLeft = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-9.5f, -1.71f));
            LevelBuildKit.Set(springLeft.GetComponent<SpringBed>(), "bounceVelocity", 14f);

            var springRight = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(4.1f, 1.49f));
            LevelBuildKit.Set(springRight.GetComponent<SpringBed>(), "bounceVelocity", 14f);

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-9.8f, -4.11f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(8.2f, -0.91f));

            var checkpointLeft = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Left", new Vector2(-4.5f, -1.36f));
            LevelBuildKit.SetAll(checkpointLeft.GetComponent<Checkpoint>(), ("side", PlayerSide.Left), ("order", 0));
            var checkpointRight = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Right", new Vector2(5.9f, -0.72f));
            LevelBuildKit.SetAll(checkpointRight.GetComponent<Checkpoint>(), ("side", PlayerSide.Right), ("order", 0));

            // 照面灯：光斑摆在对面半屏需要被照亮的位置，孔的位置和大小都跟着光斑走
            Lamp(mechanisms, "Beacon_Lamp_Left_A", new Vector2(-10.9f, -1.35f), "Glow_Left_A", new Vector2(8.6f, -1.35f));
            Lamp(mechanisms, "Beacon_Lamp_Left_B", new Vector2(-2.8f, -1.35f), "Glow_Left_B", new Vector2(2.6f, -2.60f));
            Lamp(mechanisms, "Beacon_Lamp_Right_A", new Vector2(7.0f, -0.71f), "Glow_Right_A", new Vector2(-6.8f, -1.80f));
            Lamp(mechanisms, "Beacon_Lamp_Right_B", new Vector2(2.7f, 1.85f), "Glow_Right_B", new Vector2(-9.6f, -0.60f));

            LevelBuildKit.Goal(goals, true, -11.4f, 0.12f);
            LevelBuildKit.Goal(goals, false, 2.2f, 3.00f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-11.4f, -4.04f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(2.4f, -2.76f));

            // 视窗初位 = 出生点 + 12.48，反推偏移就是 ±12.48
            LevelBuildKit.Preview(root.transform, true, new Vector2(1.08f, -4.04f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-10.08f, -2.76f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L021.prefab");
            Config("L021", "悬廊", new Vector2(12.48f, 0f), new Vector2(-12.48f, 0f));
            Debug.Log("[GrayBox] 已重建 L021 悬廊（L002 规模）");
        }

        [MenuItem("Tools/GrayBox/Rebuild/L022 双塔")]
        public static void BuildL022()
        {
            var root = LevelBuildKit.NewRoot("L022", out var terrain, out var mechanisms, out var goals);

            // 左：压板就在出生台上（不用过桥就能踩），桥开在中段
            LevelBuildKit.Platform(terrain, "Plat_L1a", -7.8f, -5.0f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L1b", -2.6f, -1.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -4.6f, -2.4f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -6.0f, -4.4f, 0.84f);

            // 右：压板同样在出生台上，桥开在中段
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.0f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.6f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.4f, 6.2f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 2.6f, 1.48f);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -5.0f, -2.6f, -3.00f);
            var bridgeRight = Bridge(mechanisms, "Bridge_Right", 3.0f, 5.6f, -2.36f);

            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-6.2f, -2.75f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(2.0f, -2.11f));
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);

            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-2.0f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-4.0f, -0.99f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -2.68f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(3.0f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            LevelBuildKit.Goal(goals, true, -5.2f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.2f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -0.6f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.0f, -0.6f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L022.prefab");
            Config("L022", "双塔", new Vector2(8.2f, 2.08f), new Vector2(-8.0f, 1.44f));
            Debug.Log("[GrayBox] 已重建 L022 双塔");
        }

        [MenuItem("Tools/GrayBox/Rebuild/L023 折线")]
        public static void BuildL023()
        {
            var root = LevelBuildKit.NewRoot("L023", out var terrain, out var mechanisms, out var goals);

            // 左：从顶层折到中层再折到底层
            LevelBuildKit.Platform(terrain, "Plat_LT", -7.8f, -6.0f, 1.48f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -6.4f, -3.6f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_LB1", -7.8f, -5.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LB2", -1.6f, -0.2f, -3.00f);

            // 右：底层 → 未干块 → 中层 → 游动平台 → 顶层
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.0f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.8f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.4f, 5.4f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 2.2f, 0.84f);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -5.2f, -1.6f, -3.00f);
            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.2f, -1.76f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-5.0f, -0.44f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-1f, 0f)),
                ("localEnd", new Vector2(1f, 0f)),
                ("influence", 1.5f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            var lens = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-6.8f, -2.66f));
            LevelBuildKit.SetAll(lens.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(3.7f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(4.8f, -2.775f));

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -2.68f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(2.8f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            LevelBuildKit.Goal(goals, true, -0.9f, -3.00f);
            LevelBuildKit.Goal(goals, false, 1.0f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.0f, 1.80f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            // 反 X 视窗
            LevelBuildKit.Preview(root.transform, true, new Vector2(7.0f, -1.6f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-1.0f, -1.6f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L023.prefab");
            Config("L023", "折线", new Vector2(0f, -3.4f), new Vector2(0f, 0.44f), true, true);
            Debug.Log("[GrayBox] 已重建 L023 折线");
        }

        [MenuItem("Tools/GrayBox/Rebuild/L024 终章")]
        public static void BuildL024()
        {
            var root = LevelBuildKit.NewRoot("L024", out var terrain, out var mechanisms, out var goals);

            // 左：底层断口 → 升降台 → 中层压板 → 蘑菇 → 高层（点灯）
            LevelBuildKit.Platform(terrain, "Plat_L1a", -7.8f, -6.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L1b", -4.8f, -3.8f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -3.8f, -1.6f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -4.6f, -2.6f, 0.84f);

            // 右：底层 → 断口 → 未干块上中层 → 压板 → 蘑菇 → 高层
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.4f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 4.8f, 6.0f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.0f, 5.2f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 2.0f, 0.84f);

            var bridgeLeft = Bridge(mechanisms, "Bridge_Left", -6.6f, -4.8f, -3.00f);
            var bridgeRight = Bridge(mechanisms, "Bridge_Right", 3.4f, 4.8f, -2.36f);

            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-3.2f, -0.83f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(4.6f, -0.19f));
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);

            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-4.2f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var springLeft = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-2.0f, -0.99f));
            LevelBuildKit.Set(springLeft.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(6.2f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(7.2f, -1.495f));

            var springRight = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(3.4f, -0.35f));
            LevelBuildKit.Set(springRight.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var lens = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Right", new Vector2(2.0f, -2.02f));
            LevelBuildKit.SetAll(lens.GetComponent<LensStation>(),
                ("side", PlayerSide.Right),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var beacon = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Left", new Vector2(-3.2f, 1.29f));
            LevelBuildKit.SetAll(beacon.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(4.0f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            LevelBuildKit.Goal(goals, true, -3.6f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.0f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -1.2f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L024.prefab");
            Config("L024", "终章", new Vector2(8.2f, 1.48f), new Vector2(-8.2f, 0.64f));
            Debug.Log("[GrayBox] 已重建 L024 终章");
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

        static void Config(string levelId, string displayName, Vector2 leftOffset, Vector2 rightOffset,
            bool invLeftX = false, bool invRightX = false, bool invLeftY = false, bool invRightY = false, float axisY = 0f)
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
            so.FindProperty("leftWindowInverted").boolValue = invLeftX;
            so.FindProperty("rightWindowInverted").boolValue = invRightX;
            so.FindProperty("leftWindowInvertedY").boolValue = invLeftY;
            so.FindProperty("rightWindowInvertedY").boolValue = invRightY;
            so.FindProperty("windowMirrorAxisX").floatValue = 0f;
            so.FindProperty("windowMirrorAxisY").floatValue = axisY;
            so.FindProperty("tutorialTitle").stringValue = "";
            so.FindProperty("tutorialBody").stringValue = "";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }
}
