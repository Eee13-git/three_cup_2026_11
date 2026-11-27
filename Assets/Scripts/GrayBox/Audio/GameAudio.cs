using System.Collections.Generic;
using UnityEngine;

namespace ThreeCup.Audio
{
    public enum Sfx
    {
        Jump,
        Land,
        Death,
        Clean,
        Switch,
        Checkpoint,
        Win,
        Fail,
        UiClick,
    }

    /// <summary>
    /// 全局音效播放器：音频从 Resources/Audio 读取，音量跟随 GameSettings 的主音量 × SFX 音量。
    /// 场景切换时不销毁，由 GameAudioBridge 负责把游戏事件接到这里。
    /// </summary>
    public sealed class GameAudio : MonoBehaviour
    {
        const int VoiceCount = 6;

        // 背景音乐：全局一条，跨场景不断
        const string BgmResource = "Audio/bgm_main";
        const float BgmLevel = 0.55f;
        const float BgmFadeSeconds = 1.2f;

        static GameAudio instance;

        readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
        AudioSource[] voices;
        AudioSource bgm;
        int cursor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Ensure()
        {
            if (instance != null) return;

            var go = new GameObject("GameAudio");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameAudio>();
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            clips[Sfx.Jump] = Prepare(Load("sfx_jump"), 0.32f);
            // 落地/存档/UI 暂时复用已有音色（低/高音高区分），后续可换成独立素材
            clips[Sfx.Land] = Prepare(Load("sfx_land") ?? Load("sfx_jump"), 0.26f);
            clips[Sfx.Death] = Prepare(Load("sfx_death"), 0.90f);
            clips[Sfx.Clean] = Prepare(Load("sfx_clean"), 1.10f);
            clips[Sfx.Switch] = Prepare(Load("sfx_switch"), 0.30f);
            clips[Sfx.Checkpoint] = Prepare(Load("sfx_checkpoint") ?? Load("sfx_clean"), 0.80f);
            clips[Sfx.Win] = Prepare(Load("sfx_win"), 2.40f);
            clips[Sfx.Fail] = Prepare(Load("sfx_fail"), 1.60f);
            clips[Sfx.UiClick] = Prepare(Load("sfx_click") ?? Load("sfx_switch"), 0.18f);

            voices = new AudioSource[VoiceCount];
            for (int i = 0; i < VoiceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.volume = 1f;
                voices[i] = source;
            }

            SetupBgm();
        }

        /// <summary>
        /// 背景音乐：一条循环音轨，跟着 GameAudio 跨场景不销毁。
        /// 音量 = 主音量 × BGM 音量 × BgmLevel，每帧平滑靠拢，
        /// 所以在设置面板里拖滑条能立刻听出变化，开场也是淡入而不是突然响起。
        /// </summary>
        void SetupBgm()
        {
            var clip = Resources.Load<AudioClip>(BgmResource);
            if (clip == null)
            {
                Debug.LogWarning($"[Audio] 找不到背景音乐 Resources/{BgmResource}");
                return;
            }

            bgm = gameObject.AddComponent<AudioSource>();
            bgm.clip = clip;
            bgm.loop = true;
            bgm.playOnAwake = false;
            bgm.spatialBlend = 0f;
            bgm.volume = 0f;
            bgm.Play();
        }

        void Update()
        {
            if (bgm == null) return;

            float target = BgmLevel * BgmScale;
            bgm.volume = Mathf.MoveTowards(bgm.volume, target, Time.unscaledDeltaTime / BgmFadeSeconds);
        }

        static AudioClip Load(string name) => Resources.Load<AudioClip>("Audio/" + name);

        /// <summary>
        /// 生成的音效开头often 有一段静音/弱起，直接播会让人误以为"声音来晚了"。
        /// 这里裁掉起音前的空白、限制总长并做淡出，让起音对齐触发瞬间。
        /// </summary>
        static AudioClip Prepare(AudioClip source, float maxSeconds)
        {
            if (source == null) return null;

            int channels = source.channels;
            int total = source.samples;
            var data = new float[total * channels];
            source.GetData(data, 0);

            float peak = 0f;
            for (int i = 0; i < data.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            if (peak < 0.001f) return source;

            float gate = Mathf.Max(0.02f, peak * 0.15f);
            int first = 0;
            for (int sample = 0; sample < total; sample++)
            {
                bool hit = false;
                for (int c = 0; c < channels; c++)
                {
                    if (Mathf.Abs(data[sample * channels + c]) <= gate) continue;
                    hit = true;
                    break;
                }

                if (!hit) continue;

                int preroll = Mathf.RoundToInt(source.frequency * 0.008f);
                first = Mathf.Max(0, sample - preroll);
                break;
            }

            int wanted = Mathf.RoundToInt(maxSeconds * source.frequency);
            int length = Mathf.Min(total - first, wanted);
            if (length <= 1) return source;

            var trimmed = new float[length * channels];
            System.Array.Copy(data, first * channels, trimmed, 0, trimmed.Length);

            // 生成音效电平差异很大（死亡音峰值只有 0.03），统一归一化到 0.7
            float trimmedPeak = 0f;
            for (int i = 0; i < trimmed.Length; i++) trimmedPeak = Mathf.Max(trimmedPeak, Mathf.Abs(trimmed[i]));
            if (trimmedPeak > 0.0005f)
            {
                float gain = Mathf.Min(0.7f / trimmedPeak, 20f);
                if (gain > 1.001f || gain < 0.999f)
                    for (int i = 0; i < trimmed.Length; i++) trimmed[i] *= gain;
            }

            int fade = Mathf.Min(length, Mathf.RoundToInt(source.frequency * 0.05f));
            for (int i = 0; i < fade; i++)
            {
                float k = 1f - (float)i / fade;
                int index = length - 1 - i;
                for (int c = 0; c < channels; c++) trimmed[index * channels + c] *= k;
            }

            var clip = AudioClip.Create(source.name + "_trim", length, channels, source.frequency, false);
            clip.SetData(trimmed, 0);
            return clip;
        }

        public static void Play(Sfx sfx, float volume = 1f, float pitch = 1f)
        {
            if (instance == null) Ensure();
            if (instance == null) return;

            instance.PlayInternal(sfx, volume, pitch);
        }

        void PlayInternal(Sfx sfx, float volume, float pitch)
        {
            if (!clips.TryGetValue(sfx, out var clip) || clip == null) return;
            if (voices == null || voices.Length == 0) return;

            var source = voices[cursor];
            cursor = (cursor + 1) % voices.Length;

            source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            source.PlayOneShot(clip, Mathf.Clamp01(volume) * VolumeScale);
        }

        static float VolumeScale
        {
            get
            {
                var settings = UI.GameSettings.Instance;
                if (settings == null) return 1f;

                return Mathf.Clamp01(settings.MasterVolume * settings.SfxVolume);
            }
        }

        /// <summary>背景音乐的音量系数：主音量 × BGM 音量。</summary>
        static float BgmScale
        {
            get
            {
                var settings = UI.GameSettings.Instance;
                if (settings == null) return 1f;

                return Mathf.Clamp01(settings.MasterVolume * settings.BgmVolume);
            }
        }
    }
}
