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

    private static PatientBreathingAndIntro _intro;
    private static Transform _rig;
    private static Transform _lead;
    private static Transform _nurse;
    private static float _start = -1f;
    private static bool _started;
    private static int _errors;

    static DoctorSetupAndShotRunner()
    {
        if (!File.Exists(FlagPath)) return;
        File.Delete(FlagPath);
        Application.logMessageReceived += (c, s, t) => { if (t == LogType.Error || t == LogType.Exception) _errors++; };
        EditorApplication.delayCall += Build;
    }

    private static void Build()
    {
        string result;
        try
        {
            HospitalExperienceSetup.BuildPlayableExperience();
            result = "SETUP_OK";
        }
        catch (System.Exception exception)
        {
            File.WriteAllText(ResultPath, "SETUP_FAIL " + exception);
            return;
        }

        Debug.Log("[DoctorSetupAndShotRunner] " + result);
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying)
        {
            if (_started) Finish("LEFT_EARLY");
            return;
        }

        if (!_started)
        {
            _started = true;
            _start = (float)EditorApplication.timeSinceStartup;
            _intro = Object.FindAnyObjectByType<PatientBreathingAndIntro>();
            GameObject offset = GameObject.Find("Camera Offset");
            if (offset != null)
            {
                _rig = offset.transform;
                PatientLookController look = _rig.GetComponent<PatientLookController>();
                if (look != null) look.enabled = false;
            }

            GameObject lead = GameObject.Find("Entity_Doctor_Lead");
            if (lead != null) _lead = lead.transform;
            GameObject nurse = GameObject.Find("Entity_Nurse_Monitor");
            if (nurse != null) _nurse = nurse.transform;
            return;
        }

        float t = (float)EditorApplication.timeSinceStartup - _start;

        if (t < 1.0f && _rig != null)
        {
            _rig.rotation = Quaternion.Euler(15f, 0f, 0f);
        }
        if (t > 1.0f && t < 2.4f) Aim(_lead);
        if (t > 2.4f) Aim(_nurse);

        if (t > 1.2f) Shot("doctor_01_default");
        if (t > 2.0f) Shot("doctor_02_lead");
        if (t > 3.2f) Shot("doctor_03_nurse");
        if (t > 4.5f) Finish("DONE");
        if (t > 20f) Finish("TIMEOUT");
    }

    private static void Aim(Transform target)
    {
        if (_rig == null || target == null || _intro == null) return;
        Vector3 point = target.position + Vector3.up * 1.55f;
        Vector3 direction = point - _intro.transform.position;
        if (direction.sqrMagnitude < 0.001f) return;
        _rig.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private static void Shot(string name)
    {
        if (File.Exists($"Temp/{name}.png")) return;
        ScreenCapture.CaptureScreenshot($"Temp/{name}.png");
    }

    private static void Finish(string reason)
    {
        EditorApplication.update -= Tick;
        File.WriteAllText(ResultPath, reason + " errors=" + _errors);
        Debug.Log("[DoctorSetupAndShotRunner] " + reason + " errors=" + _errors);
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        _started = false;
    }
}
