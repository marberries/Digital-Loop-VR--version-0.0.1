using UnityEngine;
using UnityEngine.Rendering.Universal;
using DigitalCurtain.Hospital;

namespace DigitalCurtain.Hospital
{
    /// <summary>
    /// Turns the silent unease counter into something the player can feel:
    /// a low ward drone, a quickening heartbeat, a tightening vignette and a
    /// little chromatic bleed. Everything eases off as soon as the player
    /// looks away.
    /// </summary>
    public class UneaseFeedback : MonoBehaviour
    {
        [Header("Source")]
        public PatientBreathingAndIntro intro;

        [Header("Post Processing")]
        public Vignette vignette;
        public ChromaticAberration chromaticAberration;
        public float baseVignette = 0.36f;
        public float maxVignetteBoost = 0.22f;
        public float baseChromatic = 0.06f;
        public float maxChromatic = 0.30f;

        [Header("Audio")]
        public AudioSource droneSource;
        public AudioSource heartbeatSource;
        public float maxDroneVolume = 0.26f;
        public float heartbeatVolume = 0.55f;
        public float heartbeatMinBpm = 64f;
        public float heartbeatMaxBpm = 124f;

        private AudioClip _droneClip;
        private AudioClip _heartClip;
        private float _smoothedUnease;
        private float _beatTimer;

        private void Start()
        {
            _droneClip = GenerateDroneLoop();
            _heartClip = GenerateHeartbeatClip();

            if (droneSource != null)
            {
                droneSource.clip = _droneClip;
                droneSource.loop = true;
                droneSource.volume = 0f;
                droneSource.Play();
            }

            _beatTimer = 0.9f;
        }

        private void Update()
        {
            if (intro == null) return;

            float target = Mathf.Clamp01(intro.Unease);
            _smoothedUnease = Mathf.Lerp(_smoothedUnease, target, 1f - Mathf.Exp(-2.4f * Time.deltaTime));
            float unease = _smoothedUnease;

            float pulse = Mathf.Sin(Time.time * Mathf.Lerp(1.6f, 6.4f, unease) * Mathf.PI * 2f) * unease;

            if (vignette != null)
            {
                vignette.intensity.Override(baseVignette + unease * maxVignetteBoost + pulse * 0.02f);
            }

            if (chromaticAberration != null && !intro.SequenceRunning)
            {
                chromaticAberration.intensity.Override(baseChromatic + unease * maxChromatic);
            }

            if (droneSource != null)
            {
                droneSource.volume = unease * maxDroneVolume;
                droneSource.pitch = 1f + unease * 0.18f;
            }

            if (heartbeatSource != null && unease > 0.08f)
            {
                float bpm = Mathf.Lerp(heartbeatMinBpm, heartbeatMaxBpm, unease);
                float interval = 60f / bpm;

                _beatTimer -= Time.deltaTime;
                if (_beatTimer <= 0f)
                {
                    _beatTimer = interval;
                    heartbeatSource.PlayOneShot(_heartClip, heartbeatVolume * Mathf.Lerp(0.25f, 1f, unease));
                }
            }
            else if (heartbeatSource != null)
            {
                _beatTimer = 0.6f;
            }
        }

        private AudioClip GenerateDroneLoop()
        {
            int sampleRate = 22050;
            int sampleCount = sampleRate * 6;
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(4041);
            float filtered = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float hum = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.42f;
                hum += Mathf.Sin(2f * Mathf.PI * 73f * t) * 0.22f;
                hum += Mathf.Sin(2f * Mathf.PI * 109f * t) * 0.10f;

                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                filtered = 0.995f * filtered + 0.005f * noise;

                float wobble = 0.72f + 0.28f * Mathf.Sin(2f * Mathf.PI * 0.09f * t);
                samples[i] = Mathf.Clamp((hum * 0.35f + filtered * 6.5f) * wobble, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Unease_Drone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateHeartbeatClip()
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * 0.62f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float value = 0f;

                value += Thump(t, 0.02f, 0.085f, 62f, 1.0f);
                value += Thump(t, 0.20f, 0.075f, 54f, 0.72f);

                samples[i] = Mathf.Clamp(value, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Unease_Heartbeat", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float Thump(float t, float start, float length, float frequency, float gain)
        {
            if (t < start || t > start + length) return 0f;

            float local = t - start;
            float envelope = Mathf.Exp(-local * 34f) * Mathf.Clamp01(local / 0.006f);
            return Mathf.Sin(2f * Mathf.PI * frequency * local) * envelope * gain * 0.85f;
        }
    }
}
