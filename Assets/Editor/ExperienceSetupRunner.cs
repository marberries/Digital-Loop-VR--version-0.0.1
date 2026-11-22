using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
            result = "OK scene=" + EditorSceneManager.GetActiveScene().path;
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[ExperienceSetupRunner] " + result);
    }
}
