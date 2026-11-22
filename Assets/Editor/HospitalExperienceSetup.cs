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
        HospitalWardEntitiesBuilder.BuildWardEntities();
        ApplyLyingPose();
        FixPillBottle();

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
        FixFullscreenOverlays();
        TuneHandlerMaterials();
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
        // The rig is tilted 75 degrees (lying on the bed). The head sits 60 degrees back
        // against that tilt, so the default eye line rests ~15 degrees down towards the feet.
        look.basePitch = -60f;
        look.minPitch = -35f;
        look.maxPitch = 35f;
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

    private static void ApplyLyingPose()
    {
        GameObject origin = GameObject.Find("XR Origin (VR)");
        GameObject offset = GameObject.Find("Camera Offset");
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (origin == null || offset == null || mainCamera == null)
        {
            Debug.LogWarning("[HospitalExperienceSetup] Camera rig nodes were not found; lying pose was skipped.");
            return;
        }

        // The player is strapped to the bed: the rig sits on the mattress, head on the pillow,
        // and the view is tilted forward so the patient sees their own body and the ward beyond.
        origin.transform.position = new Vector3(-1.5f, 0.65f, -3.2f);
        origin.transform.rotation = Quaternion.Euler(75f, 0f, 0f);

        offset.transform.localPosition = Vector3.zero;
        offset.transform.localRotation = Quaternion.identity;

        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        Rigidbody[] bodies = origin.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            Object.DestroyImmediate(bodies[i]);
        }
    }

    private static void FixPillBottle()
    {
        GameObject bottle = GameObject.Find("Pill_Bottle");
        if (bottle == null) bottle = GameObject.Find("Capsule");
        if (bottle == null) return;

        Rigidbody body = bottle.GetComponent<Rigidbody>();
        if (body != null) Object.DestroyImmediate(body);

        bottle.name = "Pill_Bottle";
        bottle.transform.position = new Vector3(-0.54f, 0.655f, -4.15f);
        bottle.transform.rotation = Quaternion.identity;

        Rigidbody[] childBodies = bottle.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < childBodies.Length; i++)
        {
            Object.DestroyImmediate(childBodies[i]);
        }
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

    private static void FixFullscreenOverlays()
    {
        string[] names = { "Blackout_Screen", "Headset_Static_Glitch_Screen" };
        for (int i = 0; i < names.Length; i++)
        {
            GameObject target = GameObject.Find(names[i]);
            if (target == null) continue;

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null) continue;

            rect.anchorMin = new Vector2(-0.03f, -0.03f);
            rect.anchorMax = new Vector2(1.03f, 1.03f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private static void TuneHandlerMaterials()
    {
        SetBaseColor("Assets/Materials/Handler_Coat_Mat.mat", new Color(0.15f, 0.16f, 0.19f));
        SetBaseColor("Assets/Materials/Handler_Glove_Mat.mat", new Color(0.19f, 0.20f, 0.23f));
        SetBaseColor("Assets/Materials/Handler_Mask_Mat.mat", new Color(0.07f, 0.07f, 0.09f));
        SetBaseColor("Assets/Materials/Doctor_Coat_Mat.mat", new Color(0.28f, 0.30f, 0.33f));
        SetBaseColor("Assets/Materials/Doctor_Trim_Mat.mat", new Color(0.16f, 0.17f, 0.20f));
        SetBaseColor("Assets/Materials/Doctor_Glove_Mat.mat", new Color(0.34f, 0.36f, 0.40f));
    }

    private static void SetBaseColor(string path, Color color)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) return;

        material.SetColor("_BaseColor", color);
        material.SetColor("_Color", color);
        EditorUtility.SetDirty(material);
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
