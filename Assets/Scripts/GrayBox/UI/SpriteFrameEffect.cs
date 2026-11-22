using UnityEngine;

namespace ThreeCup.GrayBox
{
    /// <summary>
    /// 一次性序列帧特效。播完销毁，或循环播放。
    /// 用来做死亡水痕、清洁晕这类不需要控制的动画。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFrameEffect : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;
        [SerializeField] Sprite[] frames = new Sprite[0];
        [SerializeField] float fps = 14f;
        [SerializeField] bool loop;
        [SerializeField] bool destroyOnFinish = true;

        float timer;
        int index;

        public void Configure(SpriteRenderer renderer, Sprite[] newFrames, float newFps, bool shouldLoop)
        {
            target = renderer;
            frames = newFrames;
            fps = newFps;
            loop = shouldLoop;
        }

        void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            timer = 0f;
            index = 0;
            Apply(0);
        }

        void Update()
        {
            if (frames == null || frames.Length == 0 || target == null) return;

            timer += Time.deltaTime;
            float step = 1f / Mathf.Max(fps, 0.01f);

            while (timer >= step)
            {
                timer -= step;
                index++;

                if (index >= frames.Length)
                {
                    if (loop)
                    {
                        index = 0;
                    }
                    else
                    {
                        Apply(frames.Length - 1);
                        if (destroyOnFinish) Destroy(gameObject);
                        return;
                    }
                }
            }

            Apply(index);
        }

        void Apply(int i)
        {
            if (target == null || frames == null || frames.Length == 0) return;
            target.sprite = frames[Mathf.Clamp(i, 0, frames.Length - 1)];
        }
    }
}
