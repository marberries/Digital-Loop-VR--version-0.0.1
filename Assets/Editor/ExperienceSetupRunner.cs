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
                if (leftover != null)
                {
                    leftover.isKinematic = true;
                    leftover.useGravity = false;
                    leftover.constraints = RigidbodyConstraints.FreezeAll;
                    EditorUtility.SetDirty(leftover);
                }
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            string scenePath = EditorSceneManager.GetActiveScene().path;
            string text = System.IO.File.ReadAllText(scenePath);
            string disk = "kinematicOnDisk=" + text.Contains("m_IsKinematic: 1") + " rigidBodiesOnDisk=" + System.Text.RegularExpressions.Regex.Matches(text, "Rigidbody:").Count;
            result = "OK scene=" + scenePath + " | " + diag + " | " + disk;
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[ExperienceSetupRunner] " + result);
    }
}
