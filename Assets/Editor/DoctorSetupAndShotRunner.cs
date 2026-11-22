using System.IO;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class DoctorSetupAndShotRunner
{
    private const string FlagPath = "Temp/run_doctor_setup.flag";
    private const string ResultPath = "Temp/run_doctor_setup.result";

    static DoctorSetupAndShotRunner()
    {
        if (!File.Exists(FlagPath)) return;
        File.Delete(FlagPath);
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        string result;
        try
        {
            HospitalExperienceSetup.BuildPlayableExperience();

            GameObject lead = GameObject.Find("Entity_Doctor_Lead");
            GameObject nurse = GameObject.Find("Entity_Nurse_Monitor");
            GameObject head = lead != null ? FindChild(lead.transform, "Head_QuestionMark") : null;

            RenderShot("doctor_01_lead", new Vector3(-1.5f, 0.78f, -3.2f), LookAt(new Vector3(-2.55f, 1.72f, -1.55f), new Vector3(-1.5f, 0.78f, -3.2f)), 55f);
            RenderShot("doctor_02_nurse", new Vector3(-1.5f, 0.78f, -3.2f), LookAt(new Vector3(-0.28f, 1.72f, -2.05f), new Vector3(-1.5f, 0.78f, -3.2f)), 55f);
            RenderShot("doctor_03_wide", new Vector3(-1.5f, 0.78f, -3.2f), Quaternion.Euler(8f, 0f, 0f), 75f);
            RenderShot("doctor_04_profile", new Vector3(0.3f, 1.3f, -0.4f), LookAt(new Vector3(-2.55f, 1.4f, -1.55f), new Vector3(0.3f, 1.3f, -0.4f)), 40f);

            result = "OK lead=" + (lead != null) + " nurse=" + (nurse != null) + " head=" + (head != null) +
                     " lookAt=" + (head != null && head.GetComponent<LookAtPlayer>() != null);
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[DoctorSetupAndShotRunner] " + result);
    }

    private static GameObject FindChild(Transform root, string name)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name) return all[i].gameObject;
        }
        return null;
    }

    private static Quaternion LookAt(Vector3 target, Vector3 from)
    {
        Vector3 direction = target - from;
        return direction.sqrMagnitude < 0.001f ? Quaternion.identity : Quaternion.LookRotation(direction, Vector3.up);
    }

    private static void RenderShot(string name, Vector3 position, Quaternion rotation, float fieldOfView)
    {
        GameObject cameraObject = new GameObject("Preview_Camera");
        cameraObject.transform.position = position;
        cameraObject.transform.rotation = rotation;

        Camera previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.fieldOfView = fieldOfView;
        previewCamera.nearClipPlane = 0.02f;
        previewCamera.farClipPlane = 60f;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.black;

        RenderTexture target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
        target.Create();
        previewCamera.targetTexture = target;
        previewCamera.Render();

        RenderTexture.active = target;
        Texture2D shot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        shot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        shot.Apply();
        RenderTexture.active = null;

        File.WriteAllBytes($"Temp/{name}.png", shot.EncodeToPNG());

        previewCamera.targetTexture = null;
        Object.DestroyImmediate(shot);
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(cameraObject);
    }
}
