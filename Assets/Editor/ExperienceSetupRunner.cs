using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class ExperienceSetupRunner
{
    private const string FlagPath = "Temp/run_experience_setup.flag";
    private const string ResultPath = "Temp/run_experience_setup.result";

    static ExperienceSetupRunner()
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

            GameObject bottle = GameObject.Find("Pill_Bottle");
            string diag = bottle == null
                ? "bottle=null"
                : $"bottle active={bottle.activeSelf} rb={(bottle.GetComponent<Rigidbody>() != null ? "YES" : "no")} childrenRigid={bottle.GetComponentsInChildren<Rigidbody>(true).Length}";

            if (bottle != null)
            {
                Rigidbody leftover = bottle.GetComponent<Rigidbody>();
                if (leftover != null) Object.DestroyImmediate(leftover);
                foreach (Rigidbody rb in bottle.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(rb);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            result = "OK scene=" + EditorSceneManager.GetActiveScene().path + " | " + diag;
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[ExperienceSetupRunner] " + result);
    }
}
