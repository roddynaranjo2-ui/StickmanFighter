// AudioManager.cs — Singleton persistente. Genera los SFX PROCEDURALMENTE en runtime
// (sin necesidad de archivos .wav/.mp3 en el proyecto). FIX G-06 SPRINT #3.
//
// Razón de diseño: el proyecto no incluye assets de audio. Para tener feedback sonoro
// inmediato sin meter binarios al repo, generamos onditas con envolventes ADSR procedurales.

using System.Collections.Generic;
using UnityEngine;

namespace StickmanFighter.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager? Instance { get; private set; }

        [Header("Mixer")]
        [Range(0f, 1f)] [SerializeField] private float _masterVolume = 0.7f;
        [SerializeField] private int _pooledSources = 8;

        private readonly List<AudioSource> _pool = new();
        private readonly Dictionary<SfxId, AudioClip> _clips = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildPool();
            GenerateAllClips();
        }

        private void BuildPool()
        {
            for (int i = 0; i < _pooledSources; i++)
            {
                var go = new GameObject($"SfxSource_{i}");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 2D
                _pool.Add(src);
            }
        }

        private void GenerateAllClips()
        {
            // Frecuencias bajas y duraciones cortas — feel arcade
            _clips[SfxId.Jump]      = GenerateTone(440f, 0.12f, 0.02f, 0.05f, 0.6f, 0.05f, WaveShape.Square);
            _clips[SfxId.Land]      = GenerateTone(180f, 0.10f, 0.01f, 0.03f, 0.4f, 0.06f, WaveShape.Triangle);
            _clips[SfxId.Punch]     = GenerateNoise(0.08f, 0.005f, 0.02f, 0.6f, 0.05f);
            _clips[SfxId.Kick]      = GenerateNoise(0.12f, 0.005f, 0.03f, 0.7f, 0.08f);
            _clips[SfxId.Hit]       = GenerateTone(120f, 0.15f, 0.002f, 0.04f, 0.7f, 0.10f, WaveShape.Square);
            _clips[SfxId.Death]     = GenerateTone(80f,  0.45f, 0.01f, 0.10f, 0.7f, 0.30f, WaveShape.Sine);
            _clips[SfxId.MenuClick] = GenerateTone(880f, 0.06f, 0.005f, 0.01f, 0.5f, 0.04f, WaveShape.Square);
        }

        public void PlaySfx(SfxId id, Vector3 worldPos)
        {
            if (!_clips.TryGetValue(id, out var clip) || clip == null) return;
            var src = GetFreeSource();
            src.transform.position = worldPos;
            src.volume = _masterVolume;
            src.pitch  = 1f + Random.Range(-0.05f, 0.05f);
            src.PlayOneShot(clip);
        }

        private AudioSource GetFreeSource()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (!_pool[i].isPlaying) return _pool[i];
            return _pool[0]; // roba la más vieja
        }

        // ───────────────────── Procedural Audio ─────────────────────
        private enum WaveShape { Sine, Square, Triangle }

        private static AudioClip GenerateTone(float freq, float total, float attack, float decay,
                                              float sustainLevel, float release, WaveShape shape)
        {
            const int sr = 44100;
            int n = Mathf.CeilToInt(total * sr);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float phase = t * freq;
                float s = shape switch
                {
                    WaveShape.Sine     => Mathf.Sin(phase * Mathf.PI * 2f),
                    WaveShape.Square   => (Mathf.Repeat(phase, 1f) < 0.5f) ? 1f : -1f,
                    WaveShape.Triangle => 4f * Mathf.Abs(Mathf.Repeat(phase, 1f) - 0.5f) - 1f,
                    _ => 0f
                };
                data[i] = s * Envelope(t, attack, decay, sustainLevel, release, total);
            }
            var clip = AudioClip.Create($"proc_tone_{freq}", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateNoise(float total, float attack, float decay,
                                               float sustainLevel, float release)
        {
            const int sr = 44100;
            int n = Mathf.CeilToInt(total * sr);
            var data = new float[n];
            var rnd = new System.Random(12345);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float s = (float)(rnd.NextDouble() * 2.0 - 1.0);
                data[i] = s * Envelope(t, attack, decay, sustainLevel, release, total) * 0.8f;
            }
            var clip = AudioClip.Create("proc_noise", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Envelope(float t, float a, float d, float sustainLevel, float r, float total)
        {
            if (t < a) return Mathf.Lerp(0f, 1f, t / Mathf.Max(0.0001f, a));
            if (t < a + d) return Mathf.Lerp(1f, sustainLevel, (t - a) / Mathf.Max(0.0001f, d));
            float releaseStart = total - r;
            if (t < releaseStart) return sustainLevel;
            return Mathf.Lerp(sustainLevel, 0f, (t - releaseStart) / Mathf.Max(0.0001f, total - releaseStart));
        }
    }
}
