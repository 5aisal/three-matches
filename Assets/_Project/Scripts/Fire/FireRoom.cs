using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// Fire in one room of the past house: levels 0-3 drive a particle fire, a smoke layer, a flickering
    /// point light and a crackle loop (GAME_SPEC §10). Levels blend smoothly. While frozen nothing moves.
    /// </summary>
    public sealed class FireRoom : MonoBehaviour
    {
        private const int Levels = FireBlend.MaxLevel + 1;

        [SerializeField] private RoomId room;

        [Header("Parts")]
        [SerializeField] private ParticleSystem fire;
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private Light fireLight;
        [SerializeField] private AudioSource crackle;

        [Header("Per level (index = fire level 0-3)")]
        [SerializeField] private float[] fireRate = { 0f, 25f, 45f, 70f };
        [SerializeField] private float[] smokeRate = { 0f, 4f, 8f, 12f };
        [SerializeField] private float[] lightIntensity = { 0f, 1.5f, 3f, 5f };
        [SerializeField] private float[] crackleVolume = { 0f, 0.08f, 0.16f, 0.28f };
        [Tooltip("Width of the burning strip (room-local x) per level.")]
        [SerializeField] private float[] fireWidth = { 0.5f, 1.5f, 3f, 5.4f };
        [Tooltip("Centre of the burning strip (room-local x) per level.")]
        [SerializeField] private float[] fireCenterX = { 0f, 0f, 0f, 0f };

        [Header("Timing")]
        [SerializeField, Min(0f)] private float blendSeconds = 1.5f;
        [SerializeField, Min(0f)] private float warmupSeconds = 2f;
        [SerializeField, Min(0.1f)] private float flickerSpeed = 7f;
        [SerializeField, Range(0f, 1f)] private float flickerDepth = 0.3f;

        private ParticleSystem.EmissionModule fireEmission;
        private ParticleSystem.EmissionModule smokeEmission;
        private ParticleSystem.ShapeModule fireShape;
        private float blended;
        private float flickerClock;
        private bool frozen = true;
        private bool paused = true;
        private Vector3 fireShapeBase;

        public RoomId Room => room;
        public int TargetLevel { get; private set; }
        public float BlendedLevel => blended;
        public bool IsFrozen => frozen;
        public Light FireLight => fireLight;

        private void Awake()
        {
            fireEmission = fire.emission;
            smokeEmission = smoke.emission;
            fireShape = fire.shape;
            fireShapeBase = fireShape.position;

            crackle.clip = CrackleClip.Get();
            crackle.loop = true;
            crackle.playOnAwake = false;
            crackle.spatialBlend = 0f;

            Apply(true);
            // Pre-simulate so the fire is already alive the first time its strip appears, then hold it.
            fire.Simulate(warmupSeconds, true, true);
            smoke.Simulate(warmupSeconds, true, true);
        }

        private void Update()
        {
            if (frozen) return;

            flickerClock += Time.deltaTime;
            float next = FireBlend.Step(blended, TargetLevel, blendSeconds, Time.deltaTime);
            bool blending = next != blended;
            blended = next;
            if (blending) Apply(false);
            ApplyLight();
        }

        public void SetTargetLevel(int level)
        {
            TargetLevel = Mathf.Clamp(level, 0, FireBlend.MaxLevel);
        }

        /// <summary>Snaps to a level with no blend (debug jumps and start-up).</summary>
        public void SnapToLevel(int level)
        {
            SetTargetLevel(level);
            blended = TargetLevel;
            Apply(false);
            ApplyLight();
        }

        public void SetFrozen(bool value)
        {
            if (frozen == value && paused == value) return;
            frozen = value;
            SetPaused(value);
        }

        private void SetPaused(bool value)
        {
            paused = value;
            if (value)
            {
                fire.Pause();
                smoke.Pause();
                crackle.Pause();
                return;
            }

            fire.Play();
            smoke.Play();
            if (crackle.volume > 0f) crackle.UnPause();
            if (!crackle.isPlaying && crackle.volume > 0f) crackle.Play();
        }

        private void Apply(bool initial)
        {
            fireEmission.rateOverTime = FireBlend.Sample(fireRate, blended);
            smokeEmission.rateOverTime = FireBlend.Sample(smokeRate, blended);

            float width = FireBlend.Sample(fireWidth, blended);
            fireShape.scale = new Vector3(Mathf.Max(0.05f, width), fireShape.scale.y, fireShape.scale.z);
            fireShape.position = fireShapeBase + new Vector3(FireBlend.Sample(fireCenterX, blended), 0f, 0f);

            crackle.volume = FireBlend.Sample(crackleVolume, blended);
            if (!paused && crackle.volume > 0f && !crackle.isPlaying) crackle.Play();
            if (initial) ApplyLight();
        }

        private void ApplyLight()
        {
            float baseIntensity = FireBlend.Sample(lightIntensity, blended);
            float noise = Mathf.PerlinNoise(flickerClock * flickerSpeed, (int)room * 3.7f);
            fireLight.intensity = baseIntensity * (1f - flickerDepth + flickerDepth * 2f * noise);
            bool on = baseIntensity > 0.001f;
            if (fireLight.enabled != on) fireLight.enabled = on;
        }
    }
}
