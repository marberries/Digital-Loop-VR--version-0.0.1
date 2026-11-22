using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DigitalCurtain.Hospital
{
    /// <summary>
    /// Small session wrapper: once the visor sequence is over the screen goes
    /// quiet and a faint line invites the player to wake up again (R restarts).
    /// </summary>
    public class ExperienceDirector : MonoBehaviour
    {
        public PatientBreathingAndIntro intro;
        public Text endingText;
        public float endingDelay = 13.5f;
        public string endingLine = "— signal lost —";
        public string restartLine = "press R to wake again";
        public float fadeSpeed = 0.9f;

        private float _fade;
        private bool _ended;

        private void Update()
        {
            if (_ended && RestartPressed())
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
                return;
            }

            if (endingText == null || intro == null) return;

            if (intro.SequenceRunning && intro.SequenceTime > endingDelay)
            {
                _ended = true;
            }

            _fade = _ended
                ? Mathf.MoveTowards(_fade, 1f, fadeSpeed * Time.deltaTime)
                : Mathf.MoveTowards(_fade, 0f, fadeSpeed * Time.deltaTime);

            Color color = endingText.color;
            color.a = _fade;
            endingText.color = color;

            if (_fade > 0.02f)
            {
                endingText.text = endingLine + "\n<size=18>" + restartLine + "</size>";
            }
            else
            {
                endingText.text = "";
            }
        }

        private static bool RestartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Keyboard.current != null
                   && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }
    }
}
