using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class HospitalArchitectureBuilder : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/1. Build Room Architecture & Lighting")]
    public static void BuildArchitecture()
    {
        GameObject envRoot = GameObject.Find("Hospital_Environment");
        if (envRoot == null)
        {
            envRoot = new GameObject("Hospital_Environment");
        }

        for (int i = envRoot.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(envRoot.transform.GetChild(i).gameObject);
        }

        Material floorMat = HospitalMaterials.CreateTiledFloorMaterial();
        Material upperWallMat = HospitalMaterials.CreateMaterial("Hospital_UpperWall_Mat", new Color(0.84f, 0.87f, 0.86f), 0.15f, 0.0f);
        Material wainscotMat = HospitalMaterials.CreateMaterial("Hospital_Wainscot_Mat", new Color(0.28f, 0.45f, 0.44f), 0.25f, 0.0f);
        Material moldingMat = HospitalMaterials.CreateMaterial("Hospital_Molding_Mat", new Color(0.68f, 0.72f, 0.71f), 0.3f, 0.05f);
        Material baseboardMat = HospitalMaterials.CreateMaterial("Hospital_Baseboard_Mat", new Color(0.18f, 0.22f, 0.24f), 0.35f, 0.05f);
        Material ceilingMat = HospitalMaterials.CreateMaterial("Hospital_Ceiling_Mat", new Color(0.85f, 0.87f, 0.88f), 0.1f, 0.0f);
        Material doorMat = HospitalMaterials.CreateMaterial("Hospital_Door_Mat", new Color(0.62f, 0.66f, 0.68f), 0.35f, 0.1f);
        Material chromeMat = HospitalMaterials.CreateMaterial("Hospital_Chrome_Mat", new Color(0.82f, 0.84f, 0.87f), 0.92f, 0.95f);
        Material glassMat = HospitalMaterials.CreateMaterial("Hospital_WindowGlass_Mat", new Color(0.65f, 0.80f, 0.88f), 0.92f, 0.1f);
        Material windowGlowMat = HospitalMaterials.CreateEmissiveMaterial("Hospital_FrostedWindow_Mat", new Color(0.75f, 0.86f, 0.92f), new Color(0.65f, 0.82f, 0.95f), 0.80f);
        Material blindSlatMat = HospitalMaterials.CreateMaterial("Hospital_BlindSlat_Mat", new Color(0.86f, 0.88f, 0.90f), 0.45f, 0.2f);
        Material fixtureHousingMat = HospitalMaterials.CreateMaterial("Light_Housing_Mat", new Color(0.75f, 0.77f, 0.79f), 0.5f, 0.4f);
        Material tubeGlowMat = HospitalMaterials.CreateEmissiveMaterial("Fluorescent_Tube_Mat", Color.white, new Color(0.92f, 0.96f, 1.0f), 2.8f);
        Material ventMat = HospitalMaterials.CreateVentLouversMaterial();
        Material oxyBlueMat = HospitalMaterials.CreateEmissiveMaterial("MedGas_Oxy_Mat", new Color(0.1f, 0.65f, 1.0f), new Color(0.1f, 0.65f, 1.0f), 1.8f);
        Material vacAmberMat = HospitalMaterials.CreateEmissiveMaterial("MedGas_Vac_Mat", new Color(1.0f, 0.65f, 0.1f), new Color(1.0f, 0.65f, 0.1f), 1.8f);

        // 1. Tiled Floor (10m x 10m)
        GameObject floor = CreatePrimitiveChild(envRoot.transform, "Plane", PrimitiveType.Plane, Vector3.zero, Vector3.zero, Vector3.one);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // 2. Outer Structural Walls & Ceiling
        float roomSize = 10f;
        float wallHeight = 3.6f;
        float half = roomSize * 0.5f;

        CreateWall(envRoot.transform, "Wall_Back",  new Vector3(0f, wallHeight * 0.5f,  half), new Vector3(roomSize, wallHeight, 0.2f), upperWallMat);
        CreateWall(envRoot.transform, "Wall_Front", new Vector3(0f, wallHeight * 0.5f, -half), new Vector3(roomSize, wallHeight, 0.2f), upperWallMat);
        CreateWall(envRoot.transform, "Wall_Left",  new Vector3(-half, wallHeight * 0.5f, 0f), new Vector3(0.2f, wallHeight, roomSize), upperWallMat);
        CreateWall(envRoot.transform, "Wall_Right", new Vector3( half, wallHeight * 0.5f, 0f), new Vector3(0.2f, wallHeight, roomSize), upperWallMat);

        GameObject ceiling = CreatePrimitiveChild(envRoot.transform, "Ceiling", PrimitiveType.Cube, new Vector3(0f, wallHeight + 0.1f, 0f), Vector3.zero, new Vector3(roomSize + 0.4f, 0.2f, roomSize + 0.4f));
        ceiling.GetComponent<Renderer>().sharedMaterial = ceilingMat;

        // 3. Lower Teal Wainscoting Panels, Moldings, Crown Trim, Baseboards & Pilasters
        float wainscotH = 1.25f;
        CreateWall(envRoot.transform, "Wall_Panel_Back",  new Vector3(0f, wainscotH * 0.5f,  half - 0.11f), new Vector3(roomSize - 0.2f, wainscotH, 0.03f), wainscotMat);
        CreateWall(envRoot.transform, "Wall_Panel_Front", new Vector3(0f, wainscotH * 0.5f, -half + 0.11f), new Vector3(roomSize - 0.2f, wainscotH, 0.03f), wainscotMat);
        CreateWall(envRoot.transform, "Wall_Panel_Left",  new Vector3(-half + 0.11f, wainscotH * 0.5f, 0f), new Vector3(0.03f, wainscotH, roomSize - 0.2f), wainscotMat);
        CreateWall(envRoot.transform, "Wall_Panel_Right", new Vector3( half - 0.11f, wainscotH * 0.5f, 0f), new Vector3(0.03f, wainscotH, roomSize - 0.2f), wainscotMat);

        CreateWall(envRoot.transform, "Wall_Molding_Back",  new Vector3(0f, wainscotH + 0.03f,  half - 0.12f), new Vector3(roomSize - 0.2f, 0.06f, 0.05f), moldingMat);
        CreateWall(envRoot.transform, "Wall_Molding_Front", new Vector3(0f, wainscotH + 0.03f, -half + 0.12f), new Vector3(roomSize - 0.2f, 0.06f, 0.05f), moldingMat);
        CreateWall(envRoot.transform, "Wall_Molding_Left",  new Vector3(-half + 0.12f, wainscotH + 0.03f, 0f), new Vector3(0.05f, 0.06f, roomSize - 0.2f), moldingMat);
        CreateWall(envRoot.transform, "Wall_Molding_Right", new Vector3( half - 0.12f, wainscotH + 0.03f, 0f), new Vector3(0.05f, 0.06f, roomSize - 0.2f), moldingMat);

        // Upper ceiling crown molding
        CreateWall(envRoot.transform, "Crown_Molding_Back",  new Vector3(0f, wallHeight - 0.05f,  half - 0.12f), new Vector3(roomSize - 0.2f, 0.08f, 0.06f), moldingMat);
        CreateWall(envRoot.transform, "Crown_Molding_Front", new Vector3(0f, wallHeight - 0.05f, -half + 0.12f), new Vector3(roomSize - 0.2f, 0.08f, 0.06f), moldingMat);
        CreateWall(envRoot.transform, "Crown_Molding_Left",  new Vector3(-half + 0.12f, wallHeight - 0.05f, 0f), new Vector3(0.06f, 0.08f, roomSize - 0.2f), moldingMat);
        CreateWall(envRoot.transform, "Crown_Molding_Right", new Vector3( half - 0.12f, wallHeight - 0.05f, 0f), new Vector3(0.06f, 0.08f, roomSize - 0.2f), moldingMat);

        // Dark rubber baseboards
        CreateWall(envRoot.transform, "Baseboard_Back",  new Vector3(0f, 0.06f,  half - 0.13f), new Vector3(roomSize - 0.2f, 0.12f, 0.04f), baseboardMat);
        CreateWall(envRoot.transform, "Baseboard_Front", new Vector3(0f, 0.06f, -half + 0.13f), new Vector3(roomSize - 0.2f, 0.12f, 0.04f), baseboardMat);
        CreateWall(envRoot.transform, "Baseboard_Left",  new Vector3(-half + 0.13f, 0.06f, 0f), new Vector3(0.04f, 0.12f, roomSize - 0.2f), baseboardMat);
        CreateWall(envRoot.transform, "Baseboard_Right", new Vector3( half - 0.13f, 0.06f, 0f), new Vector3(0.04f, 0.12f, roomSize - 0.2f), baseboardMat);

        // Structural pilasters
        CreateWall(envRoot.transform, "Pilaster_BL", new Vector3(-half + 0.16f, wallHeight * 0.5f,  half - 0.16f), new Vector3(0.24f, wallHeight, 0.24f), upperWallMat);
        CreateWall(envRoot.transform, "Pilaster_BR", new Vector3( half - 0.16f, wallHeight * 0.5f,  half - 0.16f), new Vector3(0.24f, wallHeight, 0.24f), upperWallMat);
        CreateWall(envRoot.transform, "Pilaster_Mid_Back", new Vector3(-1.35f, wallHeight * 0.5f, half - 0.14f), new Vector3(0.22f, wallHeight, 0.12f), upperWallMat);

        // 4. Frosted Observation Window with 14 Venetian Blind Slats on Left Wall
        BuildObservationWindowWithBlinds(envRoot.transform, new Vector3(-half + 0.13f, 2.08f, 1.35f), fixtureHousingMat, windowGlowMat, blindSlatMat);

        // 5. Medical Gas & Power Headwall Console behind bed
        BuildMedicalGasConsole(envRoot.transform, new Vector3(-1.50f, 1.52f, -half + 0.14f), fixtureHousingMat, chromeMat, oxyBlueMat, vacAmberMat);

        // 6. Ward Door on Back Wall
        BuildWardDoor(envRoot.transform, new Vector3(1.85f, 0f, half - 0.11f), doorMat, moldingMat, chromeMat, glassMat);

        // 7. Ceiling Air Vents & 4 Fluorescent Light Fixtures (1 shadow-casting light prevents 2048 atlas warning)
        CreateWall(envRoot.transform, "Ceiling_Vent_1", new Vector3(-2.4f, wallHeight - 0.005f, -0.4f), new Vector3(0.60f, 0.02f, 0.60f), ventMat);
        CreateWall(envRoot.transform, "Ceiling_Vent_2", new Vector3( 2.4f, wallHeight - 0.005f,  0.6f), new Vector3(0.60f, 0.02f, 0.60f), ventMat);

        Vector3[] lightPositions = new Vector3[]
        {
            new Vector3(-1.9f, wallHeight - 0.06f, -2.0f),
            new Vector3( 1.9f, wallHeight - 0.06f, -2.0f),
            new Vector3(-1.9f, wallHeight - 0.06f,  2.0f),
            new Vector3( 1.9f, wallHeight - 0.06f,  2.0f)
        };

        for (int i = 0; i < lightPositions.Length; i++)
        {
            bool castShadows = (i == 0);
            BuildCeilingLightFixture(envRoot.transform, $"Ceiling_Light_Fixture_{i + 1}", lightPositions[i], fixtureHousingMat, tubeGlowMat, castShadows);
        }

        // 8. Tune Scene Directional Light & Ambient Fog/Environment
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i].type == LightType.Directional)
            {
                allLights[i].intensity = 0.12f;
                allLights[i].color = new Color(0.78f, 0.86f, 0.96f);
                allLights[i].shadows = LightShadows.None;
            }
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.20f, 0.24f, 0.27f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.13f, 0.17f, 0.19f);
        RenderSettings.fogDensity = 0.022f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[HospitalArchitectureBuilder] Architecture, Venetian blinds window, medical gas console, and balanced lighting built.");
    }

    private static void BuildObservationWindowWithBlinds(Transform parent, Vector3 pos, Material frameMat, Material glassMat, Material slatMat)
    {
        GameObject win = new GameObject("Clinic_Observation_Window");
        win.transform.SetParent(parent);
        win.transform.localPosition = pos;

        float w = 2.20f;
        float h = 1.22f;

        CreatePrimitiveChild(win.transform, "Window_FrostedGlass", PrimitiveType.Cube, Vector3.zero, Vector3.zero, new Vector3(0.02f, h, w)).GetComponent<Renderer>().sharedMaterial = glassMat;
        CreatePrimitiveChild(win.transform, "Win_Frame_Top", PrimitiveType.Cube, new Vector3(0.03f, h * 0.5f, 0f), Vector3.zero, new Vector3(0.08f, 0.06f, w + 0.10f)).GetComponent<Renderer>().sharedMaterial = frameMat;
        CreatePrimitiveChild(win.transform, "Win_Frame_Bot", PrimitiveType.Cube, new Vector3(0.04f, -h * 0.5f, 0f), Vector3.zero, new Vector3(0.11f, 0.07f, w + 0.14f)).GetComponent<Renderer>().sharedMaterial = frameMat;
        CreatePrimitiveChild(win.transform, "Win_Frame_L",   PrimitiveType.Cube, new Vector3(0.03f, 0f, -w * 0.5f), Vector3.zero, new Vector3(0.08f, h, 0.06f)).GetComponent<Renderer>().sharedMaterial = frameMat;
        CreatePrimitiveChild(win.transform, "Win_Frame_R",   PrimitiveType.Cube, new Vector3(0.03f, 0f,  w * 0.5f), Vector3.zero, new Vector3(0.08f, h, 0.06f)).GetComponent<Renderer>().sharedMaterial = frameMat;
        CreatePrimitiveChild(win.transform, "Win_Mullion_Mid", PrimitiveType.Cube, new Vector3(0.03f, 0f, 0f), Vector3.zero, new Vector3(0.06f, h, 0.05f)).GetComponent<Renderer>().sharedMaterial = frameMat;

        // 14 tilted Venetian blind slats
        int slatCount = 14;
        for (int i = 0; i < slatCount; i++)
        {
            float t = (float)i / (slatCount - 1);
            float sy = Mathf.Lerp(h * 0.43f, -h * 0.43f, t);
            GameObject slat = CreatePrimitiveChild(win.transform, $"Blind_Slat_{i + 1}", PrimitiveType.Cube, new Vector3(0.042f, sy, 0f), new Vector3(0f, 0f, 26f), new Vector3(0.035f, 0.008f, w - 0.08f));
            slat.GetComponent<Renderer>().sharedMaterial = slatMat;
        }
    }

    private static void BuildMedicalGasConsole(Transform parent, Vector3 pos, Material bodyMat, Material chromeMat, Material oxyMat, Material vacMat)
    {
        GameObject console = new GameObject("Medical_Gas_Wall_Console");
        console.transform.SetParent(parent);
        console.transform.localPosition = pos;

        CreatePrimitiveChild(console.transform, "Console_Rail", PrimitiveType.Cube, Vector3.zero, Vector3.zero, new Vector3(1.45f, 0.18f, 0.06f)).GetComponent<Renderer>().sharedMaterial = bodyMat;
        CreatePrimitiveChild(console.transform, "Oxy_Port", PrimitiveType.Cylinder, new Vector3(-0.35f, 0f, 0.04f), new Vector3(90f, 0f, 0f), new Vector3(0.05f, 0.02f, 0.05f)).GetComponent<Renderer>().sharedMaterial = oxyMat;
        CreatePrimitiveChild(console.transform, "Vac_Port", PrimitiveType.Cylinder, new Vector3(-0.15f, 0f, 0.04f), new Vector3(90f, 0f, 0f), new Vector3(0.05f, 0.02f, 0.05f)).GetComponent<Renderer>().sharedMaterial = vacMat;
        CreatePrimitiveChild(console.transform, "Power_Outlet_1", PrimitiveType.Cube, new Vector3(0.22f, 0f, 0.035f), Vector3.zero, new Vector3(0.11f, 0.09f, 0.02f)).GetComponent<Renderer>().sharedMaterial = chromeMat;
        CreatePrimitiveChild(console.transform, "Power_Outlet_2", PrimitiveType.Cube, new Vector3(0.42f, 0f, 0.035f), Vector3.zero, new Vector3(0.11f, 0.09f, 0.02f)).GetComponent<Renderer>().sharedMaterial = chromeMat;
    }

    private static void BuildWardDoor(Transform parent, Vector3 pos, Material doorMat, Material frameMat, Material chromeMat, Material glassMat)
    {
        GameObject doorGroup = new GameObject("Hospital_Ward_Door");
        doorGroup.transform.SetParent(parent);
        doorGroup.transform.localPosition = pos;

        float doorW = 1.15f;
        float doorH = 2.25f;

        GameObject leaf = CreatePrimitiveChild(doorGroup.transform, "Door_Leaf", PrimitiveType.Cube, new Vector3(0f, doorH * 0.5f, -0.02f), Vector3.zero, new Vector3(doorW, doorH, 0.05f));
        leaf.GetComponent<Renderer>().sharedMaterial = doorMat;

        CreatePrimitiveChild(doorGroup.transform, "Door_Frame_Left",  PrimitiveType.Cube, new Vector3(-doorW * 0.5f - 0.04f, doorH * 0.5f, -0.03f), Vector3.zero, new Vector3(0.08f, doorH + 0.08f, 0.08f)).GetComponent<Renderer>().sharedMaterial = frameMat;
        CreatePrimitiveChild(doorGroup.transform, "Door_Frame_Right", PrimitiveType.Cube, new Vector3( doorW * 0.5f + 0.04f, doorH * 0.5f, -0.03f), Vector3.zero, new Vector3(0.08f, doorH + 0.08f, 0.08f)).GetComponent<Renderer>().sharedMaterial = frameMat;
        CreatePrimitiveChild(doorGroup.transform, "Door_Frame_Top",   PrimitiveType.Cube, new Vector3(0f, doorH + 0.04f, -0.03f), Vector3.zero, new Vector3(doorW + 0.16f, 0.08f, 0.08f)).GetComponent<Renderer>().sharedMaterial = frameMat;

        GameObject win = CreatePrimitiveChild(doorGroup.transform, "Door_Window", PrimitiveType.Cube, new Vector3(0.18f, 1.55f, -0.025f), Vector3.zero, new Vector3(0.26f, 0.55f, 0.06f));
        win.GetComponent<Renderer>().sharedMaterial = glassMat;

        CreatePrimitiveChild(doorGroup.transform, "Door_Handle_Plate", PrimitiveType.Cube, new Vector3(-0.42f, 1.02f, -0.055f), Vector3.zero, new Vector3(0.05f, 0.22f, 0.015f)).GetComponent<Renderer>().sharedMaterial = chromeMat;
        CreatePrimitiveChild(doorGroup.transform, "Door_Handle_Lever", PrimitiveType.Cube, new Vector3(-0.36f, 1.02f, -0.085f), Vector3.zero, new Vector3(0.14f, 0.025f, 0.025f)).GetComponent<Renderer>().sharedMaterial = chromeMat;
    }

    private static void BuildCeilingLightFixture(Transform parent, string name, Vector3 pos, Material housingMat, Material tubeMat, bool castShadows)
    {
        GameObject fixture = new GameObject(name);
        fixture.transform.SetParent(parent);
        fixture.transform.localPosition = pos;

        CreatePrimitiveChild(fixture.transform, "Fixture_Housing", PrimitiveType.Cube, Vector3.zero, Vector3.zero, new Vector3(0.36f, 0.07f, 1.35f)).GetComponent<Renderer>().sharedMaterial = housingMat;
        CreatePrimitiveChild(fixture.transform, "Fluorescent_Tube_Left", PrimitiveType.Cylinder, new Vector3(-0.08f, -0.04f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.035f, 0.62f, 0.035f)).GetComponent<Renderer>().sharedMaterial = tubeMat;
        CreatePrimitiveChild(fixture.transform, "Fluorescent_Tube_Right", PrimitiveType.Cylinder, new Vector3( 0.08f, -0.04f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.035f, 0.62f, 0.035f)).GetComponent<Renderer>().sharedMaterial = tubeMat;

        GameObject lightObj = new GameObject("Fixture_PointLight");
        lightObj.transform.SetParent(fixture.transform);
        lightObj.transform.localPosition = new Vector3(0f, -0.22f, 0f);
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(0.88f, 0.94f, 1.0f);
        pointLight.intensity = 2.65f;
        pointLight.range = 8.8f;
        pointLight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
        pointLight.shadowStrength = 0.68f;
        pointLight.shadowBias = 0.03f;
    }

    private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject wall = CreatePrimitiveChild(parent, name, PrimitiveType.Cube, pos, Vector3.zero, scale);
        wall.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static GameObject CreatePrimitiveChild(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(localEuler);
        obj.transform.localScale = localScale;
        return obj;
    }
}