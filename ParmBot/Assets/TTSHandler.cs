using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace TTSHandling
{
    public class TTSHandler : MonoBehaviour
    {
        private string voiceName = "Microsoft Sabina Desktop";

        [Header("Talking Animation")]
        [SerializeField] private Animator characterAnimator;

        private Process _ttsProcess;
        private bool isSpeaking = false;

        private readonly object processLock = new object();

        private void Awake()
        {
            characterAnimator = GameObject.FindWithTag("ParmSprite").GetComponent<Animator>();
        }

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            StopSpeaking();

            SetTalking(true);
            isSpeaking = true;

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                string safe = text.Replace("'", "");
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"Add-Type -AssemblyName System.Speech; " +
                                $"$s = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                                $"$s.SelectVoice('{voiceName}'); " +
                                $"$s.Speak('{safe}');\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process newProcess = Process.Start(psi);
                lock (processLock)
                {
                    _ttsProcess = newProcess;
                }

                newProcess?.WaitForExit();

                lock (processLock)
                {
                    if (_ttsProcess == newProcess)
                        isSpeaking = false;
                }
            });
        }

        private void Update()
        {
            if (!isSpeaking && characterAnimator != null)
            {
                if (characterAnimator.GetBool("isTalking"))
                    SetTalking(false);
            }
        }

        private void SetTalking(bool talking)
        {
            if (characterAnimator != null)
            {
                Debug.Log($"isTalking set to {talking}");
                characterAnimator.SetBool("isTalking", talking);
            }
        }

        public void StopSpeaking()
        {
            Process toKill = null;
            lock (processLock)
            {
                toKill = _ttsProcess;
                _ttsProcess = null;
            }

            if (toKill != null && !toKill.HasExited)
            {
                toKill.Kill();
                toKill.WaitForExit();
                toKill.Dispose();
            }

            isSpeaking = false;
            SetTalking(false);
        }

        private void OnDestroy()
        {
            StopSpeaking();
        }
    }
}