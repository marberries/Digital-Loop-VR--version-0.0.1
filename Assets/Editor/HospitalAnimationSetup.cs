using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitalCurtain.Hospital;

public class HospitalAnimationSetup : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/4. Attach Animations & Live Effects")]
    public static void AttachAnimations()
    {
        int attachedCount = 0;

        GameObject terminal = GameObject.Find("Med_Terminal");
        if (terminal != null)
        {
            ECGMonitorController ecg = terminal.GetComponent<ECGMonitorController>();
            if (ecg == null) ecg = terminal.AddComponent<ECGMonitorController>();

            Transform headTr = terminal.transform.Find("Terminal_Monitor_Head");
            if (headTr != null)
            {
                Transform screenTr = headTr.Find("Monitor_Screen");
                Transform ledTr = headTr.Find("Monitor_PowerLED");

                if (screenTr != null) ecg.screenRenderer = screenTr.GetComponent<Renderer>();
                if (ledTr != null) ecg.ledRenderer = ledTr.GetComponent<Renderer>();

                Transform oldCursor = headTr.Find("ECG_Sweep_Cursor");
                if (oldCursor != null) DestroyImmediate(oldCursor.gameObject);

                GameObject cursor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cursor.name = "ECG_Sweep_Cursor";
                cursor.transform.SetParent(headTr);
                cursor.transform.localPosition = new Vector3(-0.23f, 0.01f, -0.075f);
                cursor.transform.localRotation = Quaternion.identity;
                cursor.transform.localScale = new Vector3(0.012f, 0.042f, 0.005f);
                DestroyImmediate(cursor.GetComponent<Collider>());

                Material cursorMat = HospitalMaterials.CreateEmissiveMaterial(
                    "ECG_Cursor_Mat",
                    new Color(0.2f, 1.0f, 0.5f),
                    new Color(0.2f, 1.0f, 0.5f),
                    3.5f
                );
                cursor.GetComponent<Renderer>().sharedMaterial = cursorMat;
                ecg.sweepCursor = cursor.transform;
            }
            attachedCount++;
        }

        GameObject fixture = GameObject.Find("Ceiling_Light_Fixture_3");
        if (fixture != null)
        {
            FluorescentFlicker flicker = fixture.GetComponent<FluorescentFlicker>();
            if (flicker == null) flicker = fixture.AddComponent<FluorescentFlicker>();

            flicker.fixtureLight = fixture.GetComponentInChildren<Light>();
            Transform tubeL = fixture.transform.Find("Fluorescent_Tube_Left");
            Transform tubeR = fixture.transform.Find("Fluorescent_Tube_Right");
            if (tubeL != null && tubeR != null)
            {
                flicker.tubeRenderers = new Renderer[]
                {
                    tubeL.GetComponent<Renderer>(),
                    tubeR.GetComponent<Renderer>()
                };
            }
            attachedCount++;
        }

        GameObject cameraOffset = GameObject.Find("Camera Offset");
        if (cameraOffset != null)
        {
            PatientBreathingAndIntro oldIntro = cameraOffset.GetComponent<PatientBreathingAndIntro>();
            if (oldIntro != null) DestroyImmediate(oldIntro);
        }

        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            if (cam != null) cam.nearClipPlane = 0.04f;

            UniversalAdditionalCameraData camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null) camData.renderPostProcessing = true;

            PatientBreathingAndIntro intro = mainCamera.GetComponent<PatientBreathingAndIntro>();
            if (intro == null) intro = mainCamera.AddComponent<PatientBreathingAndIntro>();

            if (terminal != null)
            {
                intro.ecgMonitor = terminal.GetComponent<ECGMonitorController>();
            }

            CleanupChild(mainCamera.transform, "Neural_Headset_Visor");
            CleanupChild(mainCamera.transform, "Neural_Headset_Assembly");
            CleanupChild(mainCamera.transform, "Blackout_Overlay");
            CleanupChild(mainCamera.transform, "Patient_FirstPerson_Arms");

            BuildDetailedNeuralHeadset(intro);
            BuildMysteriousQuestionMarkHandler(intro);
            BuildPlayerFirstPersonArms(mainCamera.transform, intro);
            BuildVisorAndMonologueUI(intro);
            SetupPostProcessingVolume(intro);

            attachedCount++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[HospitalAnimationSetup] Configured {attachedCount}/3 systems: '?' Handler, Neural Headset, Player Arms, Monologue UI & URP Post-Processing.");
    }

    private static void BuildDetailedNeuralHeadset(PatientBreathingAndIntro intro)
    {
        GameObject existing = GameObject.Find("Neural_Headset_Assembly");
        if (existing != null) DestroyImmediate(existing);

        Material shellMat = HospitalMaterials.CreateMaterial("Headset_Shell_Mat", new Color(0.11f, 0.12f, 0.14f), 0.75f, 0.50f);
        Material trimSilverMat = HospitalMaterials.CreateMaterial("Headset_Silver_Mat", new Color(0.74f, 0.77f, 0.81f), 0.90f, 0.92f);
        Material foamMat = HospitalMaterials.CreateMaterial("Headset_Foam_Mat", new Color(0.05f, 0.05f, 0.06f), 0.12f, 0.0f);
        Material frontGlassMat = HospitalMaterials.CreateMaterial("Headset_FrontGlass_Mat", new Color(0.03f, 0.04f, 0.06f), 0.96f, 0.85f);
        Material cyanOpticMat = HospitalMaterials.CreateEmissiveMaterial("Headset_CyanOptic_Mat", new Color(0.1f, 0.95f, 1.0f), new Color(0.1f, 0.95f, 1.0f), 3.6f);
        Material strapMat = HospitalMaterials.CreateMaterial("Headset_Strap_Mat", new Color(0.18f, 0.19f, 0.21f), 0.22f, 0.0f);

        GameObject headset = new GameObject("Neural_Headset_Assembly");
        intro.headsetAssembly = headset.transform;

        BuildPart(headset.transform, "Visor_Front_Body", PrimitiveType.Cube, new Vector3(0f, 0.002f, 0.025f), Vector3.zero, new Vector3(0.21f, 0.098f, 0.058f), shellMat);
        BuildPart(headset.transform, "Visor_Brow_Shield", PrimitiveType.Cube, new Vector3(0f, 0.058f, 0.01f), new Vector3(8f, 0f, 0f), new Vector3(0.225f, 0.028f, 0.09f), shellMat);
        BuildPart(headset.transform, "Visor_Cheek_L", PrimitiveType.Cube, new Vector3(-0.066f, -0.046f, 0.015f), new Vector3(0f, 0f, -10f), new Vector3(0.078f, 0.025f, 0.078f), shellMat);
        BuildPart(headset.transform, "Visor_Cheek_R", PrimitiveType.Cube, new Vector3( 0.066f, -0.046f, 0.015f), new Vector3(0f, 0f,  10f), new Vector3(0.078f, 0.025f, 0.078f), shellMat);

        BuildPart(headset.transform, "Front_Glass_Plate", PrimitiveType.Cube, new Vector3(0f, 0.004f, 0.056f), Vector3.zero, new Vector3(0.195f, 0.084f, 0.006f), frontGlassMat);
        BuildPart(headset.transform, "Front_Neural_Optic_Strip", PrimitiveType.Cube, new Vector3(0f, 0.025f, 0.060f), Vector3.zero, new Vector3(0.16f, 0.007f, 0.004f), cyanOpticMat);
        BuildPart(headset.transform, "Tracking_Cam_L", PrimitiveType.Cylinder, new Vector3(-0.085f, 0.034f, 0.058f), new Vector3(90f, 0f, 0f), new Vector3(0.022f, 0.008f, 0.022f), trimSilverMat);
        BuildPart(headset.transform, "Tracking_Cam_R", PrimitiveType.Cylinder, new Vector3( 0.085f, 0.034f, 0.058f), new Vector3(90f, 0f, 0f), new Vector3(0.022f, 0.008f, 0.022f), trimSilverMat);

        BuildPart(headset.transform, "Facial_Foam_Ring", PrimitiveType.Cube, new Vector3(0f, 0.002f, -0.018f), Vector3.zero, new Vector3(0.19f, 0.09f, 0.035f), foamMat);
        BuildPart(headset.transform, "Temple_Arm_L", PrimitiveType.Cube, new Vector3(-0.110f, 0.008f, -0.065f), Vector3.zero, new Vector3(0.015f, 0.030f, 0.15f), shellMat);
        BuildPart(headset.transform, "Temple_Arm_R", PrimitiveType.Cube, new Vector3( 0.110f, 0.008f, -0.065f), Vector3.zero, new Vector3(0.015f, 0.030f, 0.15f), shellMat);
        BuildPart(headset.transform, "Side_Status_LED_L", PrimitiveType.Sphere, new Vector3(-0.118f, 0.008f, 0.015f), Vector3.zero, new Vector3(0.011f, 0.011f, 0.011f), cyanOpticMat);
        BuildPart(headset.transform, "Side_Status_LED_R", PrimitiveType.Sphere, new Vector3( 0.118f, 0.008f, 0.015f), Vector3.zero, new Vector3(0.011f, 0.011f, 0.011f), cyanOpticMat);
        BuildPart(headset.transform, "Top_Head_Strap", PrimitiveType.Cube, new Vector3(0f, 0.072f, -0.055f), new Vector3(-12f, 0f, 0f), new Vector3(0.040f, 0.010f, 0.15f), strapMat);
    }

    private static void BuildMysteriousQuestionMarkHandler(PatientBreathingAndIntro intro)
    {
        GameObject existing = GameObject.Find("Mysterious_Handler");
        if (existing != null) DestroyImmediate(existing);

        Material coatMat = HospitalMaterials.CreateMaterial("Handler_Coat_Mat", new Color(0.08f, 0.09f, 0.11f), 0.25f, 0.05f);
        Material gloveMat = HospitalMaterials.CreateMaterial("Handler_Glove_Mat", new Color(0.14f, 0.15f, 0.17f), 0.45f, 0.1f);
        Material maskMat = HospitalMaterials.CreateMaterial("Handler_Mask_Mat", new Color(0.03f, 0.03f, 0.04f), 0.85f, 0.6f);
        Material qMarkGlowMat = HospitalMaterials.CreateEmissiveMaterial(
            "Handler_QuestionMark_Mat",
            new Color(0.95f, 0.96f, 1.0f),
            new Color(0.85f, 0.95f, 1.0f),
            3.8f
        );

        GameObject root = new GameObject("Mysterious_Handler");
        root.transform.position = new Vector3(-2.32f, 0f, -3.72f);
        root.transform.rotation = Quaternion.Euler(0f, 122f, 0f);
        intro.handlerRoot = root.transform;

        BuildPart(root.transform, "Handler_LowerCoat", PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), Vector3.zero, new Vector3(0.38f, 0.55f, 0.32f), coatMat);

        GameObject torso = new GameObject("Handler_Torso");
        torso.transform.SetParent(root.transform);
        torso.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        torso.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
        intro.handlerTorso = torso.transform;

        BuildPart(torso.transform, "Torso_Chest", PrimitiveType.Capsule, new Vector3(0f, 0.28f, 0f), Vector3.zero, new Vector3(0.42f, 0.34f, 0.26f), coatMat);
        BuildPart(torso.transform, "Torso_Collar", PrimitiveType.Cylinder, new Vector3(0f, 0.56f, 0f), Vector3.zero, new Vector3(0.22f, 0.06f, 0.22f), coatMat);

        GameObject head = new GameObject("Handler_Head");
        head.transform.SetParent(torso.transform);
        head.transform.localPosition = new Vector3(0f, 0.74f, 0.02f);
        head.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);

        BuildPart(head.transform, "Head_Mask_Shell", PrimitiveType.Sphere, Vector3.zero, Vector3.zero, new Vector3(0.23f, 0.28f, 0.25f), maskMat);

        BuildPart(head.transform, "QMark_Hook_Left",   PrimitiveType.Sphere, new Vector3(-0.036f,  0.048f, 0.118f), Vector3.zero, new Vector3(0.024f, 0.028f, 0.018f), qMarkGlowMat);
        BuildPart(head.transform, "QMark_Hook_Top",    PrimitiveType.Sphere, new Vector3( 0.000f,  0.068f, 0.123f), Vector3.zero, new Vector3(0.048f, 0.022f, 0.018f), qMarkGlowMat);
        BuildPart(head.transform, "QMark_Hook_Right",  PrimitiveType.Sphere, new Vector3( 0.036f,  0.046f, 0.118f), Vector3.zero, new Vector3(0.024f, 0.032f, 0.018f), qMarkGlowMat);
        BuildPart(head.transform, "QMark_Curve_Mid",   PrimitiveType.Cube,   new Vector3( 0.016f,  0.016f, 0.122f), new Vector3(0f, 0f, 42f), new Vector3(0.020f, 0.046f, 0.016f), qMarkGlowMat);
        BuildPart(head.transform, "QMark_Stem_Vert",   PrimitiveType.Cube,   new Vector3( 0.000f, -0.014f, 0.124f), Vector3.zero, new Vector3(0.020f, 0.034f, 0.016f), qMarkGlowMat);
        BuildPart(head.transform, "QMark_Bottom_Dot",  PrimitiveType.Sphere, new Vector3( 0.000f, -0.054f, 0.123f), Vector3.zero, new Vector3(0.024f, 0.024f, 0.018f), qMarkGlowMat);

        GameObject armL = new GameObject("Handler_Arm_L");
        armL.transform.SetParent(torso.transform);
        armL.transform.localPosition = new Vector3(-0.23f, 0.48f, 0.02f);
        armL.transform.localRotation = Quaternion.Euler(-42f, 14f, -10f);
        intro.handlerLeftArm = armL.transform;
        BuildPart(armL.transform, "UpperArm_L", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0f), Vector3.zero, new Vector3(0.11f, 0.18f, 0.11f), coatMat);
        BuildPart(armL.transform, "Forearm_L",  PrimitiveType.Capsule, new Vector3(0.04f, -0.34f, 0.12f), new Vector3(45f, 0f, -12f), new Vector3(0.095f, 0.17f, 0.095f), coatMat);
        BuildPart(armL.transform, "Hand_L",     PrimitiveType.Sphere,  new Vector3(0.08f, -0.45f, 0.24f), Vector3.zero, new Vector3(0.085f, 0.065f, 0.10f), gloveMat);

        GameObject armR = new GameObject("Handler_Arm_R");
        armR.transform.SetParent(torso.transform);
        armR.transform.localPosition = new Vector3(0.23f, 0.48f, 0.02f);
        armR.transform.localRotation = Quaternion.Euler(-42f, -14f, 10f);
        intro.handlerRightArm = armR.transform;
        BuildPart(armR.transform, "UpperArm_R", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0f), Vector3.zero, new Vector3(0.11f, 0.18f, 0.11f), coatMat);
        BuildPart(armR.transform, "Forearm_R",  PrimitiveType.Capsule, new Vector3(-0.04f, -0.34f, 0.12f), new Vector3(45f, 0f, 12f), new Vector3(0.095f, 0.17f, 0.095f), coatMat);
        BuildPart(armR.transform, "Hand_R",     PrimitiveType.Sphere,  new Vector3(-0.08f, -0.45f, 0.24f), Vector3.zero, new Vector3(0.085f, 0.065f, 0.10f), gloveMat);
    }

    private static void BuildPlayerFirstPersonArms(Transform cameraParent, PatientBreathingAndIntro intro)
    {
        Material gownMat = HospitalMaterials.CreateMaterial("Patient_Gown_Mat", new Color(0.58f, 0.70f, 0.74f), 0.15f, 0.0f);
        Material skinMat = HospitalMaterials.CreateMaterial("Patient_Skin_Mat", new Color(0.84f, 0.70f, 0.60f), 0.22f, 0.0f);
        Material bandMat = HospitalMaterials.CreateMaterial("Patient_IDBand_Mat", new Color(0.92f, 0.93f, 0.95f), 0.35f, 0.0f);

        GameObject armsRoot = new GameObject("Patient_FirstPerson_Arms");
        armsRoot.transform.SetParent(cameraParent);
        armsRoot.transform.localPosition = Vector3.zero;
        armsRoot.transform.localRotation = Quaternion.identity;

        intro.playerLeftArm = BuildSinglePlayerArm(armsRoot.transform, "Player_Left_Arm", new Vector3(-0.26f, -0.34f, 0.34f), new Vector3(14f, 18f, -8f), true, gownMat, skinMat, bandMat);
        intro.playerRightArm = BuildSinglePlayerArm(armsRoot.transform, "Player_Right_Arm", new Vector3(0.26f, -0.34f, 0.34f), new Vector3(14f, -18f, 8f), false, gownMat, skinMat, bandMat);
    }

    private static Transform BuildSinglePlayerArm(Transform parent, string name, Vector3 pos, Vector3 euler, bool isLeft, Material gownMat, Material skinMat, Material bandMat)
    {
        GameObject arm = new GameObject(name);
        arm.transform.SetParent(parent);
        arm.transform.localPosition = pos;
        arm.transform.localRotation = Quaternion.Euler(euler);

        BuildPart(arm.transform, "Gown_Sleeve", PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.06f), new Vector3(90f, 0f, 0f), new Vector3(0.088f, 0.14f, 0.088f), gownMat);
        BuildPart(arm.transform, "Forearm",     PrimitiveType.Capsule,  new Vector3(0f, 0f,  0.12f), new Vector3(90f, 0f, 0f), new Vector3(0.068f, 0.13f, 0.062f), skinMat);

        if (isLeft)
        {
            BuildPart(arm.transform, "Hospital_Wristband", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.21f), new Vector3(90f, 0f, 0f), new Vector3(0.072f, 0.012f, 0.072f), bandMat);
        }

        BuildPart(arm.transform, "Palm", PrimitiveType.Cube, new Vector3(0f, -0.005f, 0.27f), new Vector3(8f, 0f, 0f), new Vector3(0.066f, 0.024f, 0.078f), skinMat);

        for (int f = 0; f < 4; f++)
        {
            float fx = -0.024f + f * 0.016f;
            BuildPart(arm.transform, $"Finger_{f + 1}", PrimitiveType.Capsule, new Vector3(fx, -0.014f, 0.325f), new Vector3(72f, 0f, 0f), new Vector3(0.013f, 0.026f, 0.013f), skinMat);
        }

        float thumbX = isLeft ? 0.038f : -0.038f;
        BuildPart(arm.transform, "Thumb", PrimitiveType.Capsule, new Vector3(thumbX, -0.010f, 0.28f), new Vector3(68f, isLeft ? 25f : -25f, 0f), new Vector3(0.015f, 0.024f, 0.015f), skinMat);

        return arm.transform;
    }

    private static void BuildVisorAndMonologueUI(PatientBreathingAndIntro intro)
    {
        GameObject oldCanvas = GameObject.Find("Visor_And_Monologue_Canvas");
        if (oldCanvas != null) DestroyImmediate(oldCanvas);

        GameObject canvasObj = new GameObject("Visor_And_Monologue_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject blackoutObj = new GameObject("Blackout_Screen");
        blackoutObj.transform.SetParent(canvasObj.transform, false);
        Image blackoutImg = blackoutObj.AddComponent<Image>();
        blackoutImg.color = new Color(0f, 0f, 0f, 0f);
        StretchFullScreen(blackoutImg.rectTransform);
        intro.blackoutImage = blackoutImg;

        GameObject staticObj = new GameObject("Headset_Static_Glitch_Screen");
        staticObj.transform.SetParent(canvasObj.transform, false);
        RawImage staticImg = staticObj.AddComponent<RawImage>();
        staticImg.color = new Color(1f, 1f, 1f, 0f);
        StretchFullScreen(staticImg.rectTransform);
        intro.staticGlitchImage = staticImg;

        GameObject panelObj = new GameObject("Monologue_Subtitle_Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.03f, 0.04f, 0.06f, 0.65f);
        RectTransform panelRt = panelImg.rectTransform;
        panelRt.anchorMin = new Vector2(0.14f, 0.06f);
        panelRt.anchorMax = new Vector2(0.86f, 0.16f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        panelObj.SetActive(false);
        intro.monologuePanel = panelObj;

        GameObject textObj = new GameObject("Patient_Monologue_Text");
        textObj.transform.SetParent(panelObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        Font builtInFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtInFont == null) builtInFont = Font.CreateDynamicFontFromOSFont("Helvetica Neue", 32);
        txt.font = builtInFont;
        txt.fontSize = 32;
        txt.fontStyle = FontStyle.Italic;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.93f, 0.96f, 0.98f, 0.96f);
        txt.text = "";

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.92f);
        shadow.effectDistance = new Vector2(2f, -2f);

        StretchFullScreen(txt.rectTransform);
        intro.monologueText = txt;
    }

    private static void SetupPostProcessingVolume(PatientBreathingAndIntro intro)
    {
        GameObject existing = GameObject.Find("Hospital_PostProcessing_Volume");
        if (existing != null) DestroyImmediate(existing);

        GameObject volObj = new GameObject("Hospital_PostProcessing_Volume");
        Volume volume = volObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.42f);
        bloom.threshold.Override(0.92f);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.38f);
        vignette.smoothness.Override(0.45f);

        ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.contrast.Override(14f);
        colorAdj.saturation.Override(-10f);
        colorAdj.colorFilter.Override(new Color(0.92f, 0.97f, 1.0f));

        ChromaticAberration chrom = profile.Add<ChromaticAberration>(true);
        chrom.intensity.Override(0.06f);
        if (intro != null)
        {
            intro.postChromaticAberration = chrom;
        }
    }

    private static void StretchFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CleanupChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) DestroyImmediate(child.gameObject);
    }

    private static GameObject BuildPart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(localEuler);
        obj.transform.localScale = localScale;
        Collider col = obj.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}