using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HospitalWardEntitiesBuilder
{
    [MenuItem("Tools/Hospital Ward/5. Build Ward Entities (Idle Watchers)")]
    public static void BuildWardEntities()
    {
        Material gownMat = HospitalMaterials.CreateMaterial("Ward_Entity_Gown_Mat", new Color(0.34f, 0.39f, 0.38f), 0.16f, 0.0f);
        Material skinMat = HospitalMaterials.CreateMaterial("Ward_Entity_Skin_Mat", new Color(0.60f, 0.61f, 0.58f), 0.10f, 0.0f);

        GameObject previousRoot = GameObject.Find("Ward_Entities");
        if (previousRoot != null)
        {
            Object.DestroyImmediate(previousRoot);
        }

        GameObject root = new GameObject("Ward_Entities");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        Vector3 lookTarget = new Vector3(-1.50f, 1.20f, -3.80f);
        GameObject bed = GameObject.Find("Hospital_Bed_Assembly");
        if (bed != null) lookTarget = bed.transform.position + Vector3.up * 1.20f;

        Vector3[] slots =
        {
            new Vector3(-3.55f, 0f, -2.35f),
            new Vector3( 0.30f, 0f, -1.62f),
            new Vector3( 3.25f, 0f,  0.85f),
            new Vector3(-0.55f, 0f,  3.25f)
        };

        string[] names =
        {
            "Ward_Entity_Footboard",
            "Ward_Entity_Bedside",
            "Ward_Entity_Window",
            "Ward_Entity_Wardrobe"
        };

        Transform[] figures = new Transform[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            Vector3 freeSpot = FindFreeSpot(slots[i]);
            float heightScale = 1f + ((i % 3) - 1) * 0.025f;
            figures[i] = BuildFigure(root.transform, names[i], freeSpot, lookTarget, heightScale, gownMat, skinMat);
        }

        WireToIntro(figures);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[HospitalWardEntitiesBuilder] Built {figures.Length} idle ward entities and wired them to PatientBreathingAndIntro.");
    }

    private static Transform BuildFigure(Transform parent, string name, Vector3 position, Vector3 lookTarget, float heightScale, Material gownMat, Material skinMat)
    {
        GameObject figure = new GameObject(name);
        figure.transform.SetParent(parent, false);
        figure.transform.position = position;
        figure.transform.rotation = Quaternion.LookRotation(
            new Vector3(lookTarget.x, position.y, lookTarget.z) - position,
            Vector3.up);
        figure.transform.localScale = Vector3.one * heightScale;

        Transform torso = CreatePart(figure.transform, "Body", PrimitiveType.Capsule,
            new Vector3(0f, 1.16f, 0f), Vector3.zero, new Vector3(0.36f, 0.32f, 0.26f), gownMat);

        CreatePart(figure.transform, "Hips", PrimitiveType.Cube,
            new Vector3(0f, 0.88f, 0f), Vector3.zero, new Vector3(0.30f, 0.18f, 0.22f), gownMat);

        CreatePart(figure.transform, "Leg_L", PrimitiveType.Capsule,
            new Vector3(-0.10f, 0.42f, 0f), Vector3.zero, new Vector3(0.13f, 0.42f, 0.13f), gownMat);

        CreatePart(figure.transform, "Leg_R", PrimitiveType.Capsule,
            new Vector3(0.10f, 0.42f, 0f), Vector3.zero, new Vector3(0.13f, 0.42f, 0.13f), gownMat);

        CreatePart(figure.transform, "Arm_L", PrimitiveType.Capsule,
            new Vector3(-0.25f, 1.11f, 0.01f), new Vector3(0f, 0f, 7f), new Vector3(0.09f, 0.33f, 0.09f), gownMat);

        CreatePart(figure.transform, "Arm_R", PrimitiveType.Capsule,
            new Vector3(0.25f, 1.11f, 0.01f), new Vector3(0f, 0f, -7f), new Vector3(0.09f, 0.33f, 0.09f), gownMat);

        CreatePart(figure.transform, "Neck", PrimitiveType.Cylinder,
            new Vector3(0f, 1.50f, 0f), Vector3.zero, new Vector3(0.07f, 0.04f, 0.07f), skinMat);

        Transform head = CreatePart(figure.transform, "Head", PrimitiveType.Sphere,
            new Vector3(0f, 1.64f, 0f), Vector3.zero, new Vector3(0.19f, 0.22f, 0.20f), skinMat);

        Collider[] primitiveColliders = figure.GetComponentsInChildren<Collider>();
        for (int i = 0; i < primitiveColliders.Length; i++)
        {
            Object.DestroyImmediate(primitiveColliders[i]);
        }

        CapsuleCollider bodyCollider = figure.AddComponent<CapsuleCollider>();
        bodyCollider.center = new Vector3(0f, 0.90f, 0f);
        bodyCollider.height = 1.80f;
        bodyCollider.radius = 0.28f;

        WardEntityIdle idle = figure.AddComponent<WardEntityIdle>();
        idle.headBone = head;
        idle.torsoBone = torso;

        return figure.transform;
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

    private static void WireToIntro(Transform[] figures)
    {
        PatientBreathingAndIntro intro = Object.FindAnyObjectByType<PatientBreathingAndIntro>(FindObjectsInactive.Include);
        if (intro == null)
        {
            Debug.LogWarning("[HospitalWardEntitiesBuilder] PatientBreathingAndIntro was not found in the open scene; ward entities were built but not wired.");
            return;
        }

        Undo.RecordObject(intro, "Wire Ward Entities");
        intro.wardEntities = figures;
        EditorUtility.SetDirty(intro);
    }
}
