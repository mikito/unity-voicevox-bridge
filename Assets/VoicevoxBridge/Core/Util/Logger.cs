using UnityEngine;

namespace VoicevoxBridge
{
    public class Logger
    {
        public bool enable = true;

        public void Log(string text)
        {
            if (enable) Debug.Log($"[VOICEVOX] {text}");
        }
    }
}