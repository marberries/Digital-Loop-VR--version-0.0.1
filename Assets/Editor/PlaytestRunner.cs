using System.Collections.Generic;
using System.IO;
using System.Text;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlaytestRunner
{
    private const string FlagPath = "Temp/run_playtest.flag";
    private const string ResultPath = "Temp/run_playtest.result";

    private static PatientBreathingAndIntro _intro;
    private static Transform _rigNode;
    private static Transform _target;
    private static float _startTime = -1f;
    private static bool _started;
    private static bool _sequenceSeen;
    private static float _sequenceStart;
    private static int _errorCount;
    private static int _exceptionCount;
    private static readonly List<string> Shots = new List<string>();
    private static readonly List<string> Samples = new List<string>();
    private static float _lastSample;

    static PlaytestRunner()
    {
        if (!File.Exists(FlagPath)) return;

        File.Delete(FlagPath);
        Application.logMessageReceived += OnLog;
        EditorApplication.delayCall += Begin;
    }

    private static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception) _exceptionCount++;
        else if (type == LogType.Error) _errorCount++;
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
            if (_started) Finish("PLAY_MODE_LEFT_EARLY");
            return;
        }

        if (!_started)
        {
            _started = true;
            _startTime = (float)EditorApplication.timeSinceStartup;
            ResolveReferences();
            return;
        }

        float t = (float)EditorApplication.timeSinceStartup - _startTime;

        if (t < 0.6f && _rigNode != null) _rigNode.rotation = Quaternion.Euler(15f, 0f, 0f);
        if (t > 0.3f) ScreenshotOnce(t, 0.3f, "play_00_default_view");
        AimAtTarget(t);
        if (t > 2.0f) ScreenshotOnce(t, 2.0f, "play_01_wake");
        if (t > 6.0f) ScreenshotOnce(t, 6.0f, "play_02_room");
        if (t > 12.0f) ScreenshotOnce(t, 12.0f, "play_03_figures");

        if (_intro != null && t - _lastSample > 2f)
        {
            _lastSample = t;
            Samples.Add($"{t:F1}s unease={_intro.Unease:F2} seq={_intro.SequenceRunning} seqT={_intro.SequenceTime:F1}");
        }

        if (_intro != null && _intro.SequenceRunning && !_sequenceSeen)
        {
            _sequenceSeen = true;
            _sequenceStart = t;
        }

        if (_sequenceSeen)
        {
            float s = t - _sequenceStart;
            if (s > 1.0f) ScreenshotOnce(t, _sequenceStart + 1.0f, "play_04_handler");
            if (s > 4.0f) ScreenshotOnce(t, _sequenceStart + 4.0f, "play_05_visor");
            if (s > 7.0f) ScreenshotOnce(t, _sequenceStart + 7.0f, "play_06_static");
            if (s > 19.0f) ScreenshotOnce(t, _sequenceStart + 19.0f, "play_07_endcard");
            if (s > 21.0f) Finish("DONE");
        }

        if (t > 70f) Finish("TIMEOUT");
    }

    private static void ResolveReferences()
    {
        _intro = Object.FindAnyObjectByType<PatientBreathingAndIntro>();
        GameObject offset = GameObject.Find("Camera Offset");
        if (offset != null)
        {
            _rigNode = offset.transform;
            PatientLookController look = _rigNode.GetComponent<PatientLookController>();
            if (look != null) look.enabled = false;
        }

        string[] candidates =
        {
            "Ward_Entity_Footboard",
            "Ward_Entity_Bedside_Left",
            "Ward_Entity_Right_Wall",
            "Ward_Entity_Back_Wall"
        };

        for (int i = 0; i < candidates.Length && _target == null; i++)
        {
            GameObject figure = GameObject.Find(candidates[i]);
            if (figure != null) _target = figure.transform;
        }
    }

    private static void AimAtTarget(float t)
    {
        if (_intro == null || _rigNode == null) return;

        Camera camera = _intro.GetComponent<Camera>();
        if (camera == null) return;

        Transform aim = _intro.SequenceRunning && _intro.handlerTorso != null ? _intro.handlerTorso : _target;
        if (aim == null) return;

        Vector3 point = aim.position + Vector3.up * (aim == _target ? 1.35f : 0.35f);
        Vector3 direction = point - camera.transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        _rigNode.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private static void ScreenshotOnce(float now, float threshold, string name)
    {
        if (now < threshold) return;
        if (Shots.Contains(name)) return;

        Shots.Add(name);
        ScreenCapture.CaptureScreenshot($"Temp/{name}.png");
    }

    private static void Finish(string reason)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("result=" + reason);
        sb.AppendLine("errors=" + _errorCount + " exceptions=" + _exceptionCount);
        sb.AppendLine("sequenceSeen=" + _sequenceSeen);
        sb.AppendLine("shots=" + string.Join(",", Shots));
        sb.AppendLine("samples:");
        for (int i = 0; i < Samples.Count; i++) sb.AppendLine("  " + Samples[i]);

        File.WriteAllText(ResultPath, sb.ToString());
        Debug.Log("[PlaytestRunner] " + sb);

        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        _started = false;
    }
}

// marker 5
