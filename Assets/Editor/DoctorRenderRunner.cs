using System.IO;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DoctorRenderRunner
{
    private const string FlagPath = "Temp/run_doctor_render.flag";
    private const string ResultPath = "Temp/run_doctor_render.result";

    static DoctorRenderRunner()
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
            Transform leadHead = FindDeep(lead, "Head_QuestionMark");
            Transform nurseHead = FindDeep(nurse, "Head_QuestionMark");

            Vector3 eye = new Vector3(-1.5f, 0.78f, -4.0f);
            if (leadHead != null)
            {
                RenderShot("doc_01_face", leadHead.position + new Vector3(0f, 0f, 1.9f), leadHead.position, 45f);
                RenderShot("doc_02_eye", eye, leadHead.position, 62f);
            }

            if (nurseHead != null)
            {
                RenderShot("doc_03_nurse", nurseHead.position + new Vector3(-1.6f, 0.15f, 1.6f), nurseHead.position, 45f);
            }

            RenderShot("doc_04_wide", eye, new Vector3(-1.2f, 1.5f, -2.0f), 75f);

            result = "OK leadHead=" + (leadHead != null) + " nurseHead=" + (nurseHead != null);
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[DoctorRenderRunner] " + result);
    }

    private static Transform FindDeep(GameObject root, string name)
    {
        if (root == null) return null;
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name) return all[i];
        }
        return null;
    }

    private static void RenderShot(string name, Vector3 position, Vector3 lookAt, float fieldOfView)
    {
        GameObject cameraObject = new GameObject("Preview_Camera");
        cameraObject.transform.position = position;
        cameraObject.transform.rotation = Quaternion.LookRotation(lookAt - position, Vector3.up);

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
