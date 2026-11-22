using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class HospitalPropsAndTerminalBuilder : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/3. Build Cabinet, Nightstand, Terminal & Camera")]
    public static void BuildPropsAndTerminal()
    {
        Material woodMat = HospitalMaterials.CreateMaterial("Bed_Wood_Mat", new Color(0.76f, 0.73f, 0.68f), 0.32f, 0.0f);
        Material darkFrameWoodMat = HospitalMaterials.CreateMaterial("Frame_DarkWood_Mat", new Color(0.24f, 0.18f, 0.14f), 0.45f, 0.05f);
        Material cabinetSteelMat = HospitalMaterials.CreateMaterial("Cabinet_Steel_Mat", new Color(0.78f, 0.81f, 0.83f), 0.45f, 0.35f);
        Material chromeMat = HospitalMaterials.CreateMaterial("Hospital_Chrome_Mat", new Color(0.82f, 0.84f, 0.87f), 0.92f, 0.95f);
        Material darkPlasticMat = HospitalMaterials.CreateMaterial("Terminal_Body_Mat", new Color(0.14f, 0.16f, 0.18f), 0.35f, 0.1f);
        Material rubberMat = HospitalMaterials.CreateMaterial("Wheel_Rubber_Mat", new Color(0.12f, 0.13f, 0.14f), 0.25f, 0.0f);
        Material chairFabricMat = HospitalMaterials.CreateMaterial("Chair_Fabric_Mat", new Color(0.24f, 0.36f, 0.44f), 0.18f, 0.0f);
        Material amberBottleMat = HospitalMaterials.CreateMaterial("Pill_Bottle_Amber_Mat", new Color(0.78f, 0.38f, 0.08f), 0.75f, 0.0f);
        Material whiteCapMat = HospitalMaterials.CreateMaterial("Pill_Cap_White_Mat", new Color(0.95f, 0.95f, 0.96f), 0.3f, 0.0f);
        Material ecgScreenMat = HospitalMaterials.CreateECGScreenMaterial();
        Material powerLedMat = HospitalMaterials.CreateEmissiveMaterial("Terminal_LED_Mat", Color.green, new Color(0.1f, 1.0f, 0.4f), 3.0f);

        Material painting1Mat = HospitalMaterials.CreatePaintingMaterial("Hospital_Painting_Landscape", 1);
        Material painting2Mat = HospitalMaterials.CreatePaintingMaterial("Hospital_Painting_CircusHorizon", 2);
        Material cert1Mat = HospitalMaterials.CreateCertificateMaterial("Medical_Certificate_A", 1);
        Material cert2Mat = HospitalMaterials.CreateCertificateMaterial("Medical_Certificate_B", 2);
        Material clockMat = HospitalMaterials.CreateClockFaceMaterial();

        BuildDetailedNightstand(new Vector3(-0.54f, 0f, -4.15f), woodMat, chromeMat);
        SetupPillBottle(new Vector3(-0.54f, 0.655f, -4.15f), amberBottleMat, whiteCapMat);
        BuildDetailedMedicalCabinet(new Vector3(-4.52f, 0f, -1.85f), cabinetSteelMat, chromeMat);
        BuildMedicalTerminal(new Vector3(-1.08f, 0f, -2.05f), darkPlasticMat, chromeMat, rubberMat, ecgScreenMat, powerLedMat);

        BuildPatientWardrobe(new Vector3(-2.35f, 0f, 4.52f), woodMat, darkFrameWoodMat, chromeMat);
        BuildFramedPainting("Hospital_Painting_Front", new Vector3(-0.35f, 1.88f, 4.86f), new Vector3(0f, 180f, 0f), new Vector2(1.15f, 0.86f), darkFrameWoodMat, painting1Mat);
        BuildFramedPainting("Hospital_Painting_Right", new Vector3(4.86f, 1.88f, -1.65f), new Vector3(0f, -90f, 0f), new Vector2(1.10f, 0.82f), darkFrameWoodMat, painting2Mat);

        BuildFramedCertificate("Medical_Certificate_1", new Vector3(4.86f, 1.88f, -0.25f), new Vector3(0f, -90f, 0f), darkFrameWoodMat, cert1Mat);
        BuildFramedCertificate("Medical_Certificate_2", new Vector3(4.86f, 1.88f,  0.42f), new Vector3(0f, -90f, 0f), darkFrameWoodMat, cert2Mat);
        BuildFramedCertificate("Medical_Certificate_3", new Vector3(3.15f, 1.85f,  4.86f), new Vector3(0f, 180f, 0f), darkFrameWoodMat, cert1Mat);

        BuildWallClock(new Vector3(0.68f, 2.52f, 4.86f), chromeMat, clockMat);
        BuildVisitorChair(new Vector3(0.55f, 0f, -2.90f), chromeMat, chairFabricMat);

        SetupPatientCamera();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[HospitalPropsAndTerminalBuilder] Complete furniture, artwork, certificates, clock, and cinematic camera configured.");
    }

    [MenuItem("Tools/Hospital Ward/Build All (Complete Ward)")]
    public static void BuildCompleteWard()
    {
        HospitalArchitectureBuilder.BuildArchitecture();
        HospitalBedAndIVBuilder.BuildBedAndIV();
        BuildPropsAndTerminal();
        Debug.Log("[Hospital Ward] Complete hospital ward generated successfully!");
    }

    private static void BuildPatientWardrobe(Vector3 worldPos, Material bodyWoodMat, Material trimWoodMat, Material chromeMat)
    {
        GameObject ward = GameObject.Find("Patient_Wardrobe");
        if (ward != null) DestroyImmediate(ward);

        ward = new GameObject("Patient_Wardrobe");
        ward.transform.position = worldPos;
        ward.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        BuildPart(ward.transform, "Wardrobe_Plinth", PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(1.24f, 0.10f, 0.56f), trimWoodMat);
        BuildPart(ward.transform, "Wardrobe_MainBody", PrimitiveType.Cube, new Vector3(0f, 1.12f, 0f), Vector3.zero, new Vector3(1.28f, 2.04f, 0.58f), bodyWoodMat);
        BuildPart(ward.transform, "Wardrobe_TopCornice", PrimitiveType.Cube, new Vector3(0f, 2.17f, 0f), Vector3.zero, new Vector3(1.34f, 0.06f, 0.62f), trimWoodMat);

        BuildPart(ward.transform, "Wardrobe_Door_L", PrimitiveType.Cube, new Vector3(-0.31f, 1.12f, 0.295f), Vector3.zero, new Vector3(0.59f, 1.94f, 0.025f), bodyWoodMat);
        BuildPart(ward.transform, "Wardrobe_Door_R", PrimitiveType.Cube, new Vector3( 0.31f, 1.12f, 0.295f), Vector3.zero, new Vector3(0.59f, 1.94f, 0.025f), bodyWoodMat);

        BuildPart(ward.transform, "Wardrobe_Handle_L", PrimitiveType.Cube, new Vector3(-0.06f, 1.14f, 0.322f), Vector3.zero, new Vector3(0.022f, 0.28f, 0.03f), chromeMat);
        BuildPart(ward.transform, "Wardrobe_Handle_R", PrimitiveType.Cube, new Vector3( 0.06f, 1.14f, 0.322f), Vector3.zero, new Vector3(0.022f, 0.28f, 0.03f), chromeMat);
        BuildPart(ward.transform, "Wardrobe_Vent_L", PrimitiveType.Cube, new Vector3(-0.31f, 1.95f, 0.31f), Vector3.zero, new Vector3(0.32f, 0.06f, 0.01f), trimWoodMat);
        BuildPart(ward.transform, "Wardrobe_Vent_R", PrimitiveType.Cube, new Vector3( 0.31f, 1.95f, 0.31f), Vector3.zero, new Vector3(0.32f, 0.06f, 0.01f), trimWoodMat);
    }

    private static void BuildFramedPainting(string name, Vector3 worldPos, Vector3 eulerRot, Vector2 size, Material frameMat, Material canvasMat)
    {
        GameObject painting = GameObject.Find(name);
        if (painting != null) DestroyImmediate(painting);

        painting = new GameObject(name);
        painting.transform.position = worldPos;
        painting.transform.rotation = Quaternion.Euler(eulerRot);

        float w = size.x;
        float h = size.y;
        float border = 0.045f;

        BuildPart(painting.transform, "Canvas", PrimitiveType.Cube, new Vector3(0f, 0f, 0.01f), Vector3.zero, new Vector3(w, h, 0.015f), canvasMat);
        BuildPart(painting.transform, "Frame_Top", PrimitiveType.Cube, new Vector3(0f, h * 0.5f + border * 0.4f, 0.018f), Vector3.zero, new Vector3(w + border * 2f, border, 0.035f), frameMat);
        BuildPart(painting.transform, "Frame_Bottom", PrimitiveType.Cube, new Vector3(0f, -h * 0.5f - border * 0.4f, 0.018f), Vector3.zero, new Vector3(w + border * 2f, border, 0.035f), frameMat);
        BuildPart(painting.transform, "Frame_Left", PrimitiveType.Cube, new Vector3(-w * 0.5f - border * 0.4f, 0f, 0.018f), Vector3.zero, new Vector3(border, h, 0.035f), frameMat);
        BuildPart(painting.transform, "Frame_Right", PrimitiveType.Cube, new Vector3(w * 0.5f + border * 0.4f, 0f, 0.018f), Vector3.zero, new Vector3(border, h, 0.035f), frameMat);
    }

    private static void BuildFramedCertificate(string name, Vector3 worldPos, Vector3 eulerRot, Material frameMat, Material certMat)
    {
        GameObject cert = GameObject.Find(name);
        if (cert != null) DestroyImmediate(cert);

        cert = new GameObject(name);
        cert.transform.position = worldPos;
        cert.transform.rotation = Quaternion.Euler(eulerRot);

        float w = 0.42f;
        float h = 0.56f;
        float border = 0.025f;

        BuildPart(cert.transform, "Certificate_Sheet", PrimitiveType.Cube, new Vector3(0f, 0f, 0.008f), Vector3.zero, new Vector3(w, h, 0.01f), certMat);
        BuildPart(cert.transform, "Frame_Top", PrimitiveType.Cube, new Vector3(0f, h * 0.5f + border * 0.4f, 0.014f), Vector3.zero, new Vector3(w + border * 2f, border, 0.024f), frameMat);
        BuildPart(cert.transform, "Frame_Bottom", PrimitiveType.Cube, new Vector3(0f, -h * 0.5f - border * 0.4f, 0.014f), Vector3.zero, new Vector3(w + border * 2f, border, 0.024f), frameMat);
        BuildPart(cert.transform, "Frame_Left", PrimitiveType.Cube, new Vector3(-w * 0.5f - border * 0.4f, 0f, 0.014f), Vector3.zero, new Vector3(border, h, 0.024f), frameMat);
        BuildPart(cert.transform, "Frame_Right", PrimitiveType.Cube, new Vector3(w * 0.5f + border * 0.4f, 0f, 0.014f), Vector3.zero, new Vector3(border, h, 0.024f), frameMat);
    }

    private static void BuildWallClock(Vector3 worldPos, Material chromeMat, Material clockFaceMat)
    {
        GameObject clock = GameObject.Find("Hospital_Wall_Clock");
        if (clock != null) DestroyImmediate(clock);

        clock = new GameObject("Hospital_Wall_Clock");
        clock.transform.position = worldPos;
        clock.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        BuildPart(clock.transform, "Clock_Bezel", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.02f), new Vector3(90f, 0f, 0f), new Vector3(0.42f, 0.02f, 0.42f), chromeMat);
        BuildPart(clock.transform, "Clock_Dial", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.032f), new Vector3(90f, 0f, 0f), new Vector3(0.38f, 0.012f, 0.38f), clockFaceMat);
    }

    private static void BuildVisitorChair(Vector3 worldPos, Material chromeMat, Material fabricMat)
    {
        GameObject chair = GameObject.Find("Hospital_Visitor_Chair");
        if (chair != null) DestroyImmediate(chair);

        chair = new GameObject("Hospital_Visitor_Chair");
        chair.transform.position = worldPos;
        chair.transform.rotation = Quaternion.Euler(0f, -55f, 0f);

        BuildPart(chair.transform, "Seat_Cushion", PrimitiveType.Cube, new Vector3(0f, 0.46f, 0f), Vector3.zero, new Vector3(0.46f, 0.06f, 0.44f), fabricMat);
        BuildPart(chair.transform, "Backrest_Cushion", PrimitiveType.Cube, new Vector3(0f, 0.74f, -0.20f), new Vector3(-8f, 0f, 0f), new Vector3(0.46f, 0.32f, 0.05f), fabricMat);

        BuildPart(chair.transform, "Leg_FL", PrimitiveType.Cylinder, new Vector3(-0.20f, 0.23f,  0.19f), Vector3.zero, new Vector3(0.024f, 0.23f, 0.024f), chromeMat);
        BuildPart(chair.transform, "Leg_FR", PrimitiveType.Cylinder, new Vector3( 0.20f, 0.23f,  0.19f), Vector3.zero, new Vector3(0.024f, 0.23f, 0.024f), chromeMat);
        BuildPart(chair.transform, "Leg_BL", PrimitiveType.Cylinder, new Vector3(-0.20f, 0.42f, -0.19f), Vector3.zero, new Vector3(0.024f, 0.42f, 0.024f), chromeMat);
        BuildPart(chair.transform, "Leg_BR", PrimitiveType.Cylinder, new Vector3( 0.20f, 0.42f, -0.19f), Vector3.zero, new Vector3(0.024f, 0.42f, 0.024f), chromeMat);
    }

    private static void BuildDetailedNightstand(Vector3 worldPos, Material woodMat, Material chromeMat)
    {
        GameObject ns = GameObject.Find("Nightstand");
        if (ns != null) DestroyImmediate(ns);

        ns = new GameObject("Nightstand");
        ns.transform.position = worldPos;
        ns.transform.rotation = Quaternion.identity;

        BuildPart(ns.transform, "NS_Plinth", PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(0.44f, 0.06f, 0.44f), woodMat);
        BuildPart(ns.transform, "NS_Body", PrimitiveType.Cube, new Vector3(0f, 0.34f, 0f), Vector3.zero, new Vector3(0.48f, 0.56f, 0.48f), woodMat);
        BuildPart(ns.transform, "NS_TopCounter", PrimitiveType.Cube, new Vector3(0f, 0.635f, 0f), Vector3.zero, new Vector3(0.51f, 0.03f, 0.51f), woodMat);

        BuildPart(ns.transform, "NS_Drawer_Front", PrimitiveType.Cube, new Vector3(0f, 0.51f, 0.245f), Vector3.zero, new Vector3(0.43f, 0.14f, 0.02f), woodMat);
        BuildPart(ns.transform, "NS_Drawer_Handle", PrimitiveType.Cube, new Vector3(0f, 0.51f, 0.268f), Vector3.zero, new Vector3(0.12f, 0.018f, 0.025f), chromeMat);

        BuildPart(ns.transform, "NS_Door_Front", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0.245f), Vector3.zero, new Vector3(0.43f, 0.34f, 0.02f), woodMat);
        BuildPart(ns.transform, "NS_Door_Handle", PrimitiveType.Cube, new Vector3(-0.15f, 0.32f, 0.268f), Vector3.zero, new Vector3(0.018f, 0.11f, 0.025f), chromeMat);
    }

    private static void SetupPillBottle(Vector3 worldPos, Material bottleMat, Material capMat)
    {
        GameObject bottle = GameObject.Find("Capsule");
        if (bottle == null) bottle = new GameObject("Capsule");

        for (int i = bottle.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(bottle.transform.GetChild(i).gameObject);
        }

        MeshRenderer rootMr = bottle.GetComponent<MeshRenderer>();
        if (rootMr != null) DestroyImmediate(rootMr);
        MeshFilter rootMf = bottle.GetComponent<MeshFilter>();
        if (rootMf != null) DestroyImmediate(rootMf);

        bottle.transform.SetParent(null);
        bottle.transform.position = worldPos;
        bottle.transform.rotation = Quaternion.identity;
        bottle.transform.localScale = Vector3.one;

        BuildPart(bottle.transform, "Bottom", PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0f), Vector3.zero, new Vector3(0.055f, 0.045f, 0.055f), bottleMat);
        BuildPart(bottle.transform, "Label_Band", PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0f), Vector3.zero, new Vector3(0.057f, 0.024f, 0.057f), capMat);
        BuildPart(bottle.transform, "Cap", PrimitiveType.Cylinder, new Vector3(0f, 0.10f, 0f), Vector3.zero, new Vector3(0.062f, 0.014f, 0.062f), capMat);
    }

    private static void BuildDetailedMedicalCabinet(Vector3 worldPos, Material steelMat, Material chromeMat)
    {
        GameObject cab = GameObject.Find("Med_Cabinet");
        if (cab != null) DestroyImmediate(cab);

        cab = new GameObject("Med_Cabinet");
        cab.transform.position = worldPos;
        cab.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        cab.transform.localScale = Vector3.one;

        BuildPart(cab.transform, "Cabinet_Plinth", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(1.06f, 0.08f, 0.48f), steelMat);
        BuildPart(cab.transform, "Cabinet_Body", PrimitiveType.Cube, new Vector3(0f, 1.04f, 0f), Vector3.zero, new Vector3(1.10f, 1.92f, 0.50f), steelMat);
        BuildPart(cab.transform, "Cabinet_TopCornice", PrimitiveType.Cube, new Vector3(0f, 2.02f, 0f), Vector3.zero, new Vector3(1.14f, 0.05f, 0.54f), steelMat);

        BuildPart(cab.transform, "Cabinet_Door_Left", PrimitiveType.Cube, new Vector3(-0.265f, 1.04f, 0.255f), Vector3.zero, new Vector3(0.51f, 1.82f, 0.02f), steelMat);
        BuildPart(cab.transform, "Cabinet_Door_Right", PrimitiveType.Cube, new Vector3(0.265f, 1.04f, 0.255f), Vector3.zero, new Vector3(0.51f, 1.82f, 0.02f), steelMat);

        BuildPart(cab.transform, "Cabinet_Handle_Left", PrimitiveType.Cube, new Vector3(-0.055f, 1.06f, 0.28f), Vector3.zero, new Vector3(0.02f, 0.24f, 0.03f), chromeMat);
        BuildPart(cab.transform, "Cabinet_Handle_Right", PrimitiveType.Cube, new Vector3(0.055f, 1.06f, 0.28f), Vector3.zero, new Vector3(0.02f, 0.24f, 0.03f), chromeMat);
        BuildPart(cab.transform, "Cabinet_KeyLock", PrimitiveType.Cylinder, new Vector3(0.055f, 1.24f, 0.268f), new Vector3(90f, 0f, 0f), new Vector3(0.025f, 0.01f, 0.025f), chromeMat);
    }

    private static void BuildMedicalTerminal(Vector3 worldPos, Material bodyMat, Material chromeMat, Material rubberMat, Material screenMat, Material ledMat)
    {
        GameObject term = GameObject.Find("Med_Terminal");
        if (term != null) DestroyImmediate(term);

        term = new GameObject("Med_Terminal");
        term.transform.position = worldPos;
        term.transform.rotation = Quaternion.Euler(0f, -18f, 0f);
        term.transform.localScale = Vector3.one;

        BuildPart(term.transform, "Terminal_Base", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(0.54f, 0.05f, 0.54f), chromeMat);
        Vector3[] wheelPos = new Vector3[]
        {
            new Vector3(-0.22f, 0.025f, -0.22f),
            new Vector3( 0.22f, 0.025f, -0.22f),
            new Vector3(-0.22f, 0.025f,  0.22f),
            new Vector3( 0.22f, 0.025f,  0.22f)
        };
        for (int i = 0; i < wheelPos.Length; i++)
        {
            BuildPart(term.transform, $"Terminal_Wheel_{i + 1}", PrimitiveType.Sphere, wheelPos[i], Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), rubberMat);
        }

        BuildPart(term.transform, "Terminal_Column", PrimitiveType.Cylinder, new Vector3(0f, 0.58f, 0f), Vector3.zero, new Vector3(0.065f, 0.54f, 0.065f), chromeMat);
        BuildPart(term.transform, "Terminal_AccessoryTray", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.06f), Vector3.zero, new Vector3(0.36f, 0.025f, 0.22f), bodyMat);

        GameObject monitorPivot = new GameObject("Terminal_Monitor_Head");
        monitorPivot.transform.SetParent(term.transform);
        monitorPivot.transform.localPosition = new Vector3(0f, 1.24f, 0f);
        monitorPivot.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);

        BuildPart(monitorPivot.transform, "Monitor_Casing", PrimitiveType.Cube, Vector3.zero, Vector3.zero, new Vector3(0.60f, 0.44f, 0.13f), bodyMat);
        BuildPart(monitorPivot.transform, "Monitor_Screen", PrimitiveType.Cube, new Vector3(0f, 0.01f, -0.067f), Vector3.zero, new Vector3(0.52f, 0.35f, 0.01f), screenMat);
        BuildPart(monitorPivot.transform, "Monitor_PowerLED", PrimitiveType.Sphere, new Vector3(0.24f, -0.19f, -0.068f), Vector3.zero, new Vector3(0.016f, 0.016f, 0.016f), ledMat);

        AudioSource audio = term.AddComponent<AudioSource>();
        audio.spatialBlend = 0.35f;
        audio.loop = true;
        audio.volume = 0.6f;
    }

    private static void SetupPatientCamera()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin (VR)");
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        GameObject mainCamera = GameObject.Find("Main Camera");

        if (xrOrigin != null)
        {
            // Shifted left and slightly higher for a wider diagonal view of the bed, hands, terminal, and '?' handler
            xrOrigin.transform.position = new Vector3(-1.82f, 1.12f, -4.32f);
            xrOrigin.transform.rotation = Quaternion.Euler(13f, 11f, 0f);
        }
        if (cameraOffset != null)
        {
            cameraOffset.transform.localPosition = Vector3.zero;
            cameraOffset.transform.localRotation = Quaternion.identity;
        }
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
    }

    private static GameObject BuildPart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(localEuler);
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}