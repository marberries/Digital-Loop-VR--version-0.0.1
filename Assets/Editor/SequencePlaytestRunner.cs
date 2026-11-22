using System.IO;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SequencePlaytestRunner
{
    private const string FlagPath = "Temp/run_seq_test.flag";
    private const string ResultPath = "Temp/run_seq_test.result";

    private static PatientBreathingAndIntro _intro;
    private static Transform _rig;
    private static Transform _target;
    private static float _start = -1f;
    private static bool _started;
    private static float _seqStart = -1f;
    private static int _errors;

    static SequencePlaytestRunner()
    {
        if (!File.Exists(FlagPath)) return;
        File.Delete(FlagPath);
        Application.logMessageReceived += (c, s, t) => { if (t == LogType.Error || t == LogType.Exception) _errors++; };
        EditorApplication.delayCall += Begin;
    }

    private static void Begin()
    {
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
            if (offset != null) _rig = offset.transform;

            if (_intro != null)
            {
                _intro.sequenceEarliestTime = 6f;
                _intro.sequenceLatestTime = 8f;
            }

            GameObject figure = GameObject.Find("Ward_Entity_Footboard");
            if (figure != null) _target = figure.transform;
            else
            {
                WardEntityIdle idle = Object.FindAnyObjectByType<WardEntityIdle>();
                if (idle != null) _target = idle.transform;
            }
            return;
        }

        float t = (float)EditorApplication.timeSinceStartup - _start;

        if (_rig != null && _target != null && (_intro == null || !_intro.SequenceRunning))
        {
            Vector3 dir = _target.position + Vector3.up * 1.3f - _intro.transform.position;
            if (dir.sqrMagnitude > 0.001f) _rig.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        if (_intro != null && _intro.SequenceRunning && _seqStart < 0f)
        {
            _seqStart = t;
        }

        string state = _intro == null
            ? "no intro"
            : $"t={t:F1} unease={_intro.Unease:F2} seq={_intro.SequenceRunning} seqT={_intro.SequenceTime:F1}";

        if (t > 1.5f) Shot("seq_01_room");
        if (_seqStart > 0f && t - _seqStart > 1.2f) Shot("seq_02_handler");
        if (_seqStart > 0f && t - _seqStart > 4.2f) Shot("seq_03_visor");
        if (_seqStart > 0f && t - _seqStart > 7.5f) Shot("seq_04_static");

        if (t > 16f || (_seqStart > 0f && t - _seqStart > 10f))
        {
            Finish("DONE | " + state);
            return;
        }

        if (t > 30f) Finish("TIMEOUT | " + state);
    }

    private static void Shot(string name)
    {
        if (File.Exists($"Temp/{name}.png")) return;
        ScreenCapture.CaptureScreenshot($"Temp/{name}.png");
    }

    private static void Finish(string reason)
    {
        EditorApplication.update -= Tick;
        File.WriteAllText(ResultPath, reason + " | errors=" + _errors);
        Debug.Log("[SequencePlaytestRunner] " + reason + " errors=" + _errors);
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        _started = false;
    }
}
