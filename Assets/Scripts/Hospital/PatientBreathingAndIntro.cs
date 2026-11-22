using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Rendering.Universal;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DigitalCurtain.Hospital
{
    public class PatientBreathingAndIntro : MonoBehaviour
    {
        [Header("Optional Real Audio Overrides (Drop .wav/.mp3 here)")]
        public AudioClip calmBreathOverride;
        public AudioClip panicBreathOverride;

        [Header("Breathing Motion & Timing")]
        public float calmBreathingRate = 0.25f;
        public float panicBreathingRate = 0.68f;
        public float verticalAmplitude = 0.014f;
        public float pitchAmplitude = 0.95f;
        public float handlerEnterTime = 11.5f;
        public bool enableAutoSequence = true;

        [Header("Scene Rig References")]
        public Transform headsetAssembly;
        public Transform handlerRoot;
        public Transform handlerTorso;
        public Transform handlerLeftArm;
        public Transform handlerRightArm;
        public Transform playerLeftArm;
        public Transform playerRightArm;
        public ECGMonitorController ecgMonitor;

        [Header("Visor Display & Monologue UI")]
        public Image blackoutImage;
        public RawImage staticGlitchImage;
        public GameObject monologuePanel;
        public Text monologueText;

        [Header("URP Post-Processing")]
        public ChromaticAberration postChromaticAberration;

        private Camera _cam;
        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;
        private float _elapsed;
        private float _breathPhase;
        private bool _lastInhaleTriggered;

        private bool _sequenceTriggered;
        private float _seqTime;
        private bool _clampSoundPlayed;
        private bool _staticSoundPlayed;

        private AudioSource _breathAudio;
        private AudioSource _sfxAudio;
        private AudioSource _staticAudio;
        private AudioClip _generatedCalmBreath;
        private AudioClip _generatedPanicBreath;
        private AudioClip _clampClip;
        private AudioClip _staticGlitchClip;

        private Texture2D _staticTex;
        private Color32[] _staticPixels;
        private System.Random _rng;
        private int _lastDialogueIndex = -1;

        private static readonly (float time, string line)[] MonologueLines = new (float, string)[]
        {
            (0.7f,  "Wha... where am I? My head feels like lead..."),
            (4.6f,  "That beeping... a hospital ward? I don't remember checking in."),
            (8.8f,  "No doctors, no nurses... why is the air so cold?"),
            (12.0f, "Wait—who's standing there?! Hey! What is that on your face?!"),
            (15.1f, "Get back! Don't put that thing on me—no, stop—!"),
            (18.0f, "I can't see the room... what is this static?! What's happening to me?!")
        };

        private void Start()
        {
            _cam = GetComponent<Camera>();
            _rng = new System.Random(2026);

            if (!XRSettings.isDeviceActive)
            {
                Behaviour[] behaviours = GetComponents<Behaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] != null && behaviours[i].GetType().Name.Contains("TrackedPoseDriver"))
                    {
                        behaviours[i].enabled = false;
                    }
                }
            }

            _initialLocalPos = transform.localPosition;
            _initialLocalRot = transform.localRotation;

            _breathAudio = gameObject.AddComponent<AudioSource>();
            _breathAudio.spatialBlend = 0f;

            _sfxAudio = gameObject.AddComponent<AudioSource>();
            _sfxAudio.spatialBlend = 0f;

            _staticAudio = gameObject.AddComponent<AudioSource>();
            _staticAudio.spatialBlend = 0f;
            _staticAudio.loop = true;

            _generatedCalmBreath = GenerateCleanHumanBreathing("Calm_Breath", 3.8f, false);
            _generatedPanicBreath = GenerateCleanHumanBreathing("Panic_Breath", 1.45f, true);
            _clampClip = GenerateHeadsetClampClip();
            _staticGlitchClip = GenerateStaticInterferenceClip();

            if (headsetAssembly != null)
            {
                headsetAssembly.gameObject.SetActive(false);
            }

            if (handlerRoot != null)
            {
                handlerRoot.position = new Vector3(-2.65f, 0f, -1.80f);
                handlerRoot.rotation = Quaternion.Euler(0f, 155f, 0f);
                handlerRoot.gameObject.SetActive(false);
            }

            _staticTex = new Texture2D(160, 90, TextureFormat.RGBA32, false);
            _staticTex.filterMode = FilterMode.Point;
            _staticPixels = new Color32[160 * 90];
            if (staticGlitchImage != null)
            {
                staticGlitchImage.texture = _staticTex;
                staticGlitchImage.color = new Color(1f, 1f, 1f, 0f);
            }

            if (blackoutImage != null)
            {
                blackoutImage.color = new Color(0f, 0f, 0f, 1f);
            }

            if (monologueText != null)
            {
                monologueText.text = "";
            }
            if (monologuePanel != null)
            {
                monologuePanel.SetActive(false);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            if (!_sequenceTriggered)
            {
                float wakeT = Mathf.Clamp01(_elapsed / 3.2f);
                if (blackoutImage != null)
                {
                    float lidAlpha = 1.0f - Mathf.SmoothStep(0f, 1f, wakeT);
                    blackoutImage.color = new Color(0f, 0f, 0f, lidAlpha * 0.92f);
                }
                if (_cam != null && !XRSettings.isDeviceActive)
                {
                    _cam.fieldOfView = Mathf.Lerp(66f, 58f, Mathf.SmoothStep(0f, 1f, wakeT));
                }
            }

            bool spacePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) spacePressed = true;
#else
            if (Input.GetKeyDown(KeyCode.Space)) spacePressed = true;
#endif
            if (!_sequenceTriggered && (spacePressed || (enableAutoSequence && _elapsed >= handlerEnterTime)))
            {
                _sequenceTriggered = true;
                _seqTime = 0f;
                _lastInhaleTriggered = false;

                if (handlerRoot != null) handlerRoot.gameObject.SetActive(true);
                if (headsetAssembly != null) headsetAssembly.gameObject.SetActive(true);

                if (ecgMonitor != null)
                {
                    ecgMonitor.beatsPerMinute = 138f;
                    ecgMonitor.scrollSpeed = 0.80f;
                }
            }

            float currentRate = _sequenceTriggered
                ? Mathf.Lerp(calmBreathingRate, panicBreathingRate, Mathf.Clamp01(_seqTime / 4.5f))
                : calmBreathingRate;

            _breathPhase += Time.deltaTime * currentRate;
            float cycle = _breathPhase % 1.0f;
            float wave = cycle < 0.35f
                ? Mathf.Sin((cycle / 0.35f) * Mathf.PI * 0.5f)
                : Mathf.Cos(((cycle - 0.35f) / 0.65f) * Mathf.PI * 0.5f);
            wave = (wave - 0.4f) * 1.5f;

            if (cycle < 0.10f && !_lastInhaleTriggered)
            {
                _lastInhaleTriggered = true;
                PlayBreathSound();
            }
            else if (cycle > 0.50f)
            {
                _lastInhaleTriggered = false;
            }

            float ampScale = _sequenceTriggered ? 1.35f : 1.0f;
            transform.localPosition = _initialLocalPos + new Vector3(0f, wave * verticalAmplitude * ampScale, 0f);
            transform.localRotation = _initialLocalRot * Quaternion.Euler(-wave * pitchAmplitude * ampScale, 0f, 0f);

            UpdatePlayerArms(wave);

            float narrativeClock = _sequenceTriggered ? (handlerEnterTime + _seqTime) : _elapsed;
            UpdateMonologue(narrativeClock);

            if (_sequenceTriggered)
            {
                _seqTime += Time.deltaTime;
                RunHandlerAndVisorSequence();
            }
        }

        private void UpdatePlayerArms(float breathWave)
        {
            float tenseLift = _sequenceTriggered ? Mathf.Clamp01(_seqTime / 2.2f) * 0.055f : 0f;
            float tensePitch = _sequenceTriggered ? Mathf.Clamp01(_seqTime / 2.2f) * -14f : 0f;

            if (playerLeftArm != null)
            {
                playerLeftArm.localPosition = new Vector3(-0.26f, -0.34f + breathWave * 0.005f + tenseLift, 0.34f);
                playerLeftArm.localRotation = Quaternion.Euler(14f + tensePitch, 18f, -8f);
            }
            if (playerRightArm != null)
            {
                playerRightArm.localPosition = new Vector3(0.26f, -0.34f + breathWave * 0.005f + tenseLift, 0.34f);
                playerRightArm.localRotation = Quaternion.Euler(14f + tensePitch, -18f, 8f);
            }
        }

        private void RunHandlerAndVisorSequence()
        {
            if (_seqTime <= 3.0f)
            {
                float t = Mathf.SmoothStep(0f, 1f, _seqTime / 3.0f);
                if (handlerRoot != null)
                {
                    handlerRoot.position = Vector3.Lerp(
                        new Vector3(-2.65f, 0f, -1.80f),
                        new Vector3(-2.32f, 0f, -3.72f),
                        t
                    );
                    handlerRoot.rotation = Quaternion.Slerp(
                        Quaternion.Euler(0f, 155f, 0f),
                        Quaternion.Euler(0f, 122f, 0f),
                        t
                    );
                }
                if (handlerTorso != null)
                {
                    handlerTorso.localRotation = Quaternion.Euler(Mathf.Lerp(4f, 14f, t), 0f, 0f);
                }
                PositionHeadsetInHandlerHands();
            }
            else if (_seqTime <= 5.6f)
            {
                float t = Mathf.SmoothStep(0f, 1f, (_seqTime - 3.0f) / 2.6f);
                if (handlerTorso != null)
                {
                    handlerTorso.localRotation = Quaternion.Euler(Mathf.Lerp(14f, 36f, t), 0f, 0f);
                }
                if (handlerLeftArm != null)
                {
                    handlerLeftArm.localRotation = Quaternion.Euler(Mathf.Lerp(-42f, -78f, t), 14f, -10f);
                }
                if (handlerRightArm != null)
                {
                    handlerRightArm.localRotation = Quaternion.Euler(Mathf.Lerp(-42f, -78f, t), -14f, 10f);
                }

                if (headsetAssembly != null)
                {
                    Vector3 handPos = (handlerLeftArm != null && handlerRightArm != null)
                        ? (handlerLeftArm.position + handlerRightArm.position) * 0.5f + handlerRoot.forward * 0.24f + Vector3.up * 0.04f
                        : transform.position + transform.forward * 0.45f;

                    Vector3 targetPos = transform.position + transform.forward * 0.04f;
                    headsetAssembly.position = Vector3.Lerp(handPos, targetPos, t);

                    Quaternion handRot = Quaternion.LookRotation(transform.position - headsetAssembly.position, Vector3.up);
                    Quaternion wornRot = transform.rotation;
                    headsetAssembly.rotation = Quaternion.Slerp(handRot, wornRot, t);
                }

                if (t >= 0.94f && !_clampSoundPlayed)
                {
                    _clampSoundPlayed = true;
                    if (_sfxAudio != null && _clampClip != null)
                    {
                        _sfxAudio.PlayOneShot(_clampClip);
                    }
                }
            }
            else
            {
                if (headsetAssembly != null)
                {
                    headsetAssembly.gameObject.SetActive(false);
                }

                if (blackoutImage != null)
                {
                    blackoutImage.color = Color.black;
                }

                if (!_staticSoundPlayed)
                {
                    _staticSoundPlayed = true;
                    if (_staticAudio != null && _staticGlitchClip != null)
                    {
                        _staticAudio.clip = _staticGlitchClip;
                        _staticAudio.volume = 0.55f;
                        _staticAudio.Play();
                    }
                }

                float insideTime = _seqTime - 5.6f;

                if (insideTime < 3.4f)
                {
                    RenderStaticAndGlitchFrame(insideTime);
                    if (staticGlitchImage != null)
                    {
                        float flickerAlpha = Mathf.Clamp01(insideTime / 0.35f) * Random.Range(0.72f, 1.0f);
                        staticGlitchImage.color = new Color(1f, 1f, 1f, flickerAlpha);
                    }
                    if (postChromaticAberration != null)
                    {
                        postChromaticAberration.intensity.Override(Random.Range(0.25f, 0.75f));
                    }
                }
                else if (insideTime < 5.2f)
                {
                    float bootFade = 1.0f - Mathf.Clamp01((insideTime - 4.3f) / 0.9f);
                    RenderNeuralBootGrid(insideTime);
                    if (staticGlitchImage != null)
                    {
                        staticGlitchImage.color = new Color(1f, 1f, 1f, bootFade);
                    }
                    if (_staticAudio != null)
                    {
                        _staticAudio.volume = bootFade * 0.45f;
                    }
                    if (postChromaticAberration != null)
                    {
                        postChromaticAberration.intensity.Override(0.06f);
                    }
                    if (ecgMonitor != null)
                    {
                        ecgMonitor.isFlatline = true;
                    }
                }
                else
                {
                    if (staticGlitchImage != null) staticGlitchImage.color = new Color(1f, 1f, 1f, 0f);
                    if (_staticAudio != null && _staticAudio.isPlaying) _staticAudio.Stop();
                    if (monologueText != null) monologueText.text = "";
                    if (monologuePanel != null) monologuePanel.SetActive(false);
                    if (postChromaticAberration != null) postChromaticAberration.intensity.Override(0.06f);
                }
            }
        }

        private void PositionHeadsetInHandlerHands()
        {
            if (headsetAssembly == null || handlerLeftArm == null || handlerRightArm == null) return;
            Vector3 midHands = (handlerLeftArm.position + handlerRightArm.position) * 0.5f;
            headsetAssembly.position = midHands + handlerRoot.forward * 0.24f + Vector3.up * 0.04f;
            headsetAssembly.rotation = Quaternion.LookRotation(handlerRoot.forward, Vector3.up);
        }

        private void RenderStaticAndGlitchFrame(float t)
        {
            if (_staticTex == null) return;
            int w = 160;
            int h = 90;

            int glitchBandY = _rng.Next(0, h);
            int glitchBandH = _rng.Next(4, 16);
            bool heavyTear = (_rng.NextDouble() > 0.62);

            for (int y = 0; y < h; y++)
            {
                bool inBand = heavyTear && (y >= glitchBandY && y < glitchBandY + glitchBandH);
                bool scanline = (y % 3 == 0);

                for (int x = 0; x < w; x++)
                {
                    byte lum = (byte)_rng.Next(15, 195);
                    if (scanline) lum = (byte)(lum / 2);

                    if (inBand)
                    {
                        bool cyanOrRed = ((x + (int)(t * 120f)) % 16 < 8);
                        _staticPixels[y * w + x] = cyanOrRed
                            ? new Color32(25, 235, 255, 255)
                            : new Color32(255, 45, 75, 255);
                    }
                    else
                    {
                        _staticPixels[y * w + x] = new Color32(lum, (byte)(lum + 8), (byte)(lum + 14), 255);
                    }
                }
            }

            _staticTex.SetPixels32(_staticPixels);
            _staticTex.Apply(false);
        }

        private void RenderNeuralBootGrid(float t)
        {
            if (_staticTex == null) return;
            int w = 160;
            int h = 90;
            Vector2 cL = new Vector2(w * 0.34f, h * 0.5f);
            Vector2 cR = new Vector2(w * 0.66f, h * 0.5f);
            float pulseR = 18f + Mathf.Sin(t * 14f) * 2f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool grid = (x % 16 == 0) || (y % 15 == 0);
                    float dL = Vector2.Distance(new Vector2(x, y), cL);
                    float dR = Vector2.Distance(new Vector2(x, y), cR);
                    bool ring = (Mathf.Abs(dL - pulseR) < 1.3f) || (Mathf.Abs(dR - pulseR) < 1.3f);

                    if (ring)
                        _staticPixels[y * w + x] = new Color32(30, 245, 255, 255);
                    else if (grid)
                        _staticPixels[y * w + x] = new Color32(8, 55, 68, 255);
                    else
                        _staticPixels[y * w + x] = new Color32(2, 8, 12, 255);
                }
            }

            _staticTex.SetPixels32(_staticPixels);
            _staticTex.Apply(false);
        }

        private void UpdateMonologue(float clock)
        {
            if (monologueText == null) return;

            int activeIdx = -1;
            for (int i = 0; i < MonologueLines.Length; i++)
            {
                if (clock >= MonologueLines[i].time)
                {
                    activeIdx = i;
                }
            }

            if (activeIdx < 0)
            {
                monologueText.text = "";
                if (monologuePanel != null) monologuePanel.SetActive(false);
                return;
            }

            if (activeIdx != _lastDialogueIndex)
            {
                _lastDialogueIndex = activeIdx;
            }

            string fullLine = MonologueLines[activeIdx].line;
            float lineElapsed = clock - MonologueLines[activeIdx].time;

            if (lineElapsed > 3.6f && activeIdx < MonologueLines.Length - 1)
            {
                monologueText.text = "";
                if (monologuePanel != null) monologuePanel.SetActive(false);
                return;
            }

            int visibleChars = Mathf.Clamp(Mathf.FloorToInt(lineElapsed * 34f), 0, fullLine.Length);
            monologueText.text = fullLine.Substring(0, visibleChars);
            if (monologuePanel != null) monologuePanel.SetActive(visibleChars > 0);
        }

        private void PlayBreathSound()
        {
            if (_breathAudio == null) return;

            AudioClip clip = _sequenceTriggered
                ? (panicBreathOverride != null ? panicBreathOverride : _generatedPanicBreath)
                : (calmBreathOverride != null ? calmBreathOverride : _generatedCalmBreath);

            if (clip != null)
            {
                _breathAudio.Stop();
                _breathAudio.volume = _sequenceTriggered ? 0.88f : 0.62f;
                _breathAudio.PlayOneShot(clip);
            }
        }

        private AudioClip GenerateCleanHumanBreathing(string clipName, float totalDuration, bool isPanic)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * totalDuration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(clipName.GetHashCode());
            BiquadBandpass nasalFilter = new BiquadBandpass();
            BiquadBandpass chestFilter = new BiquadBandpass();

            float inhaleStart = 0.04f;
            float inhaleEnd = totalDuration * (isPanic ? 0.34f : 0.28f);
            float exhaleStart = totalDuration * (isPanic ? 0.42f : 0.40f);
            float exhaleEnd = totalDuration * (isPanic ? 0.82f : 0.74f);

            float prev = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                float softNoise = 0.55f * prev + 0.45f * white;
                prev = softNoise;

                float outVal = 0f;

                if (t >= inhaleStart && t <= inhaleEnd)
                {
                    float u = (t - inhaleStart) / (inhaleEnd - inhaleStart);
                    float env = Mathf.Sin(u * Mathf.PI);
                    env *= env;

                    nasalFilter.SetParams(sampleRate, Mathf.Lerp(920f, 1180f, u), 6.5f);
                    chestFilter.SetParams(sampleRate, Mathf.Lerp(420f, 490f, u), 5.0f);

                    outVal = (nasalFilter.Process(softNoise) * 0.85f + chestFilter.Process(softNoise) * 0.45f)
                             * env * (isPanic ? 2.2f : 1.55f);
                }
                else if (t >= exhaleStart && t <= exhaleEnd)
                {
                    float u = (t - exhaleStart) / (exhaleEnd - exhaleStart);
                    float env = Mathf.Sin(u * Mathf.PI);
                    env *= env;

                    nasalFilter.SetParams(sampleRate, Mathf.Lerp(780f, 620f, u), 5.8f);
                    chestFilter.SetParams(sampleRate, Mathf.Lerp(380f, 300f, u), 4.5f);

                    outVal = (nasalFilter.Process(softNoise) * 0.55f + chestFilter.Process(softNoise) * 0.90f)
                             * env * (isPanic ? 2.4f : 1.75f);
                }

                samples[i] = Mathf.Clamp(outVal, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateHeadsetClampClip()
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * 0.40f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float v = 0f;

                if (t >= 0.01f && t <= 0.07f)
                {
                    float lt = (t - 0.01f) / 0.06f;
                    v += Mathf.Sin(2f * Mathf.PI * 310f * lt) * Mathf.Exp(-lt * 18f) * 0.70f;
                }
                if (t >= 0.13f && t <= 0.22f)
                {
                    float lt = (t - 0.13f) / 0.09f;
                    v += Mathf.Sin(2f * Mathf.PI * 190f * lt) * Mathf.Exp(-lt * 14f) * 0.75f;
                }
                samples[i] = Mathf.Clamp(v, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Headset_Clamp_SFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateStaticInterferenceClip()
        {
            int sampleRate = 44100;
            float duration = 2.0f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(909);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float crackle = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.28f;
                float hum = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.14f;
                float digitalChirp = ((int)(t * 14f) % 5 == 0)
                    ? Mathf.Sin(2f * Mathf.PI * 1480f * t) * 0.12f
                    : 0f;

                samples[i] = Mathf.Clamp(crackle + hum + digitalChirp, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Static_Interference_SFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private struct BiquadBandpass
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void SetParams(float sampleRate, float centerFreq, float q)
            {
                float omega = 2f * Mathf.PI * centerFreq / sampleRate;
                float sinW = Mathf.Sin(omega);
                float cosW = Mathf.Cos(omega);
                float alpha = sinW / (2f * q);

                float a0 = 1f + alpha;
                _b0 = alpha / a0;
                _b1 = 0f;
                _b2 = -alpha / a0;
                _a1 = (-2f * cosW) / a0;
                _a2 = (1f - alpha) / a0;
            }

            public float Process(float x)
            {
                float y = _b0 * x + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;
                _x2 = _x1;
                _x1 = x;
                _y2 = _y1;
                _y1 = y;
                return y;
            }
        }
    }
}