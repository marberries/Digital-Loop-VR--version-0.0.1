using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HospitalWardEntitiesBuilder
{
    [MenuItem("Tools/Hospital Ward/5. Build Doctors (White Coats & Glowing Question Marks)")]
    public static void BuildWardEntities()
    {
        PatientBreathingAndIntro intro = Object.FindAnyObjectByType<PatientBreathingAndIntro>(FindObjectsInactive.Include);
        BuildAllThreeQuestionMarkEntities(intro);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    public static void BuildAllThreeQuestionMarkEntities(PatientBreathingAndIntro intro)
    {
        GameObject oldGroup = GameObject.Find("Ward_QuestionMark_Entities");
        if (oldGroup != null) Object.DestroyImmediate(oldGroup);

        GameObject oldSingle = GameObject.Find("Mysterious_Handler");
        if (oldSingle != null) Object.DestroyImmediate(oldSingle);

        GameObject oldWardEntities = GameObject.Find("Ward_Entities");
        if (oldWardEntities != null) Object.DestroyImmediate(oldWardEntities);

        GameObject group = new GameObject("Ward_QuestionMark_Entities");

        // Real, opaque materials so the doctors read as people instead of black pills.
        Material whiteCoatMat = HospitalMaterials.CreateMaterial("Doc_WhiteCoat_Mat", new Color(0.92f, 0.93f, 0.94f), 0.18f, 0f);
        Material darkPantsMat = HospitalMaterials.CreateMaterial("Doc_DarkPants_Mat", new Color(0.16f, 0.17f, 0.20f), 0.28f, 0f);
        Material skinMat = HospitalMaterials.CreateMaterial("Doc_Skin_Mat", new Color(0.80f, 0.62f, 0.52f), 0.35f, 0f);
        Material qMarkGlowMat = HospitalMaterials.CreateEmissiveMaterial("Doc_QMark_Mat", Color.white, Color.white, 6.0f);

        Transform lead = BuildEntityFigure(
            group.transform, "Entity_Doctor_Lead", new Vector3(-2.55f, 0f, -1.55f), new Vector3(0f, 160f, 0f),
            whiteCoatMat, darkPantsMat, skinMat, qMarkGlowMat, true, intro);

        Transform nurse = BuildEntityFigure(
            group.transform, "Entity_Nurse_Monitor", new Vector3(-0.28f, 0f, -2.05f), new Vector3(0f, 215f, 0f),
            whiteCoatMat, darkPantsMat, skinMat, qMarkGlowMat, false, null);

        if (intro != null)
        {
            Undo.RecordObject(intro, "Wire Doctor Gaze Targets");
            intro.wardEntities = new[] { lead, nurse };
            EditorUtility.SetDirty(intro);
        }

        Debug.Log("[HospitalWardEntitiesBuilder] Built the lead doctor and the nurse with glowing question-mark heads.");
    }

    private static Transform BuildEntityFigure(
        Transform parent, string name, Vector3 worldPos, Vector3 eulerRot,
        Material coatMat, Material pantsMat, Material skinMat, Material qMarkMat,
        bool isLeadHandler, PatientBreathingAndIntro intro)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = worldPos;
        root.transform.rotation = Quaternion.Euler(eulerRot);

        // Legs
        BuildPart(root.transform, "Leg_L", PrimitiveType.Capsule, new Vector3(-0.15f, 0.45f, 0f), Vector3.zero, new Vector3(0.18f, 0.45f, 0.18f), pantsMat);
        BuildPart(root.transform, "Leg_R", PrimitiveType.Capsule, new Vector3(0.15f, 0.45f, 0f), Vector3.zero, new Vector3(0.18f, 0.45f, 0.18f), pantsMat);

        // Torso: a cube reads as a body, a capsule reads as a pill.
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(root.transform);
        torso.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        torso.transform.localRotation = Quaternion.Euler(isLeadHandler ? 6f : 3f, 0f, 0f);

        BuildPart(torso.transform, "Chest_Cube", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), Vector3.zero, new Vector3(0.5f, 0.6f, 0.3f), coatMat);
        BuildPart(torso.transform, "Lab_Coat_Tail", PrimitiveType.Cube, new Vector3(0f, -0.12f, 0f), Vector3.zero, new Vector3(0.46f, 0.45f, 0.28f), coatMat);

        // Arms
        GameObject armL = new GameObject("Arm_L");
        armL.transform.SetParent(torso.transform);
        armL.transform.localPosition = new Vector3(-0.35f, 0.45f, 0f);
        armL.transform.localRotation = Quaternion.Euler(isLeadHandler ? -30f : -5f, 15f, -10f);
        BuildPart(armL.transform, "UpperArm_L", PrimitiveType.Capsule, new Vector3(0f, -0.15f, 0f), Vector3.zero, new Vector3(0.115f, 0.2f, 0.115f), coatMat);
        BuildPart(armL.transform, "Forearm_L", PrimitiveType.Capsule, new Vector3(0.04f, -0.35f, isLeadHandler ? 0.15f : 0.02f), new Vector3(isLeadHandler ? 60f : 10f, 0f, -5f), new Vector3(0.095f, 0.2f, 0.095f), coatMat);
        BuildPart(armL.transform, "Hand_L", PrimitiveType.Sphere, new Vector3(0.05f, isLeadHandler ? -0.47f : -0.55f, 0.02f), Vector3.zero, new Vector3(0.09f, 0.09f, 0.11f), skinMat);

        GameObject armR = new GameObject("Arm_R");
        armR.transform.SetParent(torso.transform);
        armR.transform.localPosition = new Vector3(0.35f, 0.45f, 0f);
        armR.transform.localRotation = Quaternion.Euler(isLeadHandler ? -30f : -5f, -15f, 10f);
        BuildPart(armR.transform, "UpperArm_R", PrimitiveType.Capsule, new Vector3(0f, -0.15f, 0f), Vector3.zero, new Vector3(0.115f, 0.2f, 0.115f), coatMat);
        BuildPart(armR.transform, "Forearm_R", PrimitiveType.Capsule, new Vector3(-0.04f, -0.35f, isLeadHandler ? 0.15f : 0.02f), new Vector3(isLeadHandler ? 60f : 10f, 0f, 5f), new Vector3(0.095f, 0.2f, 0.095f), coatMat);
        BuildPart(armR.transform, "Hand_R", PrimitiveType.Sphere, new Vector3(-0.05f, isLeadHandler ? -0.47f : -0.55f, 0.02f), Vector3.zero, new Vector3(0.09f, 0.09f, 0.11f), skinMat);

        // Head: a large glowing question mark that always turns to the player.
        GameObject head = new GameObject("Head_QuestionMark");
        head.transform.SetParent(torso.transform);
        head.transform.localPosition = new Vector3(0f, 0.78f, 0f);
        head.AddComponent<LookAtPlayer>();

        BuildQuestionMark(head.transform, qMarkMat);

        Collider[] primitiveColliders = root.GetComponentsInChildren<Collider>();
        for (int i = 0; i < primitiveColliders.Length; i++)
        {
            Object.DestroyImmediate(primitiveColliders[i]);
        }

        CapsuleCollider bodyCollider = root.AddComponent<CapsuleCollider>();
        bodyCollider.center = new Vector3(0f, 0.95f, 0f);
        bodyCollider.height = 1.9f;
        bodyCollider.radius = 0.32f;

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

        return root.transform;
    }

    /// <summary>
    /// Builds a roughly half a metre tall "?" out of glowing blobs: a curved hook,
    /// a short stem and a dot, all sitting in the head's local XY plane.
    /// </summary>
    private static void BuildQuestionMark(Transform parent, Material glowMaterial)
    {
        const int segments = 14;
        const float radius = 0.19f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(205f, -25f, i / (float)segments) * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius, 0.16f + Mathf.Sin(angle) * radius, 0f);
            BuildPart(parent, $"Q_Hook_{i:00}", PrimitiveType.Sphere, point, Vector3.zero, Vector3.one * 0.085f, glowMaterial);
        }

        BuildPart(parent, "Q_Stem", PrimitiveType.Capsule, new Vector3(0.14f, -0.01f, 0f), new Vector3(0f, 0f, 20f), new Vector3(0.08f, 0.09f, 0.08f), glowMaterial);
        BuildPart(parent, "Q_Dot", PrimitiveType.Sphere, new Vector3(0.05f, -0.20f, 0f), Vector3.zero, Vector3.one * 0.1f, glowMaterial);
    }

    private static GameObject BuildPart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = Quaternion.Euler(localEuler);
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part;
    }
}
