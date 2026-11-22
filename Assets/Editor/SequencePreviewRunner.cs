using System.IO;
using System.Reflection;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            if (intro != null)
            {
                SetPrivate(intro, "_sequenceTriggered", false);
                SetPrivate(intro, "_seqTime", 0f);

                if (intro.handlerRoot != null)
                {
                    intro.handlerRoot.position = new Vector3(-2.55f, 0f, -1.55f);
                    intro.handlerRoot.rotation = Quaternion.Euler(0f, 160f, 0f);
                    intro.handlerRoot.gameObject.SetActive(false);
                }

                if (intro.headsetAssembly != null)
                {
                    intro.headsetAssembly.gameObject.SetActive(false);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            result = "RESTORED";
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
