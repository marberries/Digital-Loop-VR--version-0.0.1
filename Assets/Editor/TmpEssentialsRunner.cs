using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TmpEssentialsRunner
{
    private const string FlagPath = "Temp/run_tmp_import.flag";
    private const string ResultPath = "Temp/run_tmp_import.result";

    static TmpEssentialsRunner()
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
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            result = "OK font=" + (font != null ? font.name : "null") +
                     " settings=" + (TMP_Settings.instance != null);
        }
        catch (System.Exception exception)
        {
            result = "FAIL " + exception;
        }

        File.WriteAllText(ResultPath, result);
        Debug.Log("[TmpEssentialsRunner] " + result);
    }
}
