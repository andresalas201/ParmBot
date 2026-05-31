using UnityEngine;
using System.Diagnostics;

namespace TTSHandling
{
    public class TTSHandler : MonoBehaviour
    {
        private string voiceName = "Microsoft Sabina Desktop";
        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

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
                Process.Start(psi)?.WaitForExit();
            });
        }
    }
}