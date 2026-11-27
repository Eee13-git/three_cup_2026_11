using UnityEditor;
using UnityEngine;
using ThreeCup.GrayBox;

namespace ThreeCup.EditorTools
{
    /// <summary>
    /// 正式关搭建入口。每关一个菜单项，坐标写死在方法里，改设计就改这里重建。
    /// </summary>
    public static class LevelBuilders
    {
        const string PrefabDir = "Assets/Prefabs/GrayBox/";
        const string ConfigDir = "Assets/GameData/Levels/";
        const string CatalogPath = "Assets/GameData/LevelCatalog.asset";

        [MenuItem("Tools/GrayBox/Build/L010 灯")]
        public static void BuildL010()
        {
            var root = LevelBuildKit.NewRoot("L010", out var terrain, out var mechanisms, out var goals);

            // 左半屏：低(-2.36) → 中(-1.08) → 高(0.84)，爬升靠两段游动平台
            LevelBuildKit.Platform(terrain, "Plat_L1a", -2.6f, -0.6f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_L1b", -5.6f, -4.2f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -6.8f, -5.8f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_L3", -7.8f, -5.6f, 0.84f);

            // 右半屏：低(-3.00) → 中(-0.44) → 高(1.48)，蘑菇起跳 + 一段游动平台
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.6f, 3.4f, -3.00f);
            // 落台要能兜住蘑菇满速横移（bounce 13 时最远 3.2），否则右键一按到底就是必死跳
            LevelBuildKit.Platform(terrain, "Plat_R2", 3.8f, 6.4f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_R3", 5.8f, 7.8f, 1.48f);

            // 左低层的断口由右侧开关开出的补笔带补上：A 必须先等 B 踩开关
            var bridge = LevelBuildKit.Part(mechanisms, "ToggleBlock", "Bridge_Left", new Vector2(-3.4f, -2.715f));
            bridge.transform.localScale = new Vector3(1.6f / 3.64f, 1f, 1f);
            LevelBuildKit.SetAll(bridge.GetComponent<ToggleBlock>(),
                ("startActive", false),
                ("growFromLeft", true));

            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.6f, -2.40f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridge.GetComponent<ToggleBlock>());

            // 左半屏两段竖直游动平台：底面对齐地面顶面（和 L007 一样要跳 0.64 上去），
            // 行程 1.28 后停在地面层上方 0.64 处，走下来时不会和地形顶面平齐卡住。
            var moverL1 = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_L1", new Vector2(-4.4f, -2.04f));
            LevelBuildKit.SetAll(moverL1.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.28f)),
                ("oneWaySeconds", 2.0f));

            var moverL2 = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_L2", new Vector2(-5.2f, -0.76f));
            LevelBuildKit.SetAll(moverL2.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.2f));

            var moverR = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_R", new Vector2(5.4f, -0.12f));
            LevelBuildKit.SetAll(moverR.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.2f));

            // 右低层用蘑菇把 B 送到中层；弹速压到 13，落点才收在加宽后的落台内
            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(3.1f, -2.91f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            // 两盏灯互相点亮对方的高层路线
            var beaconLeft = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Left", new Vector2(-5.75f, 1.29f));
            LevelBuildKit.SetAll(beaconLeft.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(4.4f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            var beaconRight = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Right", new Vector2(4.4f, 0.01f));
            LevelBuildKit.SetAll(beaconRight.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(5.2f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-2.2f, -2.12f));

            LevelBuildKit.Goal(goals, true, -6.7f, 0.84f);
            LevelBuildKit.Goal(goals, false, 7.1f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-1.0f, -2.04f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.4f, -2.68f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(7.2f, -1.2f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-6.8f, -1.6f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L010.prefab");
            ApplyConfig("L010", "灯", new Vector2(8.2f, 0.84f), new Vector2(-8.2f, 1.08f));
            Debug.Log("[GrayBox] 已重建 L010");
        }

        [MenuItem("Tools/GrayBox/Build/L011 错层")]
        public static void BuildL011()
        {
            var root = LevelBuildKit.NewRoot("L011", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84；出生在中层，先掉到最底层再爬回去
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -5.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -2.6f, -0.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -7.8f, -5.6f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LM2", -3.4f, -2.2f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -5.4f, -2.2f, 0.84f);

            // 右：低 -3.00 / 中 -1.08 / 高 0.20
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 2.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.6f, 7.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.4f, 7.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_RH", 2.4f, 3.6f, 0.20f);

            // 两道跨屏断口：各踩对方的颜料罐才长出来
            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -5.2f, -2.6f, -3.00f);
            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 2.6f, 5.6f, -3.00f);

            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.1f, -2.40f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var switchLeft = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Left", new Vector2(-2.7f, -0.48f));
            LevelBuildKit.SetArray(switchLeft.GetComponent<Switch>(), "targets", bridgeRight.GetComponent<ToggleBlock>());

            // 左：蘑菇把 A 从底层顶到中层，升降台再送到高层
            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-0.9f, -2.91f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-1.7f, -1.40f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            // 右：升降台送到中层，游动平台送到高层
            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.6f, -3.32f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            // 游动平台的起始面要和出发平台齐平（直接走上去），升到比目标面高 0.64 再走下来
            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(4.0f, -1.40f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-3.4f, 1.08f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(6.2f, -0.84f));

            LevelBuildKit.Goal(goals, true, -4.6f, 0.84f);
            LevelBuildKit.Goal(goals, false, 3.0f, 0.20f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-6.9f, -0.76f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.2f, -2.68f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.3f, -1.9f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.0f, -1.0f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L011.prefab");
            ApplyConfig("L011", "错层", new Vector2(8.2f, -1.14f), new Vector2(-8.2f, 1.68f));
            Debug.Log("[GrayBox] 已重建 L011");
        }

        [MenuItem("Tools/GrayBox/Build/L012 镜背")]
        public static void BuildL012()
        {
            var root = LevelBuildKit.NewRoot("L012", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -1.2f, -0.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -7.8f, -5.6f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LM2", -2.2f, -1.2f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -6.2f, -3.0f, 0.84f);

            // 右：低 -2.36 / 中 -0.44 / 高 1.48
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 2.6f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.2f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 5.0f, 7.0f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RH", 0.2f, 3.8f, 1.48f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -4.2f, -1.2f, -3.00f);
            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 2.6f, 5.2f, -2.36f);

            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.1f, -1.76f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var switchLeft = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Left", new Vector2(-1.7f, -0.48f));
            LevelBuildKit.SetArray(switchLeft.GetComponent<Switch>(), "targets", bridgeRight.GetComponent<ToggleBlock>());

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.8f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Left", new Vector2(-2.6f, -1.40f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(7.3f, -2.27f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var moverRight = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(4.5f, -0.76f));
            LevelBuildKit.SetAll(moverRight.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            // 尖刺要放在两侧都有落脚空地的位置，不能贴着落台边缘或弹簧
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-4.2f, 1.08f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(2.6f, 1.72f));

            LevelBuildKit.Goal(goals, true, -5.4f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.0f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-6.9f, -0.76f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.2f, -2.04f));

            // 两侧反 X：窗口显示的是对面半屏的远端
            LevelBuildKit.Preview(root.transform, true, new Vector2(6.9f, -1.9f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-1.2f, -1.2f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L012.prefab");
            ApplyConfig("L012", "镜背", new Vector2(0f, -1.14f), new Vector2(0f, 0.84f), true, true);
            Debug.Log("[GrayBox] 已重建 L012");
        }

        [MenuItem("Tools/GrayBox/Build/L013 俯视")]
        public static void BuildL013()
        {
            var root = LevelBuildKit.NewRoot("L013", out var terrain, out var mechanisms, out var goals);

            // 左：高 1.48 / 中 -1.08 / 低 -3.00，A 从最高处出发往下走
            LevelBuildKit.Platform(terrain, "Plat_LT", -7.8f, -5.4f, 1.48f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -6.2f, -3.4f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LB1", -7.8f, -5.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LB2", -2.4f, -0.2f, -3.00f);

            // 右：低 -3.00 / 中 -0.44 / 高 1.48，B 从底层往上爬
            LevelBuildKit.Platform(terrain, "Plat_RB", 0.6f, 3.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.6f, 5.8f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 5.8f, 7.8f, 1.48f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -5.4f, -2.4f, -3.00f);

            // 开关要离 B 的出生点留出间距，否则一进关就被触发、桥直接是开的
            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.0f, -2.40f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(3.0f, -2.91f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13.5f);

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(5.4f, -0.12f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.2f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-4.6f, -0.84f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(4.6f, -0.20f));

            LevelBuildKit.Goal(goals, true, -0.9f, -3.00f);
            LevelBuildKit.Goal(goals, false, 7.0f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-6.6f, 1.80f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(0.9f, -2.68f));

            // 两侧反 Y（轴 -0.5）：上层角色的窗口朝下、下层角色朝上
            LevelBuildKit.Preview(root.transform, true, new Vector2(1.6f, -2.8f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.3f, 1.68f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L013.prefab");
            ApplyConfig("L013", "俯视", new Vector2(8.2f, 0f), new Vector2(-8.2f, 0f), false, false, true, true, -0.5f);
            Debug.Log("[GrayBox] 已重建 L013");
        }

        [MenuItem("Tools/GrayBox/Build/L014 双面")]
        public static void BuildL014()
        {
            var root = LevelBuildKit.NewRoot("L014", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84，三段竖直拉开，终点在靠近中缝的高处
            LevelBuildKit.Platform(terrain, "Plat_LB", -7.8f, -2.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -5.9f, -4.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -3.0f, -0.2f, 0.84f);

            // 右：低 -3.00 / 中 -0.44 / 高 1.48；中层缩短，冲刺跳上高层
            LevelBuildKit.Platform(terrain, "Plat_RB", 0.8f, 4.0f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 2.4f, 4.4f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 5.8f, 7.8f, 1.48f);

            var springLeft = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-6.4f, -2.91f));
            LevelBuildKit.Set(springLeft.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-3.6f, -1.40f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var springRight = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(1.9f, -2.91f));
            LevelBuildKit.Set(springRight.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(5.2f, -0.12f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.2f));

            // 灯装在自己高层，点亮对面半屏的中层落点
            var beacon = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Left", new Vector2(-2.6f, 1.29f));
            LevelBuildKit.SetAll(beacon.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(4.0f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-1.8f, 1.08f));

            LevelBuildKit.Goal(goals, true, -0.6f, 0.84f);
            LevelBuildKit.Goal(goals, false, 7.2f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.5f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(0.9f, -2.68f));

            // A 反 X（看对面远端的高层），B 正向平移（看 A 的中层）
            LevelBuildKit.Preview(root.transform, true, new Vector2(7.5f, 1.1f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-6.5f, -0.7f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L014.prefab");
            ApplyConfig("L014", "双面", new Vector2(0f, 3.78f), new Vector2(-7.4f, 1.98f), true, false);
            Debug.Log("[GrayBox] 已重建 L014");
        }

        [MenuItem("Tools/GrayBox/Build/L015 泵")]
        public static void BuildL015()
        {
            var root = LevelBuildKit.NewRoot("L015", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -3.8f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -1.4f, -0.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -3.8f, -2.2f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -5.6f, -3.6f, 0.84f);
            // 压板石墩：抬高一格，必须先跳上去才能踩；中层台从 −3.8 起，
            // 给石墩右侧留出下脚位（站在墩上头顶正好 −1.72，不能被台底挡住）
            LevelBuildKit.Platform(terrain, "Plat_LP", -5.4f, -4.6f, -2.36f);

            // 右：低 -3.00 / 中 -1.08 / 高 0.20
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 6.0f, 7.8f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.6f, 6.6f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_RH", 0.2f, 3.4f, 0.20f);
            LevelBuildKit.Platform(terrain, "Plat_RP", 2.6f, 3.3f, -2.36f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -3.8f, -1.4f, -3.00f);
            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 3.4f, 6.0f, -3.00f);

            // 两块压板必须同时被踩，闸才打开两道断口；压板立在石墩上，前面还有尖刺
            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-5.0f, -2.11f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(2.95f, -2.11f));
            // 压板整体缩到 0.72：视觉和触发区一起变小，必须站到板中央
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);
            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.8f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-2.9f, -0.99f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -3.32f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(4.0f, -1.40f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            // 石墩前各一根尖刺：必须跳过尖刺再跳上石墩才踩得到压板
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-6.0f, -2.76f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(2.0f, -2.76f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_High", new Vector2(2.9f, 0.44f));

            LevelBuildKit.Goal(goals, true, -4.6f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.6f, 0.20f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.68f));

            // 两人都要能看见对方脚下那块压板，才能对上时机
            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -2.4f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -2.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L015.prefab");
            ApplyConfig("L015", "泵", new Vector2(8.2f, 0.28f), new Vector2(-8.2f, 0.28f));
            Debug.Log("[GrayBox] 已重建 L015");
        }

        [MenuItem("Tools/GrayBox/Build/L016 回廊")]
        public static void BuildL016()
        {
            var root = LevelBuildKit.NewRoot("L016", out var terrain, out var mechanisms, out var goals);

            // 左：中 -1.08 / 高 0.84，底层整个是空井
            LevelBuildKit.Platform(terrain, "Plat_LM1", -7.8f, -5.8f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LM2", -1.6f, -0.2f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -4.6f, -1.0f, 0.84f);

            // 右：低 -2.36 / 中 -0.44 / 高 1.48
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 2.2f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.0f, 6.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 2.6f, 4.8f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 1.6f, 1.48f);

            // 左：长行程横向游动平台，横跨空井
            var moverLeft = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Left", new Vector2(-5.4f, -1.40f));
            LevelBuildKit.SetAll(moverLeft.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(3.4f, 0f)),
                ("oneWaySeconds", 3.0f));

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.6f, -1.40f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            // 折痕：骑着平台经过时才把视窗翻到下层
            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-3.6f, -1.08f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-2f, 0f)),
                ("localEnd", new Vector2(2f, 0f)),
                ("influence", 1.5f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            // 右：两块未干块过缺口，蘑菇上中层，游动平台上高层
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(2.8f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(3.9f, -2.775f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(6.2f, -2.27f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var moverRight = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(2.2f, -0.76f));
            LevelBuildKit.SetAll(moverRight.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-2.0f, 1.08f));

            LevelBuildKit.Goal(goals, true, -3.0f, 0.84f);
            LevelBuildKit.Goal(goals, false, 0.8f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-6.8f, -0.76f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.4f, -1.6f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -0.2f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L016.prefab");
            ApplyConfig("L016", "回廊", new Vector2(8.2f, -0.84f), new Vector2(-8.2f, 1.84f));
            Debug.Log("[GrayBox] 已重建 L016");
        }

        [MenuItem("Tools/GrayBox/Build/L017 井")]
        public static void BuildL017()
        {
            var root = LevelBuildKit.NewRoot("L017", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84，靠两级未干块爬上中层
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -2.2f, -0.4f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -4.4f, -1.0f, 0.84f);

            // 右：低 -3.00 / 中 -0.44 / 高 1.48
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 5.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 2.4f, 4.0f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 1.4f, 1.48f);

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L1", new Vector2(-3.9f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L2", new Vector2(-2.7f, -2.135f));

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.6f, -1.40f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(4.6f, -2.91f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(1.8f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            var cpLeft = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Left", new Vector2(-1.4f, -0.76f));
            LevelBuildKit.SetAll(cpLeft.GetComponent<Checkpoint>(), ("side", PlayerSide.Left), ("order", 1));

            var cpRight = LevelBuildKit.Part(mechanisms, "Checkpoint", "Checkpoint_Right", new Vector2(2.0f, -2.68f));
            LevelBuildKit.SetAll(cpRight.GetComponent<Checkpoint>(), ("side", PlayerSide.Right), ("order", 1));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-5.4f, -2.76f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(3.6f, -2.76f));

            LevelBuildKit.Goal(goals, true, -3.0f, 0.84f);
            LevelBuildKit.Goal(goals, false, 0.8f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.68f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -1.2f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L017.prefab");
            ApplyConfig("L017", "井", new Vector2(8.2f, 1.48f), new Vector2(-8.2f, 2.28f));
            Debug.Log("[GrayBox] 已重建 L017");
        }

        [MenuItem("Tools/GrayBox/Build/L018 露珠")]
        public static void BuildL018()
        {
            var root = LevelBuildKit.NewRoot("L018", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -3.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -2.8f, -0.6f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -4.0f, -1.4f, 0.84f);

            // 右：低 -2.36 / 中 -0.44 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 2.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.6f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.4f, 6.6f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RH", 0.2f, 3.4f, 0.84f);

            // 露珠透镜：必须显式指定 side，预制体默认是 Left；偏移倍率保持 1，
            // 否则缩小时窗口中心会被拉回自己半屏（实现在 VisionWindow.SetZoom）
            var lensLeft = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-6.1f, -2.66f));
            LevelBuildKit.SetAll(lensLeft.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var lensRight = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Right", new Vector2(2.2f, -2.02f));
            LevelBuildKit.SetAll(lensRight.GetComponent<LensStation>(),
                ("side", PlayerSide.Right),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L1", new Vector2(-4.4f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L2", new Vector2(-3.4f, -2.135f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(3.3f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(4.4f, -2.775f));

            var liftLeft = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.9f, -1.40f));
            LevelBuildKit.SetAll(liftLeft.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -2.68f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(3.8f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-5.2f, -2.76f));

            LevelBuildKit.Goal(goals, true, -2.6f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.2f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.0f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -1.4f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.6f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L018.prefab");
            ApplyConfig("L018", "露珠", new Vector2(8.2f, 1.28f), new Vector2(-8.2f, 0.44f));
            Debug.Log("[GrayBox] 已重建 L018");
        }

        [MenuItem("Tools/GrayBox/Build/L019 灯下")]
        public static void BuildL019()
        {
            var root = LevelBuildKit.NewRoot("L019", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -1.6f, -0.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -4.2f, -2.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -5.4f, -3.6f, 0.84f);

            // 右：低 -2.36 / 中 -0.44 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.2f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.4f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.4f, 6.6f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 3.4f, 0.84f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -4.4f, -1.6f, -3.00f);
            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 3.2f, 5.4f, -2.36f);

            // 压板触发区收窄，必须站到板中央；位置也往路线深处挪，不能一出脚就踩到
            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-5.0f, -2.75f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(2.7f, -2.11f));
            // 压板整体缩到 0.72：视觉和触发区一起变小，必须站到板中央
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);
            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.7f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-2.9f, -0.99f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -2.68f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(3.8f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            // 灯装在自己高层，点亮对面中层；对面光斑用独立预制体手动摆位并绑定
            var beacon = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Left", new Vector2(-3.9f, 1.29f));
            LevelBuildKit.SetAll(beacon.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(4.0f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            var remoteGlow = LevelBuildKit.Part(mechanisms, "Beacon_Glow", "Beacon_Glow_Left", new Vector2(3.9f, 1.29f));
            remoteGlow.transform.localScale = new Vector3(4.0f / 3.34f, 3.0f / 3.44f, 1f);
            beacon.GetComponent<BeaconSwitch>().SetRemoteGlow(remoteGlow);

            LevelBuildKit.Goal(goals, true, -4.9f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.0f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.0f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.2f, -1.2f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L019.prefab");
            ApplyConfig("L019", "灯下", new Vector2(8.2f, 1.48f), new Vector2(-8.2f, 0.64f));
            Debug.Log("[GrayBox] 已重建 L019");
        }

        [MenuItem("Tools/GrayBox/Build/L020 错拍")]
        public static void BuildL020()
        {
            var root = LevelBuildKit.NewRoot("L020", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -2.4f, -0.4f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -4.4f, -1.2f, 0.84f);

            // 右：低 -2.36 / 中 -0.44 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 2.6f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.4f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.0f, 5.0f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 1.4f, 0.84f);

            // 左：两块未干块抢时间，走过折痕时视窗翻到下层
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L1", new Vector2(-4.0f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L2", new Vector2(-2.9f, -2.135f));

            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-1.4f, -1.08f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-1f, 0f)),
                ("localEnd", new Vector2(1f, 0f)),
                ("influence", 1.5f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.7f, -1.40f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 2.6f, 5.4f, -2.36f);
            var switchLeft = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Left", new Vector2(-1.9f, -0.48f));
            LevelBuildKit.SetArray(switchLeft.GetComponent<Switch>(), "targets", bridgeRight.GetComponent<ToggleBlock>());

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(6.4f, -2.27f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(2.2f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(1.9f, -2.12f));

            LevelBuildKit.Goal(goals, true, -3.0f, 0.84f);
            LevelBuildKit.Goal(goals, false, 0.8f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, 0.6f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L020.prefab");
            ApplyConfig("L020", "错拍", new Vector2(8.2f, 3.28f), new Vector2(-8.2f, 0.64f));
            Debug.Log("[GrayBox] 已重建 L020");
        }

        [MenuItem("Tools/GrayBox/Build/L021 纵深")]
        public static void BuildL021()
        {
            var root = LevelBuildKit.NewRoot("L021", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -1.8f, -0.2f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -3.4f, -1.2f, 0.84f);

            // 右：低 -2.36 / 中 -0.44 / 高 1.48
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 2.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.2f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 3.0f, 5.2f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 1.4f, 1.48f);

            var lensLeft = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-6.4f, -2.66f));
            LevelBuildKit.SetAll(lensLeft.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var lensRight = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Right", new Vector2(2.2f, -2.02f));
            LevelBuildKit.SetAll(lensRight.GetComponent<LensStation>(),
                ("side", PlayerSide.Right),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L1", new Vector2(-3.6f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_L2", new Vector2(-2.6f, -2.135f));

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.7f, -1.40f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 2.8f, 5.2f, -2.36f);
            var switchLeft = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Left", new Vector2(-1.5f, -0.48f));
            LevelBuildKit.SetArray(switchLeft.GetComponent<Switch>(), "targets", bridgeRight.GetComponent<ToggleBlock>());

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Right", new Vector2(6.6f, -2.27f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(2.2f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-5.4f, -2.76f));

            LevelBuildKit.Goal(goals, true, -2.4f, 0.84f);
            LevelBuildKit.Goal(goals, false, 0.8f, 1.48f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.2f, -1.0f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.2f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L021.prefab");
            ApplyConfig("L021", "纵深", new Vector2(8.2f, 1.68f), new Vector2(-8.2f, 0.84f));
            Debug.Log("[GrayBox] 已重建 L021");
        }

        [MenuItem("Tools/GrayBox/Build/L022 互换")]
        public static void BuildL022()
        {
            var root = LevelBuildKit.NewRoot("L022", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -1.6f, -0.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -4.2f, -2.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -5.4f, -3.6f, 0.84f);
            LevelBuildKit.Platform(terrain, "Plat_LP", -5.6f, -4.8f, -2.36f);

            // 右：低 -2.36 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.4f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 6.0f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.6f, 6.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 3.4f, 0.84f);
            LevelBuildKit.Platform(terrain, "Plat_RP", 2.5f, 3.2f, -1.72f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -4.6f, -1.6f, -3.00f);
            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 3.4f, 6.0f, -2.36f);

            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-5.2f, -2.11f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(2.85f, -1.47f));
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);

            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.7f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-2.9f, -0.99f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -2.68f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 1.92f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(4.0f, -1.40f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            var beacon = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Left", new Vector2(-3.9f, 1.29f));
            LevelBuildKit.SetAll(beacon.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(4.0f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Left", new Vector2(-6.2f, -2.76f));
            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(1.8f, -2.76f));

            LevelBuildKit.Goal(goals, true, -4.6f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.0f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(0.8f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -1.2f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.0f, -1.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L022.prefab");
            ApplyConfig("L022", "互换", new Vector2(8.2f, 1.48f), new Vector2(-7.8f, 0.64f));
            Debug.Log("[GrayBox] 已重建 L022");
        }

        [MenuItem("Tools/GrayBox/Build/L023 闭环")]
        public static void BuildL023()
        {
            var root = LevelBuildKit.NewRoot("L023", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.4f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -1.6f, -0.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -4.2f, -2.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -6.6f, -5.0f, 0.84f);

            // 右：低 -2.36 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.2f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 5.8f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.4f, 6.2f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 0.2f, 3.4f, 0.84f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -4.4f, -1.6f, -3.00f);

            var switchRight = LevelBuildKit.Part(mechanisms, "Switch", "Switch_Right", new Vector2(2.6f, -1.76f));
            LevelBuildKit.SetArray(switchRight.GetComponent<Switch>(), "targets", bridgeLeft.GetComponent<ToggleBlock>());

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.7f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-3.1f, -1.08f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-1f, 0f)),
                ("localEnd", new Vector2(1f, 0f)),
                ("influence", 1.5f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            var moverLeft = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Left", new Vector2(-4.6f, -1.40f));
            LevelBuildKit.SetAll(moverLeft.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R1", new Vector2(3.6f, -2.775f));
            LevelBuildKit.Part(mechanisms, "CrumblePlatform", "Crumble_R2", new Vector2(4.7f, -2.775f));

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -2.68f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 1.92f),
                ("speed", 2.5f));

            var moverRight = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(3.8f, -1.40f));
            LevelBuildKit.SetAll(moverRight.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 2.56f)),
                ("oneWaySeconds", 2.6f));

            LevelBuildKit.Part(mechanisms, "Spike", "Spike_Right", new Vector2(2.2f, -2.12f));

            LevelBuildKit.Goal(goals, true, -5.8f, 0.84f);
            LevelBuildKit.Goal(goals, false, 1.0f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.2f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(1.0f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(1.0f, -1.4f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.2f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L023.prefab");
            ApplyConfig("L023", "闭环", new Vector2(8.2f, 1.28f), new Vector2(-8.2f, 1.48f));
            Debug.Log("[GrayBox] 已重建 L023");
        }

        [MenuItem("Tools/GrayBox/Build/L024 终章")]
        public static void BuildL024()
        {
            var root = LevelBuildKit.NewRoot("L024", out var terrain, out var mechanisms, out var goals);

            // 左：低 -3.00 / 中 -1.08 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_L1", -7.8f, -4.6f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_L2", -1.8f, -0.2f, -3.00f);
            LevelBuildKit.Platform(terrain, "Plat_LM", -4.2f, -2.0f, -1.08f);
            LevelBuildKit.Platform(terrain, "Plat_LH", -5.6f, -3.8f, 0.84f);
            LevelBuildKit.Platform(terrain, "Plat_LP", -5.8f, -5.0f, -2.36f);

            // 右：低 -2.36 / 中 -0.44 / 高 0.84
            LevelBuildKit.Platform(terrain, "Plat_R1", 0.2f, 3.6f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_R2", 6.0f, 7.8f, -2.36f);
            LevelBuildKit.Platform(terrain, "Plat_RM", 4.0f, 6.0f, -0.44f);
            LevelBuildKit.Platform(terrain, "Plat_RT", 1.2f, 2.8f, 0.84f);
            LevelBuildKit.Platform(terrain, "Plat_RP", 2.7f, 3.4f, -1.72f);

            var bridgeLeft = BuildBridge(mechanisms, "Bridge_Left", -4.6f, -1.8f, -3.00f);
            var bridgeRight = BuildBridge(mechanisms, "Bridge_Right", 3.7f, 6.0f, -2.36f);

            var plateLeft = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Left", new Vector2(-5.4f, -2.11f));
            var plateRight = LevelBuildKit.Part(mechanisms, "TandemPlate", "Plate_Right", new Vector2(3.05f, -1.47f));
            plateLeft.transform.localScale = new Vector3(0.72f, 1f, 1f);
            plateRight.transform.localScale = new Vector3(0.72f, 1f, 1f);

            var gate = LevelBuildKit.Bare(mechanisms, "TandemGate");
            var tandem = gate.AddComponent<TandemGate>();
            LevelBuildKit.Set(tandem, "plateA", plateLeft.GetComponent<TandemPlate>());
            LevelBuildKit.Set(tandem, "plateB", plateRight.GetComponent<TandemPlate>());
            LevelBuildKit.SetArray(tandem, "targets", bridgeLeft.GetComponent<ToggleBlock>(), bridgeRight.GetComponent<ToggleBlock>());
            LevelBuildKit.Set(tandem, "oneShot", true);

            var lensLeft = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Left", new Vector2(-6.6f, -2.66f));
            LevelBuildKit.SetAll(lensLeft.GetComponent<LensStation>(),
                ("side", PlayerSide.Left),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var lensRight = LevelBuildKit.Part(mechanisms, "Lens_Dew", "Lens_Right", new Vector2(2.0f, -2.02f));
            LevelBuildKit.SetAll(lensRight.GetComponent<LensStation>(),
                ("side", PlayerSide.Right),
                ("zoomInScale", new Vector2(1.35f, 1f)),
                ("zoomOutScale", new Vector2(0.7f, 1f)));

            var lift = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Left", new Vector2(-0.8f, -3.32f));
            LevelBuildKit.SetAll(lift.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 2.56f),
                ("speed", 2.5f));

            var crease = LevelBuildKit.Part(mechanisms, "CreaseLine", "Crease_Left", new Vector2(-3.1f, -1.08f));
            LevelBuildKit.SetAll(crease.GetComponent<CreaseLine>(),
                ("localStart", new Vector2(-1f, 0f)),
                ("localEnd", new Vector2(1f, 0f)),
                ("influence", 1.5f),
                ("ownerFilter", CreaseOwnerFilter.Left));
            crease.transform.localScale = new Vector3(0.5f, 1f, 1f);

            var spring = LevelBuildKit.Part(mechanisms, "Spring_Mushroom", "Spring_Left", new Vector2(-2.8f, -0.99f));
            LevelBuildKit.Set(spring.GetComponent<SpringBed>(), "bounceVelocity", 13f);

            var liftRight = LevelBuildKit.Part(mechanisms, "Lift", "Lift_Right", new Vector2(7.0f, -3.32f));
            LevelBuildKit.SetAll(liftRight.GetComponent<Lift>(),
                ("size", new Vector2(0.64f, 0.64f)),
                ("riseDistance", 3.20f),
                ("speed", 2.5f));

            var mover = LevelBuildKit.Part(mechanisms, "MovingPlatform", "Mover_Right", new Vector2(3.4f, -0.76f));
            LevelBuildKit.SetAll(mover.GetComponent<MovingPlatform>(),
                ("travel", new Vector2(0f, 1.92f)),
                ("oneWaySeconds", 2.4f));

            var beacon = LevelBuildKit.Part(mechanisms, "Beacon_Lamp", "Beacon_Left", new Vector2(-4.2f, 1.29f));
            LevelBuildKit.SetAll(beacon.GetComponent<BeaconSwitch>(),
                ("targetSize", new Vector2(4.0f, 3.0f)),
                ("targetOffset", Vector2.zero),
                ("mirrorAcrossCenter", true));

            LevelBuildKit.Goal(goals, true, -4.8f, 0.84f);
            LevelBuildKit.Goal(goals, false, 2.0f, 0.84f);

            LevelBuildKit.Spawn(root.transform, true, new Vector2(-7.4f, -2.68f));
            LevelBuildKit.Spawn(root.transform, false, new Vector2(0.8f, -2.04f));

            LevelBuildKit.Preview(root.transform, true, new Vector2(0.8f, -1.2f), new Vector2(4f, 3f));
            LevelBuildKit.Preview(root.transform, false, new Vector2(-7.2f, -1.4f), new Vector2(4f, 3f));

            LevelBuildKit.Save(root, PrefabDir + "L024.prefab");
            ApplyConfig("L024", "终章", new Vector2(8.2f, 1.48f), new Vector2(-8.0f, 0.64f));
            Debug.Log("[GrayBox] 已重建 L024");
        }

        static GameObject BuildBridge(Transform parent, string name, float x1, float x2, float topY)
        {
            var bridge = LevelBuildKit.Part(parent, "ToggleBlock", name, new Vector2((x1 + x2) * 0.5f, topY - 0.355f));
            bridge.transform.localScale = new Vector3(Mathf.Abs(x2 - x1) / 3.64f, 1f, 1f);
            LevelBuildKit.SetAll(bridge.GetComponent<ToggleBlock>(),
                ("startActive", false),
                ("growFromLeft", true));
            return bridge;
        }

        static void ApplyConfig(string levelId, string displayName, Vector2 leftOffset, Vector2 rightOffset,
            bool invLeftX = false, bool invRightX = false, bool invLeftY = false, bool invRightY = false, float axisY = 0f)
        {
            var path = ConfigDir + levelId + ".asset";
            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LevelConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            var so = new SerializedObject(config);
            so.FindProperty("levelId").stringValue = levelId;
            so.FindProperty("enabled").boolValue = true;
            so.FindProperty("levelPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + levelId + ".prefab");
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
            EnsureCatalogEntry(config);
            AssetDatabase.SaveAssets();
        }

        static void EnsureCatalogEntry(LevelConfig config)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null) throw new System.Exception("找不到 " + CatalogPath);

            var so = new SerializedObject(catalog);
            var levels = so.FindProperty("levels");
            for (int i = 0; i < levels.arraySize; i++)
            {
                if (levels.GetArrayElementAtIndex(i).objectReferenceValue == config) return;
            }

            levels.InsertArrayElementAtIndex(levels.arraySize);
            levels.GetArrayElementAtIndex(levels.arraySize - 1).objectReferenceValue = config;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }
    }
}
