using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HospitalWardEntitiesBuilder
{
    [MenuItem("Tools/Hospital Ward/5. Build Ward Entities (Detailed Doctors)")]
    public static void BuildWardEntities()
    {
        Material coatMat = HospitalMaterials.CreateMaterial("Doctor_Coat_Mat", new Color(0.16f, 0.18f, 0.20f), 0.22f, 0.05f);
        Material trimMat = HospitalMaterials.CreateMaterial("Doctor_Trim_Mat", new Color(0.10f, 0.11f, 0.13f), 0.18f, 0.0f);
        Material gloveMat = HospitalMaterials.CreateMaterial("Doctor_Glove_Mat", new Color(0.22f, 0.24f, 0.27f), 0.35f, 0.05f);
        Material maskMat = HospitalMaterials.CreateMaterial("Doctor_Mask_Mat", new Color(0.70f, 0.72f, 0.75f), 0.10f, 0.0f);
        Material qMarkGlowMat = HospitalMaterials.CreateEmissiveMaterial(
            "Doctor_QuestionMark_Mat",
            new Color(0.95f, 0.97f, 1.0f),
            new Color(0.88f, 0.96f, 1.0f),
            4.2f
        );

        PatientBreathingAndIntro intro = Object.FindAnyObjectByType<PatientBreathingAndIntro>(FindObjectsInactive.Include);

        GameObject previousRoot = GameObject.Find("Ward_Entities");
        if (previousRoot != null)
        {
            Object.DestroyImmediate(previousRoot);
        }

        GameObject previousHandler = GameObject.Find("Mysterious_Handler");
        if (previousHandler != null)
        {
            Object.DestroyImmediate(previousHandler);
        }

        GameObject root = new GameObject("Ward_Entities");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        Vector3[] slots =
        {
            new Vector3(-3.55f, 0f, -2.35f),
            new Vector3( 0.30f, 0f, -1.62f),
            new Vector3( 3.25f, 0f,  0.85f),
            new Vector3(-0.55f, 0f,  3.25f)
        };

        string[] names =
        {
            "Ward_Entity_Bedside_Left",
            "Ward_Entity_Footboard",
            "Ward_Entity_Right_Wall",
            "Ward_Entity_Back_Wall"
        };

        Transform[] figures = new Transform[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            Vector3 spot = FindFreeSpot(slots[i]);
            Vector3 facing = new Vector3(-1.50f, 0f, -3.80f) - spot;
            float heightScale = 0.97f + (i % 3) * 0.03f;

            BuildEntityFigure(
                root.transform, names[i], spot, facing, heightScale,
                coatMat, trimMat, gloveMat, maskMat, qMarkGlowMat,
                false, intro);

            figures[i] = root.transform.Find(names[i]);
        }

        BuildEntityFigure(
            root.transform,
            "Mysterious_Handler",
            new Vector3(-3.35f, 0f, -1.05f),
            new Vector3(-1.50f, 0f, -3.55f) - new Vector3(-3.35f, 0f, -1.05f),
            1.04f,
            coatMat, trimMat, gloveMat, maskMat, qMarkGlowMat,
            true, intro);

        WireToIntro(intro, figures);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[HospitalWardEntitiesBuilder] Built {figures.Length} detailed ward entities and the lead doctor.");
    }

    private static void BuildEntityFigure(
        Transform parent, string name, Vector3 worldPos, Vector3 facing, float heightScale,
        Material coatMat, Material trimMat, Material gloveMat, Material maskMat, Material qMarkGlowMat,
        bool isLeadHandler, PatientBreathingAndIntro intro)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = worldPos;

        if (facing.sqrMagnitude > 0.001f)
        {
            root.transform.rotation = Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.z), Vector3.up);
        }

        root.transform.localScale = Vector3.one * heightScale;

        // Legs (under the coat)
        BuildPart(root.transform, "Leg_L", PrimitiveType.Capsule, new Vector3(-0.12f, 0.4f, 0f), Vector3.zero, new Vector3(0.15f, 0.45f, 0.15f), trimMat);
        BuildPart(root.transform, "Leg_R", PrimitiveType.Capsule, new Vector3(0.12f, 0.4f, 0f), Vector3.zero, new Vector3(0.15f, 0.45f, 0.15f), trimMat);

        // Torso / Medical Gown
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(root.transform);
        torso.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        torso.transform.localRotation = Quaternion.Euler(isLeadHandler ? 6f : 3f, 0f, 0f);

        BuildPart(torso.transform, "Chest", PrimitiveType.Capsule, new Vector3(0f, 0.25f, 0f), Vector3.zero, new Vector3(0.5f, 0.35f, 0.3f), coatMat);
        BuildPart(torso.transform, "Abdomen", PrimitiveType.Capsule, new Vector3(0f, -0.05f, 0f), Vector3.zero, new Vector3(0.45f, 0.3f, 0.25f), coatMat);
        BuildPart(torso.transform, "Collar", PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), Vector3.zero, new Vector3(0.25f, 0.05f, 0.2f), coatMat);

        // Head: a three dimensional glowing question mark instead of a face.
        GameObject head = new GameObject("Head_QuestionMark");
        head.transform.SetParent(torso.transform);
        head.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        head.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

        BuildPart(head.transform, "Q_Hook_Top", PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(0.18f, 0.08f, 0.05f), qMarkGlowMat);
        BuildPart(head.transform, "Q_Hook_L", PrimitiveType.Sphere, new Vector3(-0.08f, 0.02f, 0f), Vector3.zero, new Vector3(0.06f, 0.08f, 0.05f), qMarkGlowMat);
        BuildPart(head.transform, "Q_Hook_R", PrimitiveType.Sphere, new Vector3(0.08f, 0.02f, 0f), Vector3.zero, new Vector3(0.06f, 0.08f, 0.05f), qMarkGlowMat);
        BuildPart(head.transform, "Q_Stem", PrimitiveType.Cube, new Vector3(0f, -0.04f, 0f), new Vector3(0f, 0f, 15f), new Vector3(0.05f, 0.10f, 0.04f), qMarkGlowMat);
        BuildPart(head.transform, "Q_Dot", PrimitiveType.Sphere, new Vector3(0f, -0.13f, 0f), Vector3.zero, new Vector3(0.06f, 0.06f, 0.06f), qMarkGlowMat);

        // Arms
        Vector3 armLRot = isLeadHandler ? new Vector3(-30f, 15f, -10f) : new Vector3(-5f, 0f, -8f);
        Vector3 armRRot = isLeadHandler ? new Vector3(-30f, -15f, 10f) : new Vector3(-5f, 0f, 8f);

        GameObject armL = new GameObject("Arm_L");
        armL.transform.SetParent(torso.transform);
        armL.transform.localPosition = new Vector3(-0.25f, 0.45f, 0f);
        armL.transform.localRotation = Quaternion.Euler(armLRot);
        BuildPart(armL.transform, "UpperArm_L", PrimitiveType.Capsule, new Vector3(0f, -0.15f, 0f), Vector3.zero, new Vector3(0.12f, 0.18f, 0.12f), coatMat);
        BuildPart(armL.transform, "Forearm_L", PrimitiveType.Capsule, new Vector3(0.04f, -0.35f, isLeadHandler ? 0.15f : 0.02f), new Vector3(isLeadHandler ? 60f : 10f, 0f, -5f), new Vector3(0.10f, 0.18f, 0.10f), coatMat);
        BuildPart(armL.transform, "Hand_L", PrimitiveType.Sphere, new Vector3(0.06f, -0.48f, isLeadHandler ? 0.30f : 0.04f), Vector3.zero, new Vector3(0.08f, 0.08f, 0.10f), gloveMat);

        GameObject armR = new GameObject("Arm_R");
        armR.transform.SetParent(torso.transform);
        armR.transform.localPosition = new Vector3(0.25f, 0.45f, 0f);
        armR.transform.localRotation = Quaternion.Euler(armRRot);
        BuildPart(armR.transform, "UpperArm_R", PrimitiveType.Capsule, new Vector3(0f, -0.15f, 0f), Vector3.zero, new Vector3(0.12f, 0.18f, 0.12f), coatMat);
        BuildPart(armR.transform, "Forearm_R", PrimitiveType.Capsule, new Vector3(-0.04f, -0.35f, isLeadHandler ? 0.15f : 0.02f), new Vector3(isLeadHandler ? 60f : 10f, 0f, 5f), new Vector3(0.10f, 0.18f, 0.10f), coatMat);
        BuildPart(armR.transform, "Hand_R", PrimitiveType.Sphere, new Vector3(-0.06f, -0.48f, isLeadHandler ? 0.30f : 0.04f), Vector3.zero, new Vector3(0.08f, 0.08f, 0.10f), gloveMat);

        Collider[] primitiveColliders = root.GetComponentsInChildren<Collider>();
        for (int i = 0; i < primitiveColliders.Length; i++)
        {
            Object.DestroyImmediate(primitiveColliders[i]);
        }

        CapsuleCollider bodyCollider = root.AddComponent<CapsuleCollider>();
        bodyCollider.center = new Vector3(0f, 0.90f, 0f);
        bodyCollider.height = 1.80f;
        bodyCollider.radius = 0.30f;

        WardEntityIdle idle = root.AddComponent<WardEntityIdle>();
        idle.torsoTransform = torso.transform;
        idle.headTransform = head.transform;
        idle.phaseOffset = worldPos.x * 1.3f;

        if (isLeadHandler && intro != null)
        {
            intro.handlerRoot = root.transform;
            intro.handlerTorso = torso.transform;
            intro.handlerLeftArm = armL.transform;
            intro.handlerRightArm = armR.transform;
            intro.handlerIdle = idle;
        }
    }

    private static Transform CreatePart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = Quaternion.Euler(localEuler);
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part.transform;
    }

    private static void BuildPart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material material)
    {
        CreatePart(parent, name, type, localPos, localEuler, localScale, material);
    }

    private static Vector3 FindFreeSpot(Vector3 desired)
    {
        Physics.SyncTransforms();
        if (IsFree(desired)) return desired;

        Vector3[] offsets =
        {
            new Vector3(0.45f, 0f, 0f),
            new Vector3(-0.45f, 0f, 0f),
            new Vector3(0f, 0f, 0.45f),
            new Vector3(0f, 0f, -0.45f),
            new Vector3(0.45f, 0f, 0.45f),
            new Vector3(-0.45f, 0f, -0.45f),
            new Vector3(0.75f, 0f, 0f),
            new Vector3(0f, 0f, 0.75f)
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 candidate = desired + offsets[i];
            candidate.x = Mathf.Clamp(candidate.x, -4.35f, 4.35f);
            candidate.z = Mathf.Clamp(candidate.z, -4.35f, 4.35f);
            Physics.SyncTransforms();
            if (IsFree(candidate)) return candidate;
        }

        return desired;
    }

    private static bool IsFree(Vector3 position)
    {
        Vector3 bottom = position + Vector3.up * 0.35f;
        Vector3 top = position + Vector3.up * 1.55f;
        return !Physics.CheckCapsule(bottom, top, 0.34f, ~0, QueryTriggerInteraction.Ignore);
    }

    private static void WireToIntro(PatientBreathingAndIntro intro, Transform[] figures)
    {
        if (intro == null)
        {
            Debug.LogWarning("[HospitalWardEntitiesBuilder] PatientBreathingAndIntro was not found in the open scene; the doctors were built but not wired.");
            return;
        }

        Undo.RecordObject(intro, "Wire Ward Entities");
        intro.wardEntities = figures;
        EditorUtility.SetDirty(intro);
    }
}
