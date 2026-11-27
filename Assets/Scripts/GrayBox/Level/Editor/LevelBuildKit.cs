using UnityEditor;
using UnityEngine;

namespace ThreeCup.EditorTools
{
    /// <summary>
    /// 关卡预制体的搭建原语：只负责摆件，不判断设计。
    /// 平台顶面坐标 = 传入的 topY，角色站立中心 = topY + 0.32。
    /// </summary>
    public static class LevelBuildKit
    {
        public const float Layer = 0.64f;
        public const string MechanismDir = "Assets/Prefabs/GrayBox/Mechanisms/";

        static Sprite terrainSprite;

        public static Sprite TerrainSprite
        {
            get
            {
                if (terrainSprite == null)
                {
                    var platform = AssetDatabase.LoadAssetAtPath<GameObject>(MechanismDir + "Platform.prefab");
                    terrainSprite = platform.GetComponent<SpriteRenderer>().sprite;
                }

                return terrainSprite;
            }
        }

        public static GameObject NewRoot(string name, out Transform terrain, out Transform mechanisms, out Transform goals)
        {
            var root = new GameObject(name);
            if (root.GetComponent<ThreeCup.GrayBox.LevelCamera>() == null)
                root.AddComponent<ThreeCup.GrayBox.LevelCamera>();
            terrain = Section(root.transform, "Terrain");
            mechanisms = Section(root.transform, "Mechanisms");
            goals = Section(root.transform, "Goals");
            return root;
        }

        static Transform Section(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        public static Transform Platform(Transform parent, string name, float x1, float x2, float topY)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            float width = Mathf.Abs(x2 - x1);
            go.transform.localPosition = new Vector3((x1 + x2) * 0.5f, topY - Layer * 0.5f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = TerrainSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(width, Layer);
            sr.sortingOrder = 0;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, Layer);
            return go.transform;
        }

        public static GameObject Part(Transform parent, string prefabName, string name, Vector2 pos)
        {
            var path = MechanismDir + prefabName + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new System.Exception("找不到预制体 " + path);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            return go;
        }

        public static void Set(Component target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) throw new System.Exception($"{target.GetType().Name}.{field} 不存在");

            switch (value)
            {
                case float f: p.floatValue = f; break;
                case int i: p.intValue = i; break;
                case bool b: p.boolValue = b; break;
                case Vector2 v: p.vector2Value = v; break;
                case string s: p.stringValue = s; break;
                case System.Enum e: p.enumValueIndex = System.Convert.ToInt32(e); break;
                case Object o: p.objectReferenceValue = o; break;
                default: throw new System.Exception("不支持的类型 " + value.GetType());
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetAll(Component target, params (string field, object value)[] values)
        {
            foreach (var (field, value) in values) Set(target, field, value);
        }

        /// <summary>没有预制体的纯组件对象（双压板闸这类逻辑载体）。</summary>
        public static GameObject Bare(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static void SetArray(Component target, string field, params Object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) throw new System.Exception($"{target.GetType().Name}.{field} 不存在");

            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static GameObject Goal(Transform parent, bool left, float x, float topY)
        {
            return Part(parent, left ? "Goal_Left" : "Goal_Right", left ? "Goal_Left" : "Goal_Right", new Vector2(x, topY + 0.9f));
        }

        public static Transform Spawn(Transform parent, bool left, Vector2 pos)
        {
            var go = new GameObject(left ? "Spawn_Left" : "Spawn_Right");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go.transform;
        }

        public static GameObject Preview(Transform parent, bool left, Vector2 center, Vector2 size)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GrayBox/WindowPreview.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = left ? "WindowPreview_Left" : "WindowPreview_Right";
            go.transform.localPosition = new Vector3(center.x, center.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            return go;
        }

        public static void Save(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
        }
    }
}
