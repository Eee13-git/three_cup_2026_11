using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 通关后的整场擦除：遮蔽层消散之后，用同一套扩散逻辑把画面上
    /// 除背景以外的所有东西一起擦掉，只留下一张干净的画布。
    ///
    /// 关卡实例每关都会被销毁重建，但中央墙、背景这些常驻对象不会，
    /// 所以擦完之后要把它们记下来，下一关加载时由 ResetVisuals 还原。
    /// </summary>
    public sealed class SceneDissolve : MonoBehaviour
    {
        [Tooltip("使用 GrayBox/StainDissolve 的材质")]
        [SerializeField] Material dissolveMaterial;

        [Tooltip("通关擦除的时长")]
        [SerializeField] float duration = 1.6f;

        [Tooltip("进关绘制前，空白画布静止展示的时长")]
        [SerializeField] float revealHold = 1f;

        [Tooltip("进关绘制的时长（逆特效）")]
        [SerializeField] float revealDuration = 1.4f;

        [Tooltip("边缘羽化，越大过渡越柔")]
        [SerializeField, Range(0.01f, 2f)] float softness = 0.6f;

        [Tooltip("低于这个排序的精灵不参与（空白画布是 -20，斑驳背景是 -10）")]
        [SerializeField] int minSortingOrder = -10;

        [Tooltip("这个排序的精灵不参与——遮蔽层要一直盖着，由 VisionFog 自己管")]
        [SerializeField] int skipSortingOrder = GrayBoxSorting.Fog;

        [Tooltip("扩散起点，相对画面中心的偏移；留空则用四象限默认点")]
        [SerializeField] Vector2[] seedOffsets =
        {
            new Vector2(-4.5f, 2.5f),
            new Vector2(4.5f, -2.0f),
            new Vector2(-3.0f, -2.8f),
            new Vector2(4.0f, 2.8f)
        };

        static readonly int SeedsId = Shader.PropertyToID("_Seeds");
        static readonly int RadiiId = Shader.PropertyToID("_Radii");
        static readonly int SoftnessId = Shader.PropertyToID("_Softness");

        const int MaxSeeds = 4;

        readonly Vector4[] seeds = new Vector4[MaxSeeds];
        readonly float[] radii = new float[MaxSeeds];
        readonly float[] maxRadii = new float[MaxSeeds];

        readonly List<SpriteRenderer> targets = new List<SpriteRenderer>();
        readonly List<Material> targetOriginals = new List<Material>();
        readonly List<SpriteRenderer> hidden = new List<SpriteRenderer>();
        readonly List<Material> hiddenOriginals = new List<Material>();

        Material runtime;

        public bool IsDissolving { get; private set; }

        public IEnumerator DissolveRoutine()
        {
            if (IsDissolving) yield break;
            if (dissolveMaterial == null) yield break;

            CollectTargets();
            if (targets.Count == 0) yield break;

            IsDissolving = true;
            runtime = new Material(dissolveMaterial);
            PrepareSeeds();
            Push();

            for (int i = 0; i < targets.Count; i++)
            {
                targetOriginals.Add(targets[i].sharedMaterial);
                targets[i].sharedMaterial = runtime;
            }

            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime / Mathf.Max(duration, 0.01f);
                UpdateRadii(Mathf.Clamp01(timer));
                Push();
                yield return null;
            }

            UpdateRadii(1f);
            Push();
            yield return null;

            HideAll();
            IsDissolving = false;
        }

        /// <summary>
        /// 进关时的"画回来"：从完全擦除的状态开始，让扩散半径反向收缩，
        /// 画面内容从种子点向外长出来。玩家在这期间应当是冻结的。
        /// </summary>
        public IEnumerator RevealRoutine()
        {
            if (IsDissolving) yield break;
            if (dissolveMaterial == null) yield break;

            CollectTargets();
            if (targets.Count == 0) yield break;

            IsDissolving = true;
            runtime = new Material(dissolveMaterial);
            PrepareSeeds();

            // 起始状态：整屏都被擦掉，只剩底下的空白画布
            for (int i = 0; i < MaxSeeds; i++) radii[i] = maxRadii[i];
            Push();

            for (int i = 0; i < targets.Count; i++)
            {
                targetOriginals.Add(targets[i].sharedMaterial);
                targets[i].sharedMaterial = runtime;
                targets[i].enabled = true;
            }

            // 先让空白画布停一会儿，再开始画
            if (revealHold > 0f) yield return new WaitForSeconds(revealHold);

            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime / Mathf.Max(revealDuration, 0.01f);
                UpdateRadii(1f - Mathf.Clamp01(timer));
                Push();
                yield return null;
            }

            UpdateRadii(0f);
            Push();
            yield return null;

            RestoreMaterials();
            IsDissolving = false;
        }

        void RestoreMaterials()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var renderer = targets[i];
                if (renderer == null) continue;

                renderer.sharedMaterial = targetOriginals[i];
                renderer.enabled = true;
            }

            targets.Clear();
            targetOriginals.Clear();

            if (runtime != null)
            {
                Destroy(runtime);
                runtime = null;
            }
        }

        /// <summary>下一关加载前恢复常驻对象的显示。</summary>
        public void ResetVisuals()
        {
            // 显现 / 擦除被中断时，先把正在用的材质还回去
            RestoreMaterials();

            for (int i = 0; i < hidden.Count; i++)
            {
                var renderer = hidden[i];
                if (renderer == null) continue;

                renderer.sharedMaterial = hiddenOriginals[i];
                renderer.enabled = true;
            }

            hidden.Clear();
            hiddenOriginals.Clear();

            IsDissolving = false;
        }

        void CollectTargets()
        {
            targets.Clear();
            targetOriginals.Clear();

            foreach (var renderer in FindObjectsOfType<SpriteRenderer>())
            {
                if (!renderer.enabled) continue;
                if (renderer.sortingOrder < minSortingOrder) continue;
                if (renderer.sortingOrder == skipSortingOrder) continue;

                targets.Add(renderer);
            }
        }

        void PrepareSeeds()
        {
            var cam = Camera.main;
            Vector2 center = cam != null ? (Vector2)cam.transform.position : Vector2.zero;
            float halfHeight = cam != null ? cam.orthographicSize : 4.5f;
            float halfWidth = halfHeight * (cam != null ? cam.aspect : 16f / 9f);

            int count = Mathf.Min(seedOffsets != null ? seedOffsets.Length : 0, MaxSeeds);

            for (int i = 0; i < MaxSeeds; i++)
            {
                radii[i] = 0f;

                if (i >= count)
                {
                    seeds[i] = Vector4.zero;
                    maxRadii[i] = 0f;
                    continue;
                }

                Vector2 point = center + seedOffsets[i];
                seeds[i] = new Vector4(point.x, point.y, 0f, 0f);

                float max = 0f;
                for (int c = 0; c < 4; c++)
                {
                    var corner = new Vector2(
                        center.x + ((c & 1) == 0 ? -halfWidth : halfWidth),
                        center.y + ((c & 2) == 0 ? -halfHeight : halfHeight));
                    max = Mathf.Max(max, Vector2.Distance(point, corner));
                }

                maxRadii[i] = max + softness;
            }
        }

        void UpdateRadii(float t)
        {
            for (int i = 0; i < MaxSeeds; i++)
            {
                if (maxRadii[i] <= 0f) { radii[i] = 0f; continue; }

                float start = i * 0.06f;
                float local = Mathf.Clamp01((t - start) / Mathf.Max(0.01f, 1f - start));
                radii[i] = Mathf.Lerp(0f, maxRadii[i], local);
            }
        }

        void Push()
        {
            if (runtime == null) return;

            runtime.SetVectorArray(SeedsId, seeds);
            runtime.SetFloatArray(RadiiId, radii);
            runtime.SetFloat(SoftnessId, softness);
        }

        void HideAll()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var renderer = targets[i];
                if (renderer == null) continue;

                renderer.sharedMaterial = targetOriginals[i];
                renderer.enabled = false;

                hidden.Add(renderer);
                hiddenOriginals.Add(targetOriginals[i]);
            }

            targets.Clear();
            targetOriginals.Clear();
        }
    }
}
