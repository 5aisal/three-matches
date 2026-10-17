using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// Holding Hum (H) while a window is open and the SONG memory is burned makes the shadow hum Noor's song
    /// (GAME_SPEC §7, §12): a quiet music-box loop (placeholder) and a few drifting musical notes.
    /// </summary>
    public sealed class ShadowHumming : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameInput input;
        [SerializeField] private MatchController matches;
        [SerializeField] private ShadowController shadow;
        [SerializeField] private ParticleSystem notes;
        [SerializeField] private AudioSource source;

        [Header("Placeholder lullaby (6 notes, looped)")]
        [SerializeField] private float[] noteHz = { 523.25f, 587.33f, 659.25f, 783.99f, 659.25f, 587.33f };
        [SerializeField, Min(0.1f)] private float noteSeconds = 0.5f;
        [Tooltip("Higher = duller, more like humming.")]
        [SerializeField, Range(0.01f, 1f)] private float lowPass = 0.12f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.25f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 0.25f;

        private float level;

        /// <summary>True while the shadow is humming this frame.</summary>
        public bool IsHumming { get; private set; }

        private void Awake()
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.clip = BuildLoop();
            SetNotes(false);
        }

        private void Update()
        {
            bool want = shadow.IsActive
                && matches.State.IsBurned(MemoryId.Song)
                && input.Hum.IsPressed();

            if (want != IsHumming)
            {
                IsHumming = want;
                SetNotes(want);
            }

            float target = want ? volume : 0f;
            level = Mathf.MoveTowards(level, target, volume / fadeSeconds * Time.unscaledDeltaTime);
            source.volume = level;

            if (level > 0f && !source.isPlaying) source.Play();
            else if (level <= 0f && source.isPlaying) source.Pause();
        }

        private void SetNotes(bool on)
        {
            ParticleSystem.EmissionModule emission = notes.emission;
            emission.enabled = on;
            if (on && !notes.isPlaying) notes.Play();
        }

        private AudioClip BuildLoop()
        {
            int rate = AudioSettings.outputSampleRate;
            int perNote = Mathf.CeilToInt(rate * noteSeconds);
            var data = new float[perNote * noteHz.Length];
            float smoothed = 0f;
            for (int n = 0; n < noteHz.Length; n++)
            {
                for (int i = 0; i < perNote; i++)
                {
                    float t = (float)i / rate;
                    float tone = Mathf.Sin(2f * Mathf.PI * noteHz[n] * t)
                        + 0.3f * Mathf.Sin(2f * Mathf.PI * noteHz[n] * 2f * t);
                    float env = Mathf.Exp(-t * 3f) * Mathf.Min(1f, t * 100f) * Mathf.Min(1f, (noteSeconds - t) * 100f);
                    smoothed += lowPass * (tone * env * 0.5f - smoothed);
                    data[n * perNote + i] = smoothed;
                }
            }

            AudioClip clip = AudioClip.Create("HummingLoop", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
