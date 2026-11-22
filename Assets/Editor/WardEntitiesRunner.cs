using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class WardEntitiesRunner
{
    private const string FlagPath = "Temp/run_ward_entities.flag";
    private const string ResultPath = "Temp/run_ward_entities.result";

    static WardEntitiesRunner()
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
            var scene = EditorSceneManager.GetActiveScene();
            HospitalWardEntitiesBuilder.BuildWardEntities();

            if (!string.IsNullOrEmpty(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
                result = "OK scene=" + scene.path;
            }
            else
            {
                result = "NO_SCENE_PATH scene=" + scene.name;
            }
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[WardEntitiesRunner] " + result);
    }
}
