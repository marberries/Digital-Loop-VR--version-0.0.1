using System.IO;
using System.Reflection;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SequencePreviewRunner
{
    private const string FlagPath = "Temp/run_seq_preview.flag";
    private const string ResultPath = "Temp/run_seq_preview.result";

    static SequencePreviewRunner()
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
            PatientBreathingAndIntro intro = Object.FindAnyObjectByType<PatientBreathingAndIntro>(FindObjectsInactive.Include);
            if (intro == null)
            {
                result = "FAIL no intro";
            }
            else
            {
                if (intro.handlerRoot != null) intro.handlerRoot.gameObject.SetActive(true);
                SetPrivate(intro, "_sequenceTriggered", true);

                SetPrivate(intro, "_seqTime", 1.2f);
                Invoke(intro, "RunHandlerAndVisorSequence");
                RenderShot("seq_a_approach", new Vector3(0.9f, 1.5f, -2.9f), new Vector3(-1.9f, 1.35f, -2.2f), 50f);

                SetPrivate(intro, "_seqTime", 4.1f);
                Invoke(intro, "RunHandlerAndVisorSequence");
                RenderShot("seq_b_visor", new Vector3(0.9f, 1.5f, -2.9f), new Vector3(-1.75f, 1.3f, -2.5f), 50f);

                SetPrivate(intro, "_seqTime", 4.1f);
                Invoke(intro, "RunHandlerAndVisorSequence");
                RenderShot("seq_c_from_bed", new Vector3(-1.5f, 0.78f, -3.2f), new Vector3(-1.75f, 1.45f, -2.5f), 70f);

                result = "OK visorPos=" + (intro.headsetAssembly != null ? intro.headsetAssembly.position.ToString("F2") : "null") +
                         " handlerPos=" + (intro.handlerRoot != null ? intro.handlerRoot.position.ToString("F2") : "null");
            }
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[SequencePreviewRunner] " + result);
    }

    private static void SetPrivate(object target, string field, object value)
    {
        FieldInfo info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
        if (info != null) info.SetValue(target, value);
    }

    private static void Invoke(object target, string method)
    {
        MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
        if (info != null) info.Invoke(target, null);
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
