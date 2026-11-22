using System.Collections.Generic;
using System.IO;
using System.Text;
using DigitalCurtain.Hospital;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            if (_started)
            {
                Finish("PLAY_MODE_LEFT_EARLY");
            }
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

        if (t > 0.6f && (_intro == null || !_intro.SequenceRunning)) AimAtFigure();
        if (t > 1.5f) ScreenshotOnce(t, 1.5f, "play_01_wake");
        if (t > 4.0f) ScreenshotOnce(t, 4.0f, "play_02_room");

        if (_intro != null && t - _lastSample > 1f)
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
            if (s > 1.0f) ScreenshotOnce(t, _sequenceStart + 1.0f, "play_03_handler");
            if (s > 4.0f) ScreenshotOnce(t, _sequenceStart + 4.0f, "play_04_visor_hands");
            if (s > 7.0f) ScreenshotOnce(t, _sequenceStart + 7.0f, "play_05_static");
            if (s > 13.5f) ScreenshotOnce(t, _sequenceStart + 13.5f, "play_06_inside");
            if (s > 19.5f) ScreenshotOnce(t, _sequenceStart + 19.5f, "play_07_endcard");
            if (s > 23.0f) Finish("DONE");
        }

        if (t > 60f) Finish("TIMEOUT");
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

    private static void AimAtFigure()
    {
        if (_intro == null || _rigNode == null || _target == null) return;

        Camera camera = _intro.GetComponent<Camera>();
        if (camera == null) return;

        Vector3 head = _target.position + Vector3.up * 1.2f;
        Vector3 direction = head - camera.transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        _rigNode.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private static void ScreenshotOnce(float now, float threshold, string name)
    {
        if (now < threshold) return;

        string key = name;
        if (Shots.Contains(key)) return;

        Shots.Add(key);
        string path = $"Temp/{name}.png";
        ScreenCapture.CaptureScreenshot(path);
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

// playtest reload marker 2
