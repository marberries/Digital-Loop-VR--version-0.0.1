using System.Text;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class HospitalExperienceSetup
{
    private const string VolumeProfilePath = "Assets/Settings/HospitalPostFX.asset";

    [MenuItem("Tools/Hospital Ward/6. Build Playable Experience (Look, Unease, Audio)")]
    public static void BuildPlayableExperience()
    {
        HospitalAnimationSetup.AttachAnimations();

        GameObject mainCamera = GameObject.Find("Main Camera");
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        if (mainCamera == null || cameraOffset == null)
        {
            Debug.LogError("[HospitalExperienceSetup] Main Camera / Camera Offset were not found in the open scene.");
            return;
        }

        PatientBreathingAndIntro intro = mainCamera.GetComponent<PatientBreathingAndIntro>();
        if (intro == null)
        {
            intro = mainCamera.AddComponent<PatientBreathingAndIntro>();
        }

        intro.uneaseGrowthRate = 0.12f;
        intro.uneaseDecayRate = 0.05f;
        intro.gazeHoldThreshold = 1.5f;
        intro.gazeMaxAngle = 32f;
        intro.uneaseThreshold = 1.0f;

        if (intro.wardEntities == null || intro.wardEntities.Length == 0)
        {
            intro.wardEntities = CollectWardEntities();
        }

        SetupLookController(cameraOffset.transform, intro);

        VolumeProfile profile = EnsureVolumeProfileAsset();
        Vignette vignette = null;
        ChromaticAberration chromatic = null;
        if (profile != null)
        {
            profile.TryGet(out vignette);
            profile.TryGet(out chromatic);
        }

        SetupUneaseFeedback(intro, vignette, chromatic);
        SetupDirector(intro);
        EnsureSceneInBuildSettings(EditorSceneManager.GetActiveScene().path);

        if (Object.FindAnyObjectByType<AudioListener>() == null)
        {
            mainCamera.AddComponent<AudioListener>();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[HospitalExperienceSetup] Playable experience ready.\n" + DescribeWiring(intro) + "\nScene: " + EditorSceneManager.GetActiveScene().path);
    }

    private static Transform[] CollectWardEntities()
    {
        WardEntityIdle[] idles = Object.FindObjectsByType<WardEntityIdle>(FindObjectsInactive.Exclude);
        Transform[] result = new Transform[idles.Length];
        for (int i = 0; i < idles.Length; i++)
        {
            result[i] = idles[i].transform;
        }
        return result;
    }

    private static void SetupLookController(Transform cameraOffset, PatientBreathingAndIntro intro)
    {
        PatientLookController look = cameraOffset.GetComponent<PatientLookController>();
        if (look == null) look = cameraOffset.gameObject.AddComponent<PatientLookController>();

        look.sensitivity = 2.2f;
        look.minPitch = -72f;
        look.maxPitch = 66f;
        look.lockCursor = true;
        look.intro = intro;
        look.enabled = true;
    }

    private static VolumeProfile EnsureVolumeProfileAsset()
    {
        Volume volume = Object.FindAnyObjectByType<Volume>();
        if (volume == null)
        {
            Debug.LogWarning("[HospitalExperienceSetup] No URP Volume found; post processing feedback will be skipped.");
            return null;
        }

        VolumeProfile profile = volume.sharedProfile != null ? volume.sharedProfile : volume.profile;
        if (profile == null) return null;

        if (!AssetDatabase.Contains(profile))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            AssetDatabase.DeleteAsset(VolumeProfilePath);
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            AssetDatabase.SaveAssets();
        }

        volume.sharedProfile = profile;
        volume.profile = profile;
        EditorUtility.SetDirty(volume);

        return profile;
    }

    private static void SetupUneaseFeedback(PatientBreathingAndIntro intro, Vignette vignette, ChromaticAberration chromatic)
    {
        GameObject feedbackObject = GameObject.Find("Unease_Feedback");
        if (feedbackObject == null)
        {
            feedbackObject = new GameObject("Unease_Feedback");
        }

        UneaseFeedback feedback = feedbackObject.GetComponent<UneaseFeedback>();
        if (feedback == null) feedback = feedbackObject.AddComponent<UneaseFeedback>();

        AudioSource[] sources = feedbackObject.GetComponents<AudioSource>();
        AudioSource drone = sources.Length > 0 ? sources[0] : feedbackObject.AddComponent<AudioSource>();
        AudioSource heartbeat = sources.Length > 1 ? sources[1] : feedbackObject.AddComponent<AudioSource>();

        drone.playOnAwake = false;
        drone.loop = true;
        drone.spatialBlend = 0f;
        drone.volume = 0f;

        heartbeat.playOnAwake = false;
        heartbeat.loop = false;
        heartbeat.spatialBlend = 0f;

        feedback.intro = intro;
        feedback.vignette = vignette;
        feedback.chromaticAberration = chromatic;
        feedback.droneSource = drone;
        feedback.heartbeatSource = heartbeat;
    }

    private static void SetupDirector(PatientBreathingAndIntro intro)
    {
        Text ending = null;
        GameObject canvas = GameObject.Find("Visor_And_Monologue_Canvas");
        if (canvas != null)
        {
            Transform existing = canvas.transform.Find("Ending_Card_Text");
            GameObject textObject = existing != null ? existing.gameObject : new GameObject("Ending_Card_Text");
            if (existing == null) textObject.transform.SetParent(canvas.transform, false);

            ending = textObject.GetComponent<Text>();
            if (ending == null) ending = textObject.AddComponent<Text>();

            ending.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ending.fontSize = 30;
            ending.fontStyle = FontStyle.Normal;
            ending.alignment = TextAnchor.MiddleCenter;
            ending.color = new Color(0.88f, 0.93f, 1f, 0f);
            ending.text = "";
            ending.raycastTarget = false;

            RectTransform rect = ending.rectTransform;
            rect.anchorMin = new Vector2(0.1f, 0.30f);
            rect.anchorMax = new Vector2(0.9f, 0.56f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        GameObject directorObject = GameObject.Find("Experience_Director");
        if (directorObject == null) directorObject = new GameObject("Experience_Director");

        ExperienceDirector director = directorObject.GetComponent<ExperienceDirector>();
        if (director == null) director = directorObject.AddComponent<ExperienceDirector>();

        director.intro = intro;
        director.endingText = ending;
    }

    private static void EnsureSceneInBuildSettings(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath)) return;

        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == scenePath) return;
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[HospitalExperienceSetup] Added " + scenePath + " to Build Settings so the session can restart itself.");
    }

    private static string DescribeWiring(PatientBreathingAndIntro intro)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("handlerRoot=").Append(Assigned(intro.handlerRoot));
        sb.Append(" torso=").Append(Assigned(intro.handlerTorso));
        sb.Append(" arms=").Append(Assigned(intro.handlerLeftArm) && Assigned(intro.handlerRightArm));
        sb.Append(" headset=").Append(Assigned(intro.headsetAssembly));
        sb.Append(" playerArms=").Append(Assigned(intro.playerLeftArm) && Assigned(intro.playerRightArm));
        sb.Append(" blackout=").Append(intro.blackoutImage != null);
        sb.Append(" static=").Append(intro.staticGlitchImage != null);
        sb.Append(" monologue=").Append(intro.monologueText != null);
        sb.Append(" ecg=").Append(intro.ecgMonitor != null);
        sb.Append(" chromatic=").Append(intro.postChromaticAberration != null);
        sb.Append(" wardEntities=").Append(intro.wardEntities != null ? intro.wardEntities.Length : 0);
        return sb.ToString();
    }

    private static bool Assigned(Object value)
    {
        return value != null;
    }
}
