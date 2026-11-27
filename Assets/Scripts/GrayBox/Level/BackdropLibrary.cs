using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 斑驳背景的候选图集。每关加载时从中随机抽两张铺在左右半屏，
    /// 让不同关卡的纸面纹理不一样，而不是固定用同一对贴图。
    /// 运行时从 Resources/BackdropLibrary 读取，找不到就保持场景里原来的贴图。
    /// </summary>
    [CreateAssetMenu(menuName = "GrayBox/Backdrop Library", fileName = "BackdropLibrary")]
    public sealed class BackdropLibrary : ScriptableObject
    {
        [SerializeField] Sprite[] sprites;

        static BackdropLibrary cached;
        static bool looked;

        public int Count => sprites != null ? sprites.Length : 0;

        public static BackdropLibrary Load()
        {
            if (looked) return cached;

            looked = true;
            cached = Resources.Load<BackdropLibrary>("BackdropLibrary");
            return cached;
        }

        /// <summary>抽两张互不相同的贴图；图集不足两张时返回空数组。</summary>
        public Sprite[] PickTwo()
        {
            int count = Count;
            if (count < 2) return new Sprite[0];

            int first = Random.Range(0, count);
            int second = Random.Range(0, count - 1);
            if (second >= first) second++;

            return new[] { sprites[first], sprites[second] };
        }
    }
}
